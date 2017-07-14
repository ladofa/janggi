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

			public int visited = 0;
			public int win = 0;

			//각 길에 대한 다음 노드
			public Node[] children;

			public Node(Node parent, Board board, Move prevMove)
			{
				lock (this)
				{
					this.parent = parent;
					this.board = board;
					this.prevMove = prevMove;

					moves = GetMoves();
					children = new Node[moves.Count];
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

			public Node GetUcbChild()
			{
				Node unvisitedChild = GetUnvisitedChild();
				if (unvisitedChild != null)
				{
					return unvisitedChild;
				}

				//베스트 스코어를 찾는다
				const double rate = 0.5;
				double maxScore = double.MinValue;
				int maxIndex = -1;
				double k = 2 * Math.Log(visited);
				for (int i = 0; i < children.Length; i++)
				{
					Node node = children[i];
					double score = node.win / node.visited + rate * Math.Sqrt(k / node.visited);

					if (score > maxScore)
					{
						maxScore = score;
						maxIndex = i;
					}
				}

				return children[maxIndex].GetUcbChild();
			}

			public Node GetUnvisitedChild()
			{
				//비어있는 노드가 있으면 expend
				lock (this)
				{
					for (int i = 0; i < children.Length; i++)
					{
						if (children[i] == null)
						{
							return GetChild(i);
						}
					}


					return null;
				}
			}

			public static readonly double rate = 0.7;

			public Node GetBestUcbChild()
			{
				//베스트 스코어를 찾는다
				lock (this)
				{
					double maxScore = double.MinValue;
					int maxIndex = -1;
					double k = 2 * Math.Log(visited);
					for (int i = 0; i < children.Length; i++)
					{
						Node node = children[i];

						if (node.visited == 0)
						{
							continue;
						}

						double score = (double)node.win / node.visited + rate * Math.Sqrt(k / node.visited);

						if (score > maxScore)
						{
							maxScore = score;
							maxIndex = i;
						}
					}

					if (maxIndex == -1)
					{
						throw new Exception("???");
					}
					return children[maxIndex];
				}
			}

			public double UcbScore()
			{
				return (double)win / visited + rate * Math.Sqrt(2 * Math.Log(parent.visited) / visited);
			}
			

			public void Clear()
			{
				lock (this)
				{
					children = null;
					moves = null;
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

		

		public Node SearchNext()
		{
			int maxDepth = 0;
			int depthSum = 0;

			bool isMyTurn = root.board.IsMyTurn;

			const int numSearchNodes = 10000;

			int countFinish = 0;

			Parallel.For(0, numSearchNodes, turn =>
			//for (int turn = 0; turn < numSearchNodes; turn++)
			{
				//랜덤으로 깊이 탐색
				Node parent = root;
				List<Node> nodes = new List<Node>();
				nodes.Add(root);
				Node next;

				bool myWin = false;
				bool finished = false;

				lock (root)
				{
					while (true)
					{
						//비어있는 노드가 있으면 expend
						Node unvisited = parent.GetUnvisitedChild();
						if (unvisited != null)
						{
							next = unvisited;
							nodes.Add(next);
							break;
						}
						else
						{
							//베스트 스코어를 찾는다
							parent = parent.GetBestUcbChild();
							//if (parent.board.IsMyWin)
							//{
							//	finished = true;
							//	myWin = true;
							//}
							//else if (parent.board.IsYoWin)
							//{
							//	finished = true;
							//	myWin = false;
							//}
							nodes.Add(parent);
						}
					}
				}

				int depth = nodes.Count;
				depthSum += depth;
				if (depth > maxDepth)
				{
					maxDepth = depth;
				}

				//rollout
				Board rollout = new Board(next.board);

				//100수까지만 하자 혹시나.
				for (int i = 0; i < 100; i++)
				{
					if (rollout.IsMyWin)
					{
						finished = true;
						myWin = true;
						break;
					}
					else if (rollout.IsYoWin)
					{
						finished = true;
						myWin = false;
						break;
					}

					rollout.MoveRandomNext();
				}

				if (!finished)
				{
					myWin = rollout.Point > 0;
					countFinish++;
				}

				//update
				lock (root)
				{
					//rollout.PrintStones();
					for (int i = 0; i < nodes.Count; i++)
					{
						Node node = nodes[i];
						if (node.board.IsMyTurn ^ myWin)//상대턴인 노드를 고를 때, 내 턴이다.
						{
							node.win++;
						}
						node.visited++;
					}
				}


				//for (int i = 0; i < root.children.Length; i++)
				//{
				//	Node child = root.children[i];
				//	if (child == null) Console.WriteLine(i + "not visited");
				//	else
				//	Console.WriteLine($"{i} : {child.win} / {child.visited} ... {child.UcbScore()}");
				//}
			});

			Console.WriteLine($"Max depth : {maxDepth}, Average : {depthSum / (double)numSearchNodes}");
			Console.WriteLine("Count Not Finish : " + countFinish);
			for (int i = 0; i < root.children.Length; i++)
			{
				Node child = root.children[i];
				Console.WriteLine($"{i} : {child.win} / {child.visited} ... {root.moves[i]} ... {child.UcbScore()}");
			}

			//if (root.promNode == null)
			//{
			//	Console.WriteLine("== NO PROMISSING");
			//	root.promNode = root.GetRandomChild(promCalculator);
			//}

			//Console.WriteLine("Current Point : " + root.point);
			//Console.WriteLine("Expected Point : " + root.promNode.point);


			//Node temp = root.promNode;
			//while (temp.promNode != null)
			//{
			//	Console.WriteLine(temp.promNode.prevMove.ToString() + " : " + temp.promNode.point.ToString());
			//	temp = temp.promNode;
			//}


			return root.GetBestUcbChild();
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
