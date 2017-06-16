using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public class Mcts
	{
		public interface IPromCalculator
		{
			List<double> Calc(Board board, List<Move> moves);
		}

		public class Node
		{
			//게임 스테이트
			public Board board;
			//현재로부터 움직일 수 있는 모든 길
			public List<Move> moves;
			//각 길에 대한 가능성 누적
			public List<double> cproms;
			//각 길에 대한 다음 노드
			public List<Node> children;

			public Node(Board board)
			{
				this.board = board;
			}

			public List<Move> GetMoves()
			{
				if (moves == null)
				{
					moves = board.GetAllMoves();
				}

				return moves;
			}

			public List<double> GetCproms(IPromCalculator promCalculator)
			{
				if (cproms == null)
				{
					List<double> proms = promCalculator.Calc(board, moves);
					cproms = new List<double>();

					cproms.Add(0);
					for (int i = 0; i < proms.Count - 1; i++)
					{
						cproms[i + 1] = cproms[i] + proms[i];
					}
					cproms.Add(1);
				}
				
				return cproms;
			}

			public List<Node> GetChildren()
			{
				if (moves == null)
				{
					moves = board.GetAllMoves();
				}

				if (children == null)
				{
					children = new List<Node>();
					foreach (Move move in moves)
					{
						Board nextBoard = board.GetNext(move);
						Node nextNode = new Node(nextBoard);
						children.Add(nextNode);
					}
				}

				return children;
			}

			public Node GetRandomChild(IPromCalculator promCalculator)
			{
				if (cproms != null)
				{
					GetCproms(promCalculator);
				}

				//prob [0, 1)
				double prob = Global.Rand.NextDouble();
				if (prob == 1)
				{
					return children[children.Count - 1];
				}

				int N = cproms.Count;
				
				int bottom = 0;
				int top = N - 1;
				int k = (bottom + top) / 2;

				while (true)
				{
					if (prob < cproms[k])
					{
						top = k - 1;
					}
					else if (prob >= cproms[k + 1])
					{
						bottom = k + 1;
					}
					else
					{
						break;
					}

					k = (bottom + top) / 2;
				}

				return children[k];
			}
		}


		//--------------------------------------------------------


		Node start;
		Node root;

		int currentLevel;

		int myFirst;//나부터 시작했으면 0

		IPromCalculator promCalculator;

		public void Init(Board board)
		{
			start = new Node(board);
			root = start;
			
			currentLevel = 0;
			myFirst = board.IsMyTurn ? 0 : 1;
		}

		bool isMyTurn(int level)
		{
			return (level % 2) == myFirst;
		}

		public Move SearchNext()
		{
			bool myTurn = root.board.IsMyTurn;
			int level = currentLevel;
			Node child = root;
			while (level - currentLevel < 10)
			{
				child = child.GetRandomChild(promCalculator);
				if (child.board.IsMyWin)
				{
					
				}
			}
			
		}

		public void ForceStopSearch()
		{

		}

		public void SetMove(Move move)
		{

		}
	}
}
