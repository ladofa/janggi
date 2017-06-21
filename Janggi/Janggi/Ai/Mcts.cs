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
			//->노드 기본 정보
			//게임 스테이트
			public Board board;
			//현재 상태로 오게 만드는 무브
			public Move prevMove;
			
			//-->서치 중 동적 생성, 이후 계속 유지
			//현재로부터 움직일 수 있는 모든 길
			public List<Move> moves;
			//각 길에 대한 가능성 누적
			public List<double> cproms;
			//각 길에 대한 다음 노드
			public List<Node> children;

			//최종적인 현재 노드의 선택 가치
			public int point;
			//가장 유망한 노드
			public Node promNode = null;
			//방문햇는지 여부
			public bool isVisited = false;

			public Node(Board board, Move prevMove)
			{
				this.board = board;
				this.prevMove = prevMove;
				if (board.IsMyTurn)
				{
					point = Int32.MinValue;
				}
				else
				{
					point = Int32.MaxValue;
				}

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
						Node nextNode = new Node(nextBoard, move);
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
			start = new Node(board, Move.Rest);
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
			Move bestMove = Move.Rest;

			for (int turn = 0; turn < 400; turn++)
			{
				//랜덤으로 깊이 탐색
				Node child = root;
				List<Node> search = new List<Node>();
				int maxLevel = currentLevel + 10;//홀수이면 내 차례가 마지막
				for (int level = currentLevel; level < maxLevel; level++)
				{
					child = child.GetRandomChild(promCalculator);
					search.Add(child);
					if (child.board.IsMyWin)
					{
						break;
					}
					else if (child.board.IsYoWin)
					{
						break;
					}
				}

				//상향식 점수 업데이트
				//마지막 노드의 점수 넣기
				child.point = child.board.Point;
				
				for (int i = search.Count - 2; i >= 0; i--)
				{
					Node p1 = search[i];//parent
					Node p2 = search[i + 1];//child

					//내 차례라면 높은 포인트를 선택
					if (p1.board.IsMyTurn)
					{
						if (p2.point > p1.point)
						{
							p1.point = p2.point;
							p1.promNode = p2;
						}
					}
					//상대방 차례면 낮은 포인틀르 선택
					else
					{
						if (p2.point < p1.point)
						{
							p1.point = p2.point;
							p1.promNode = p2;
						}
						
					}
				}

				//마지막 업데이트
				if (root.board.IsMyTurn)
				{
					if (search[0].point > root.point)
					{
						root.point = search[0].point;
						root.promNode = search[0];
					}
				}
				else
				{
					if (search[0].point < root.point)
					{
						root.point = search[0].point;
						root.promNode = search[0];
					}
				}
			}

			if (root.promNode == null)
			{
				throw new Exception("unexpected");
			}

			return root.promNode.prevMove;
		}

		public void ForceStopSearch()
		{

		}

		public void SetMove(Move move)
		{

		}
	}
}
