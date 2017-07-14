using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi.Ai
{
	public class Mcts
	{
		public interface IPromCalculator
		{
			List<double> Calc(Node node);
		}

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
			//각 길에 대한 가능성 누적
			public List<double> cproms;



			public int visited = 0;
			public int win = 0;

			//각 길에 대한 다음 노드
			public List<Node> children;

			//최종적인 현재 노드의 선택 가치
			public int point;
			//가장 유망한 노드
			public Node promNode = null;
			//방문햇는지 여부
			public bool isVisited = false;

			public Node(Node parent, Board board, Move prevMove)
			{
				lock (this)
				{
					this.parent = parent;
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

					moves = GetMoves();
					children = new List<Node>();
					for (int i = 0; i < moves.Count; i++)
					{
						children.Add(null);
					}
				}
			}

			public List<Move> GetMoves()
			{
				lock (this)
				{
					if (moves == null)
					{
						moves = board.GetAllMoves();

						//반복수 제거

						// my    yo    my   yo     my
						// p4 -> p3 -> p2-> p1 -> this -> next

						Node p2 = this.parent?.parent;
						Node p4 = p2?.parent?.parent;
						//둘 다 제자리로 온 경우
						if (p2 != null && board.Equals(p2.board))
						{
							moves.Remove(this.parent.prevMove);
						}

						//와리가리 한 경우
						if (p4 != null && board.Equals(p4.board))
						{
							moves.Remove(p2.parent.prevMove);
						}
					}
				}

				return moves;
			}

			public List<double> GetCproms(IPromCalculator promCalculator)
			{
				lock (this)
				{
					if (cproms == null)
					{
						List<double> proms = promCalculator.Calc(this);
						cproms = new List<double>();

						cproms.Add(0);
						for (int i = 0; i < proms.Count - 1; i++)
						{
							//cproms[i + 1] = cproms[i] + proms[i];
							cproms.Add(cproms[i] + proms[i]);
						}
						cproms.Add(1);
					}
				}

				return cproms;
			}

			public Node GetChild(int index)
			{
				lock (this)
				{
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

			public Node GetRandomChild(IPromCalculator promCalculator)
			{
				GetCproms(promCalculator);

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
						//cproms[k] <= prob < cproms[k + 1]
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
					return GetChild(k);
				
			}

			public void Clear()
			{
				lock (this)
				{
					children = null;
					cproms = null;
					moves = null;
					promNode = null;
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

			public void RegatherPoint()
			{
				lock (this)
				{
					if (board.IsMyTurn)
					{
						point = int.MinValue;
						//Parallel.ForEach<Node>(children, child =>
						foreach (Node child in children)
						{
							if (child != null && child.point > point)
							{
								point = child.point;
								promNode = child;
							}
						}//);
					}
					else
					{
						point = int.MaxValue;
						//Parallel.ForEach<Node>(children, child =>
						foreach (Node child in children)
						{
							if (child != null && child.point < point)
							{
								point = child.point;
								promNode = child;
							}
						}
					}
				}
			}
		}


		//--------------------------------------------------------
		List<Node> history;

		Node start;
		Node root;

		int currentLevel;

		int myFirst;//나부터 시작했으면 0

		IPromCalculator promCalculator;

		public void Init(Board board)
		{
			start = new Node(null, board, Move.Rest);
			root = start;

			currentLevel = 0;
			myFirst = board.IsMyTurn ? 0 : 1;

			history = new List<Node>();

			promCalculator = new PointPromCalculator();
		}

		private void updatePoint(List<Node> nodes)
		{
			Node last = nodes[nodes.Count - 1];
			last.point = last.board.Judge();
			last.promNode = last.GetChild(last.moves.Count - 1);
			last.promNode.point = last.point;

			for (int i = nodes.Count - 2; i >= 0; i--)
			{
				Node p1 = nodes[i];//parent
				Node p2 = nodes[i + 1];//child

				//원래 선택했던 노드
				if (p1.promNode == p2)
				{
					//내 차례라면 높은 포인트를 선택
					if (p1.board.IsMyTurn)
					{
						if (p2.point >= p1.point)
						{
							p1.point = p2.point;
						}
						else
						{
							p1.RegatherPoint();
						}
					}
					//상대방 차례면 낮은 포인틀르 선택
					else
					{
						if (p2.point <= p1.point)
						{
							p1.point = p2.point;
						}
						else
						{
							p1.RegatherPoint();
						}
					}
				}
				else
				{
					if (p1.board.IsMyTurn)
					{
						if (p2.point >= p1.point)
						{
							p1.point = p2.point;
							p1.promNode = p2;
						}
					}
					//상대방 차례면 낮은 포인틀르 선택
					else
					{
						if (p2.point <= p1.point)
						{
							p1.point = p2.point;
							p1.promNode = p2;
						}
					}
				}
			}
		}

		public Node SearchNext()
		{
			int maxDepth = 0;
			int depthSum = 0;

			bool isMyTurn = root.board.IsMyTurn;

			const int numSearchNodes = 10000;

			object maxDepthObject = new object();

			const int limitDepth = 30;


			Parallel.For(0, numSearchNodes, turn =>
			//for (int turn = 0; turn < numSearchNodes; turn++)
			{
				//랜덤으로 깊이 탐색
				Node child = root;
				List<Node> nodes = new List<Node>();
				//nodes.Add(root);
				int depth = 0;

				Node next = child;
				while (true)
				{
					//다음 노드를 랜덤하게 선택
					next = child.GetRandomChild(promCalculator);

					if (next.isVisited)
					{
						if (next.board.IsFinished)
						{
							//끝낸다.
							//child = root;
							break;
						}
						else
						{
							//넥스트를 차일드로 선택하고,
							child = next;
							nodes.Add(child);
							depth++;
							if (depth > limitDepth)
							{
								break;
							}
							//다시 넥스트를 고른다.
						}
					}
					else
					{
						child = next;
						nodes.Add(child);
						depth++;

						child.isVisited = true;
						lock (root)
						{
							updatePoint(nodes);
						}

						//상대 차례를 마지막으로 끝낸다.
						if (isMyTurn == child.board.IsMyTurn)
						{
							break;
						}


					}
				}


				depthSum += depth;
				if (depth > maxDepth)
				{
					maxDepth = depth;
				}
			});

			lock (root)
			{
				root.RegatherPoint();
			}

			Console.WriteLine($"Max depth : {maxDepth}, Average : {depthSum / (double)numSearchNodes}");
			if (root.promNode == null)
			{
				Console.WriteLine("== NO PROMISSING");
				root.promNode = root.GetRandomChild(promCalculator);
			}

			Console.WriteLine("Current Point : " + root.point);
			Console.WriteLine("Expected Point : " + root.promNode.point);

			//int otherMin = int.MaxValue;
			//int otherMax = int.MinValue;
			//for (int i = 0; i < root.children.Count; i++)
			//{
			//	Node child = root.children[i];
			//	if (child != null && child.isVisited )
			//	{
			//		Console.WriteLine($"Other[{i}] : " + root.moves[i].ToString() + " : " + child.point + $"  ({root.proms[i]})");
			//		if (child.point < otherMin)
			//		{
			//			otherMin = child.point;
			//		}
			//		if (child.point > otherMax)
			//		{
			//			otherMax = child.point;
			//		}
			//	}
			//}

			//if (isMyTurn)
			//{
			//	if (otherMax > root.promNode.point)
			//	{
			//		Console.WriteLine("?????????? : " + otherMax);
			//	}
			//}
			//else
			//{
			//	if (otherMin < root.promNode.point)
			//	{
			//		Console.WriteLine("?????????? : " + otherMin);
			//	}
			//}

			Node temp = root.promNode;
			while (temp.promNode != null)
			{
				Console.WriteLine(temp.promNode.prevMove.ToString() + " : " + temp.promNode.point.ToString());
				temp = temp.promNode;
			}


			return root.promNode;
		}

		public void ForceStopSearch()
		{

		}

		public void SetMove(Move move)
		{
			bool moved = false;
			for (int i = 0; i < root.moves.Count; i++)
			{
				if (move.Equals(root.moves[i]))
				{
					SetMove(root.GetChild(i));
					currentLevel++;
					moved = true;
					break;
				}
			}

			if (!moved)
			{
				throw new Exception("bad move");
			}
		}

		public void SetMove(Node node)
		{
			root.Clear();
			history.Add(root);
			root = node;
		}
	}
}
