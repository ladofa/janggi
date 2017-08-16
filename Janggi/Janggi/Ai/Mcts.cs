using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.StoneHelper;

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

			public int visited = 0;
			public int win = 0;

			bool deadEnd;

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

			//스코어 계산.
			public void CalcScores()
			{
				lock (this)
				{
					if (scores == null)
					{
						CalcMoves();
						scores = new float[moves.Count];
					}

					CalcChildren();
					CalcProms();

					if (visited == 0)
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
							if (children[i] == null)
							{
								//방문 안 한건 무조건 방문하도록 높게 책정
								scores[i] = float.MaxValue;
								//scores[i] = proms[i] / (1 + visited);
							}
							else
							{
								scores[i] = children[i].UcbScore();
								//scores[i] = (float)children[i].visited / visited + proms[i] / (1 + visited);
							}
						}
					}
				}
			}

			public Node GetBestScoreChild()
			{
				CalcScores();

				lock (this)
				{
					float max = scores[0];
					int index = 0;
					for (int i = 1; i < scores.Length; i++)
					{
						if (max < scores[i])
						{
							max = scores[i];
							index = i;
						}
					}

					return GetChild(index);
				}
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

			public static float rate = 0.1f;
			public float UcbScore()
			{
				return (float)win / visited + (float)(rate * Math.Sqrt(2 * Math.Log(parent.visited) / visited));
			}

			static Random random = new Random();

			public bool RolloutGetMyWin()
			{
				//return board.Judge() > 0;

				//rollout
				Board rollout = new Board(board);

				//100수까지만 하자 혹시나.
				for (int i = 0; i < 200; i++)
				{
					if (rollout.IsMyWin)
					{
						return true;
					}
					else if (rollout.IsYoWin)
					{
						return false;
					}

					rollout.MoveRandomNext();
				}

				return rollout.Point > 0;
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

			const int numSearchNodes = 5000;

			int countFinish = 0;

			ParallelOptions option = new ParallelOptions();
			option.MaxDegreeOfParallelism = 10;

			Parallel.For(0, numSearchNodes, option, turn =>
			//for (int turn = 0; turn < numSearchNodes; turn++)
			{
				//랜덤으로 깊이 탐색
				Node parent = root;
				List<Node> nodes = new List<Node>();
				nodes.Add(root);
				Node next;

				bool myWin = false;
				bool finished = false;


				while (true)
				{
					//비어있는 노드가 있으면 expend
					next = parent.GetBestScoreChild();

					if (next == null)
					{
						break;
					}

					nodes.Add(next);
					if (next.visited != 0)
					{
						//그리고 다음 와일~
						parent = next;
					}
					else
					{
						if (next.board.IsMyWin)
						{
							finished = true;
							myWin = true;
						}
						else if (next.board.IsYoWin)
						{
							finished = true;
							myWin = false;
						}

						break;
					}
				}

				int depth = nodes.Count;
				depthSum += depth;
				if (depth > maxDepth)
				{
					maxDepth = depth;
				}

				if (next == null)
				{
					throw new Exception("???");
					//return;
				}

				if (!finished)
				{
					myWin = next.RolloutGetMyWin();
				}

				//update
				lock (root)
				{
					for (int i = nodes.Count - 1; i >= 0; i--)
					{
						Node node = nodes[i];
						if (node.board.IsMyTurn != myWin)//child노드의 win은 부모노드의 승리를 뜻한다.
						{
							node.win++;
						}
						node.visited++;

						node.CalcScores();
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
				if (child != null)
				{
					Console.WriteLine($"{i} : {child.win} / {child.visited} ... {root.moves[i]} ... {root.scores[i]}");
				}
				else
				{
					Console.WriteLine($"{i} : ... {root.scores[i]}");
				}
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


			return root.GetBestScoreChild();
		}

		public void ForceStopSearch()
		{

		}

		public void SetMove(Move move)
		{
			root.CalcMoves();
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