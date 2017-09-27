using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.StoneHelper;
using Janggi.TensorFlow;

namespace Janggi.Ai
{
	public class RealYame : Mcts.Handlers
	{
		TcpCommClient client = new TcpCommClient();

		string policyNetName = "policy192";
		string valueNetName = "value192";

		public RealYame()
		{
			while (!client.Connect("localhost", 9999))
			{
				Console.WriteLine("ConnectionFailed.");
				System.Threading.Thread.Sleep(1000);
			}

			client.LoadModel(NetworkKinds.Policy, policyNetName, policyNetName);
			client.LoadModel(NetworkKinds.Value, valueNetName, valueNetName);

			MaxRolloutDepth = 100;
			ExplorationRate = 1;
			Alpha = 0.5f;
		}

		public RealYame(TcpCommClient client)
		{
			this.client = client;

			MaxRolloutDepth = 100;
			ExplorationRate = 0.2;
			Alpha = 1;
		}

		public int MaxRolloutDepth
		{
			set;
			get;
		}

		public double ExplorationRate
		{
			set;
			get;
		}

		public float Alpha
		{
			set;
			get;
		}


		public override void CalcPolicyWeights(Node node)
		{
			node.PreparePolicyWeights();

			if (node.board.IsMyTurn)
			{
				node.policyWeights = client.EvaluatePolicy(node.board, node.moves, policyNetName);
			}
			else
			{
				Board opBoard = node.board.GetOpposite();
				List<Move> opMoves = Move.GetOpposite(node.moves);
				node.policyWeights = client.EvaluatePolicy(opBoard, opMoves, policyNetName);
			}

			float[] proms = node.policyWeights;

			float total = 0;

			for (int i = 0; i < proms.Length; i++)
			{
				total += proms[i];
			}

			if (total == 0)
			{
				proms[Global.Rand.Next(proms.Length)] = 1;
			}
			else
			{
				for (int i = 0; i < proms.Length; i++)
				{
					proms[i] /= total;
				}
			}
		}

		public override float[] CalcScores(Node node)
		{
			Node[] children = node.children;
			float[] scores = new float[children.Length];
			float[] policyWeights = node.policyWeights;
			if (node.visited == 0)
			{
				for (int i = 0; i < children.Length; i++)
				{
					scores[i] = float.MaxValue;
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
						scores[i] = float.MaxValue;
					}
					else
					{
						scores[i] = child.win / child.visited +
							(float)(ExplorationRate * (policyWeights[i] + 0.5) * Math.Sqrt(node.visited / child.visited));
					}
				}
			}
			return scores;
		}

		public override float CalcLeafEvaluation(Node node)
		{
			//rollout
			Board rollout = new Board(node.board);

			bool finished = false;
			float rolloutResult = 0;

			if (Alpha != 0)
			{
				//100수까지만 하자 혹시나.
				for (int i = 0; i < 10; i++)
				{
					moveRandomNext(rollout);

					if (rollout.IsMyWin)
					{
						rolloutResult = 1;
						finished = true;
					}
					else if (rollout.IsYoWin)
					{
						rolloutResult = 0;
						finished = true;
					}
				}
				if (!finished)
				{
					rolloutResult = (float)Math.Max(Math.Min(rollout.Point * 0.01 + 0.5, 1), 0);
				}
			}


			//100% value network으로 대체

			float valueResult = 0;
			if (Alpha != 1)
			{
				valueResult = client.EvaluateValue(node.board, valueNetName);
			}

			return rolloutResult * Alpha + (1 - Alpha) * valueResult;
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
					return takingPoint + ((IsYours(targetTo) ? GetPoint(stoneFrom) : 0) + (takingPoint != 0 ? 10 : 0));
				};
			}
			else
			{
				Judge = (stoneFrom, stoneTo, targetTo, targetFrom) =>
				{
					int takingPoint = -GetPoint(stoneTo);
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

			proms[proms.Length - 1] = min;
			sum += min;

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
	}
}
