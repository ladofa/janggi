using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.StoneHelper;

namespace Janggi.Ai
{
	public class PrimaryUcb : Mcts.IHandlers
	{
		public PrimaryUcb()
		{
			MaxRolloutDepth = 100;
			ExplorationRate = 0.7f;
		}

		public int MaxRolloutDepth
		{
			set;
			get;
		}

		public float ExplorationRate
		{
			set;
			get;
		}

		void moveRandomNext(Board board)
		{
			List<Move> moves = board.GetAllPossibleMoves();

			//int k = Global.Rand.Next(moves.Count);
			//MoveNext(moves[k]);
			//return;

			int[] proms = new int[moves.Count];

			Func<uint, uint, uint, uint, int> Judge;
			if (board.IsMyTurn)
			{
				Judge = (stoneFrom, stoneTo, targetTo, targetFrom) =>
				{
					//일단 상대를 따먹으면 10점
					int takingPoint = GetPoint(stoneTo);
					int dodgePoint = IsYours(targetFrom) ? 10 : 0;
					return takingPoint + dodgePoint + ((IsYours(targetTo) ? GetPoint(stoneFrom) : 0) + (takingPoint != 0 ? 10 : 0));
				};
			}
			else
			{
				Judge = (stoneFrom, stoneTo, targetTo, targetFrom) =>
				{
					int takingPoint = -GetPoint(stoneTo);
					int dodgePoint = IsMine(targetFrom) ? 10 : 0;
					return takingPoint + ((IsMine(targetTo) ? -GetPoint(stoneFrom) : 0) + (takingPoint != 0 ? 10 : 0));
				};
			}

			//최소 점수
			int min = int.MaxValue;
			int sum = 0;

			//마지막 rest빼고.
			for (int i = 0; i < moves.Count - 1; i++)
			{
				Move move = moves[i];
				uint stoneFrom = board[move.From];
				uint stoneTo = board[move.To];
				uint targetTo = board.targets[move.To.Y, move.To.X];
				uint targetFrom = board.targets[move.From.Y, move.From.X];

				int judge = Judge(stoneFrom, stoneTo, targetTo, targetFrom);
				proms[i] = judge;

				if (judge < min)
				{
					min = judge;
				}

				sum += judge;
			}

			sum += (-min + 10) * proms.Length;

			int prob = Global.Rand.Next(sum);

			bool oldMyTurn = board.IsMyTurn;
			int cumm = 0;
			for (int i = 0; i < proms.Length; i++)
			{
				proms[i] = proms[i] - min + 10;
				cumm += proms[i];

				if (prob < cumm)
				{
					board.MoveNext(moves[i]);
					return;
				}
			}

			throw new Exception("??");
		}

		public bool Rollout(Node node)
		{
			//rollout
			Board rollout = new Board(node.board);

			//100수까지만 하자 혹시나.
			for (int i = 0; i < 100; i++)
			{
				moveRandomNext(rollout);

				if (rollout.IsMyWin)
				{
					return true;
				}
				else if (rollout.IsYoWin)
				{
					return false;
				}
			}

			return rollout.Point > 0;
		}

		public float[] CalcPolicy(Node node)
		{
			throw new NotImplementedException();
		}

		public void CalcScores(Node node)
		{
			Node[] children = node.children;
			if (node.visited == 0)
			{
				for (int i = 0; i < children.Length; i++)
				{
					node.scores[i] = float.MaxValue;
				}
			}
			else
			{
				for (int i = 0; i < children.Length; i++)
				{
					Node child = children[i];
					if (child == null)
					{
						//방문 안 한건 무조건 방문하도록 높게 책정
						node.scores[i] = float.MaxValue;
					}
					else
					{
						node.scores[i] = (float)child.visited / node.visited + 
							(float)(ExplorationRate * Math.Sqrt(2 * Math.Log(node.visited) / child.visited));
					}
				}
			}
			
		}

		public float CalcValue(Node node)
		{
			throw new NotImplementedException();
		}
	}
}
