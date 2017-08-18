using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.StoneHelper;

namespace Janggi.Ai
{
	public class Node
	{
		//->노드 기본 정보
		//게임 스테이트
		public Board board;
		//현재 상태로 오게 만드는 무브
		public Move prevMove;
		//부모 노드
		public Node parent;

		//-->서치 중 동적 생성, 이후 계속 유지
		//현재로부터 움직일 수 있는 모든 길
		public List<Move> moves;

		public int visited = 0;
		public float win = 0;

		//각 길에 대한 다음 노드
		public Node[] children;

		//갈 길에 대한 policy network 확률
		public float[] proms;

		//최종 점수
		public float[] scores;

		public Node(Node parent, Board board, Move prevMove)
		{
			lock (this)
			{
				this.parent = parent;
				this.board = board;
				this.prevMove = prevMove;
			}
		}

		public void CalcMoves()
		{
			lock (this)
			{
				if (moves == null)
				{
					moves = board.GetAllPossibleMoves();

					//반복수 제거

					// my    yo    my   yo     my
					// p4 -> p3 -> p2-> p1 -> this -> next

					Node p2 = parent?.parent;
					Node p4 = p2?.parent?.parent;
					//둘 다 제자리로 온 경우
					if (p2 != null && board.Equals(p2.board))
					{
						moves.Remove(parent.prevMove);
					}

					//와리가리 한 경우
					if (p4 != null && board.Equals(p4.board))
					{
						moves.Remove(p2.parent.prevMove);
					}


				}
			}
		}

		public void CalcChildren()
		{
			lock (this)
			{
				if (children == null)
				{
					CalcMoves();
					children = new Node[moves.Count];
				}
			}
		}

		public Node GetChild(int index)
		{
			lock (this)
			{
				CalcChildren();

				if (children[index] == null)
				{
					Move move = moves[index];
					Board nextBoard = board.GetNext(move);
					Node nextNode = new Node(this, nextBoard, move);
					children[index] = nextNode;
				}
			}

			return children[index];
		}

		public void CalcProms()
		{
			lock (this)
			{
				if (moves == null)
				{
					CalcMoves();
				}



				proms = new float[moves.Count];
				int[] points = new int[moves.Count];

				//todo...
				Func<uint, uint, uint, int> Judge;
				if (board.IsMyTurn)
				{
					Judge = (stoneFrom, stoneTo, target) =>
					{
						//일단 상대를 따먹으면 10점
						int takingPoint = GetPoint(stoneTo);
						return takingPoint + ((IsYours(target) ? GetPoint(stoneFrom) : 0) + (takingPoint != 0 ? 10 : 0));
					};
				}
				else
				{
					Judge = (stoneFrom, stoneTo, target) =>
					{
						int takingPoint = -GetPoint(stoneTo);
						return takingPoint + ((IsMine(target) ? -GetPoint(stoneFrom) : 0) + (takingPoint != 0 ? 10 : 0));
					};
				}

				//최소 점수
				int min = int.MaxValue;
				int max = int.MinValue;

				//마지막 rest빼고.
				for (int i = 0; i < proms.Length - 1; i++)
				{
					Move move = moves[i];
					uint stoneFrom = board[move.From];
					uint stoneTo = board[move.To];
					uint target = board.targets[move.To.Y, move.To.X];

					int judge = Judge(stoneFrom, stoneTo, target);
					points[i] = judge;

					if (judge < min)
					{
						min = judge;
					}

					if (judge > max)
					{
						max = judge;
					}
				}

				const int diff = 100;
				const int under = 10;

				int sum = 0;
				for (int i = 0; i < proms.Length - 1; i++)
				{
					points[i] = points[i] - max + diff;
					if (points[i] < under)
					{
						points[i] = under;
					}

					sum += points[i];
				}

				//마지막 rest
				points[proms.Length - 1] = under;
				sum += under;

				//확률값으로 변경
				for (int i = 0; i < proms.Length; i++)
				{
					proms[i] = points[i] / (float)sum;
				}
			}
		}

		

		public void Clear()
		{
			lock (this)
			{
				children = null;
				moves = null;
				proms = null;
				scores = null;
			}
		}

		public void ClearAll()
		{
			lock (this)
			{
				if (children != null)
				{
					foreach (Node node in children)
					{
						node.ClearAll();
					}
				}

				Clear();
			}
		}
	}
}
