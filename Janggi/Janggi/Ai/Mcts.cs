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
			double[] Calc(Node node);
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
			public double[] cproms;

			public int visited = 0;
			public int win = 0;

			//각 길에 대한 다음 노드
			public Node[] children;

			//차일드로부터 업데이트된 가치
			public int myPoint;
			public int yoPoint;

			//딱 현재 노드의 가치
			public int judge;

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

					judge = board.Judge();

					if (board.IsMyTurn)
					{
						yoPoint = judge;
						myPoint = Int32.MaxValue;
					}
					else
					{
						myPoint = judge;
						yoPoint = Int32.MinValue;
					}
				}
			}

			public void GetMoves()
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
			}

			public void GetCproms(IPromCalculator promCalculator)
			{
				lock (this)
				{
					if (cproms == null)
					{
						double[] proms = promCalculator.Calc(this);
						cproms = new double[proms.Length + 1];

						cproms[0] = 0;
						for (int i = 0; i < proms.Length - 1; i++)
						{
							//cproms[i + 1] = cproms[i] + proms[i];
							cproms[i + 1] = cproms[i] + proms[i];
						}
						cproms[cproms.Length - 1] = 1;
					}
				}
			}

			public void GetChildren()
			{
				lock (this)
				{
					if (children == null)
					{
						GetMoves();
						children = new Node[moves.Count];
						for (int i = 0; i < children.Length; i++)
						{
							Move move = moves[i];
							Board nextBoard = board.GetNext(move);
							Node nextNode = new Node(this, nextBoard, move);
							children[i] = nextNode;
						}
					}
				}
			}

			public Node GetRandomChild(IPromCalculator promCalculator)
			{
				GetCproms(promCalculator);

				//prob [0, 1)
				double prob = Global.Rand.NextDouble();
				if (prob == 1)
				{
					return children[children.Length - 1];
				}

				int N = cproms.Length;

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

				GetChildren();
				return children[k];
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
					GetChildren();
					if (board.IsMyTurn)
					{
						yoPoint = int.MinValue;
						foreach (Node child in children)
						{
							if (child.yoPoint > yoPoint)
							{
								yoPoint = child.yoPoint;
								promNode = child;
							}
						}

						if (promNode.myPoint != Int32.MaxValue)
						{
							myPoint = promNode.myPoint;
						}

					}
					else
					{
						myPoint = int.MaxValue;
						foreach (Node child in children)
						{
							if (child.myPoint < myPoint)
							{
								myPoint = child.myPoint;
								promNode = child;
							}
						}

						if (promNode.yoPoint != Int32.MinValue)
						{
							yoPoint = promNode.yoPoint;
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

			//last에서 선택하기

			if (last.board.IsMyTurn)
			{
				Node[] children = last.children;
				int maxPoint = Int32.MinValue;
				for (int i = 0; i < children.Length; i++)
				{
					Node child = children[i];
					if (child.myPoint > maxPoint)
					{
						maxPoint = child.myPoint;
						last.promNode = child;
					}
				}
				last.myPoint = maxPoint;
			}
			else
			{
				Node[] children = last.children;
				int minPoint = Int32.MaxValue;
				for (int i = 0; i < children.Length; i++)
				{
					Node child = children[i];
					if (child.yoPoint < minPoint)
					{
						minPoint = child.yoPoint;
						last.promNode = child;
					}
				}
				last.yoPoint = minPoint;
			}

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
						if (p2.yoPoint > p1.yoPoint)
						{
							p1.yoPoint = p2.yoPoint;
							if (p2.myPoint != Int32.MaxValue)
							{
								p1.myPoint = p2.myPoint;
							}
						}
						else
						{
							p1.RegatherPoint();
						}
					}
					//상대방 차례면 낮은 포인틀르 선택
					else
					{
						if (p2.myPoint < p1.myPoint)
						{
							p1.myPoint = p2.myPoint;
							if (p2.yoPoint != Int32.MinValue)
							{
								p1.yoPoint = p2.yoPoint;
							}
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
						if (p2.yoPoint > p1.yoPoint)
						{
							p1.yoPoint = p2.yoPoint;
							p1.promNode = p2;
							if (p2.myPoint != Int32.MaxValue)
							{
								p1.myPoint = p2.myPoint;
							}
						}
					}
					//상대방 차례면 낮은 포인틀르 선택
					else
					{
						if (p2.myPoint < p1.myPoint)
						{
							p1.myPoint = p2.myPoint;
							p1.promNode = p2;
							if (p2.yoPoint != Int32.MinValue)
							{
								p1.yoPoint = p2.yoPoint;
							}
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

			const int numSearchNodes = 50000;

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

					if (next.children != null)
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

						child.GetChildren();

						lock (root)
						{
							updatePoint(nodes);
						}

						break;
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

			Console.WriteLine("My Current Point : " + root.myPoint);
			Console.WriteLine("Yo Current Point : " + root.yoPoint);
			Console.WriteLine("My Expected Point : " + root.promNode.myPoint);
			Console.WriteLine("Yo Expected Point : " + root.promNode.yoPoint);

			int otherMin = int.MaxValue;
			int otherMax = int.MinValue;
			for (int i = 0; i < root.children.Length; i++)
			{
				Node child = root.children[i];
				
					Console.WriteLine($"Other[{i}] : " + root.moves[i].ToString() + " : " + child.myPoint + " : " + child.yoPoint);
					if (child.yoPoint < otherMin)
					{
						otherMin = child.yoPoint;
					}
					if (child.myPoint > otherMax)
					{
						otherMax = child.myPoint;
					}
				
			}

			if (isMyTurn)
			{
				if (otherMax > root.promNode.myPoint)
				{
					Console.WriteLine("?????????? : " + otherMax);
				}
			}
			else
			{
				if (otherMin < root.promNode.yoPoint)
				{
					Console.WriteLine("?????????? : " + otherMin);
				}
			}

			Node temp = root.promNode;
			while (temp.promNode != null)
			{
				Console.WriteLine(temp.promNode.prevMove.ToString() + " : " + temp.promNode.myPoint + " : " + temp.promNode.yoPoint);
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
					root.GetChildren();
					SetMove(root.children[i]);
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
