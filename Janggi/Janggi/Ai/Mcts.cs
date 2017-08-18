using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Janggi.Ai
{
	public class Mcts
	{
		//--각종 델리게이트
		public delegate bool RolloutHandler(Node node);
		public delegate void CalcScoresHandler(Node node);
		public delegate float CalcValueHandler(Node node);
		public delegate float[] CalcPolicyHandler(Node node);

		public interface IHandlers
		{
			bool Rollout(Node node);
			void CalcScores(Node node);
			float CalcValue(Node node);
			float[] CalcPolicy(Node node);
		}

		public RolloutHandler Rollout;
		public CalcScoresHandler CalcScores;
		public CalcValueHandler CalcValue;
		public CalcPolicyHandler CalcPolicy;

		//--------------------------------------------------------
		List<Node> history;

		Node start;
		Node root;

		int currentLevel;

		int myFirst;//나부터 시작했으면 0

		public void Init(Board board)
		{
			start = new Node(null, board, Move.Rest);
			root = start;

			currentLevel = 0;
			myFirst = board.IsMyTurn ? 0 : 1;

			history = new List<Node>();
		}

		public void Init(IHandlers handlers)
		{
			Rollout = handlers.Rollout;
			CalcScores = handlers.CalcScores;
			CalcValue = handlers.CalcValue;
			CalcPolicy = handlers.CalcPolicy;
		}

		public Node SearchNext()
		{
			if (Rollout == null)
			{
				throw new Exception("Rollout must not be null.");
			}

			if (CalcScores == null)
			{
				throw new Exception("CalcScore must not be null.");
			}

			if (CalcValue == null)
			{
				throw new Exception("CalcValue must not be null.");
			}

			if (CalcPolicy == null)
			{
				throw new Exception("CalcPolicy must not be null.");
			}

			int maxDepth = 0;
			int depthSum = 0;

			bool isMyTurn = root.board.IsMyTurn;

			const int numSearchNodes = 10000;

			int countFinish = 0;

			ParallelOptions option = new ParallelOptions();
			option.MaxDegreeOfParallelism = 10;

			Parallel.For(0, numSearchNodes, option, turn =>
			//for (int turn = 0; turn < numSearchNodes; turn++)
			{
				//랜덤으로 깊이 탐색
				Node parent = root;
				List<Node> nodes = new List<Node>();
				Node next;

				bool myWin = false;
				bool finished = false;


				while (true)
				{
					//비어있는 노드가 있으면 expend
					next = GetBestScoreChild(parent);

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
					myWin = Rollout(next);
				}

				if (finished)
				{
					countFinish++;
				}

				//update
				lock (root)
				{
					for (int i = nodes.Count - 1; i >= 0; i--)
					{
						Node node = nodes[i];
						//부모노드의 승리를 각 child에 나눠서 저장
						if (node.board.IsMyTurn != myWin)
						{
							node.win++;
						}

						node.visited++;
					}
					root.visited++;
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
			Console.WriteLine("Count Finish : " + countFinish + " / " + numSearchNodes);
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


			return GetMostVisitedChild(root);
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

		public Node GetBestScoreChild(Node parent)
		{
			lock (parent)
			{
				if (parent.scores == null)
				{
					parent.CalcMoves();
					parent.scores = new float[parent.moves.Count];
				}

				parent.CalcChildren();
				parent.CalcProms();

				CalcScores(parent);

				float[] scores = parent.scores;
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

				return parent.GetChild(index);
			}
		}

		public Node GetMostVisitedChild(Node parent)
		{
			lock (parent)
			{
				Node[] children = parent.children;
				Node node = null;
				int max = int.MinValue;
				for (int i = 0; i < children.Length; i++)
				{
					if (children[i] == null)
					{
						continue;
					}

					if (max < children[i].visited)
					{
						max = children[i].visited;
						node = children[i];
					}
				}

				if (node == null)
				{
					throw new Exception("There is no visited child");
				}

				return node;
			}
		}
	}
}