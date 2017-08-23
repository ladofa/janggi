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
		public delegate float[] CalcScoresHandler(Node node);
		public delegate float CalcLeafEvaluationHandler(Node node);
		public delegate void CalcPolicyWeightsHandler(Node node);

		public abstract class Handlers
		{
			public abstract float[] CalcScores(Node node);
			public abstract float CalcLeafEvaluation(Node node);
			public abstract void CalcPolicyWeights(Node node);
		}

		public CalcScoresHandler CalcScores;
		public CalcLeafEvaluationHandler CalcLeafEvaluation;
		public CalcPolicyWeightsHandler CalcPolicyWeights;

		//--이벤트

		public delegate void ProgressUpdatedHandler(Mcts mcts, int visit, double rate);
		public event ProgressUpdatedHandler ProgressUpdated;

		//--------------------------------------------------------
		List<Node> history;

		Node start;
		public Node root;

		int currentLevel;

		int myFirst;//나부터 시작했으면 0

		public Mcts(Handlers handlers)
		{
			CalcScores = handlers.CalcScores;
			CalcLeafEvaluation = handlers.CalcLeafEvaluation;
			CalcPolicyWeights = handlers.CalcPolicyWeights;

			maxVisitCount = 50000;
		}


		public void Init(Board board)
		{
			start = new Node(null, board, Move.Rest);
			root = start;

			currentLevel = 0;
			myFirst = board.IsMyTurn ? 0 : 1;

			history = new List<Node>();
		}

		int maxVisitCount;
		public int MaxVisitCount
		{
			set => maxVisitCount = value;
			get => maxVisitCount;
		}

		//searchNext함수가 끝날때까지 기다린다.
		public void WaitSearching()
		{
			signalSearch.WaitOne();
		}

		public void PauseSearching()
		{
			signalPause.Reset();
		}

		public void WaitCycle()
		{
			signalCycle.WaitOne();
		}

		public void ResumeSearching()
		{
			signalPause.Set();
		}

		public bool IsSearching
		{
			//reset이면 누군가 들어있는(서치하고 있는) 상황
			get => !signalSearch.WaitOne(0);
		}

		public bool IsPaused
		{
			//reset이면 걸려서 멈춘 상황
			get => !signalPause.WaitOne(0);
		}

		// 두 번 이상 SearchNext를 수행하지 않도록 막는 시그널
		System.Threading.ManualResetEvent signalSearch = new System.Threading.ManualResetEvent(true);
		//잠시멈춤 시그널
		System.Threading.ManualResetEvent signalPause = new System.Threading.ManualResetEvent(true);
		//한 사이클 처리가 끝나기를 기다릴 수 있는 시그널
		System.Threading.ManualResetEvent signalCycle = new System.Threading.ManualResetEvent(true);

		public async Task<Node> SearchNextAsync()
		{
			if (IsSearching)
			{
				return null;
			}

			signalSearch.Reset();

			await Task.Run(() =>
			{
				if (CalcScores == null)
				{
					throw new Exception("CalcScore must not be null.");
				}

				if (CalcLeafEvaluation == null)
				{
					throw new Exception("CalcValue must not be null.");
				}

				if (CalcPolicyWeights == null)
				{
					throw new Exception("CalcPolicy must not be null.");
				}

				int maxDepth = 0;
				int depthSum = 0;

				bool isMyTurn = root.board.IsMyTurn;

				//ParallelOptions option = new ParallelOptions();
				//option.MaxDegreeOfParallelism = 10;

				signalPause.Set();

				while (true)
				{
					//이미 셋으로 돌아섰다면 그냥 리턴
					if (signalSearch.WaitOne(0))
					{
						break;
					}

					//조건을 만족한다면 셋으로 변경하고 리턴
					if (root.visited > maxVisitCount)
					{
						signalSearch.Set();
						break;
					}

					signalCycle.Reset();

					//500단위로 끊자.
					Parallel.For(0, 500, turn =>
					//for (int turn = 0; turn < 500; turn++)
					{
						//랜덤으로 깊이 탐색
						List<Node> visitedNodes = new List<Node>();

						//예전 커밋을 보면 while로 내려가던 것을
						//parallel에서 lock을 걸기 편하게 하기 위해서
						//재귀형식으로 바꿈

						visitDown(root);

						float visitDown(Node node)
						{
							lock (node)
							{
								float leafEvaluation;
								if (node.board.IsMyWin)
								{
									leafEvaluation = 1;
									//exit
								}
								else if (node.board.IsYoWin)
								{
									leafEvaluation = 0;
									//exit
								}
								//첫 방문 노드라면 (익스펜드된 노드라면)
								else if (node.visited == 0)
								{
									//첫 방문 때, 정책망 계산을 한다.
									CalcPolicyWeights(node);
									//끝 노드를 평가한다.
									leafEvaluation = CalcLeafEvaluation(node);
									//exit
								}
								else
								{
									//익스펜션 포함해서 다음 노드를 꺼내준다.
									Node next = GetBestScoreChild(node);
									//go down
									leafEvaluation = visitDown(next);
								}

								//평가가 끝났으면 재귀가 풀리면서 업데이트
								//부모 노드의 턴으로 생각해야 하므로 myTurn을 반대로
								if (!node.board.IsMyTurn)
								{
									//부모 노드의 내차례인 경우
									node.win += leafEvaluation;
								}
								else
								{
									//상대 차례일 때는 반대로 업데이트
									node.win += (1 - leafEvaluation);
								}

								node.visited++;

								return leafEvaluation;
							}
						}
					});

					signalCycle.Set();

					ProgressUpdated?.Invoke(this, root.visited, root.visited / (double)MaxVisitCount);

					signalPause.WaitOne();
				}

				Console.WriteLine($"Max depth : {maxDepth}");
				Console.WriteLine($"Visited : {root.visited}");
			});




			for (int i = 0; i < root.children.Length; i++)
			{
				Node child = root.children[i];
				if (child != null)
				{
					Console.WriteLine($"{i} : {child.win} / {child.visited} ... {root.moves[i]}");
				}
				else
				{
					Console.WriteLine($"{i} : ... ");
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
			signalPause.Set();
			signalSearch.Set();
		}

		public void SetMove(Move move)
		{
			root.PrepareMoves();
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
				parent.PrepareChildren();
				float[] scores = CalcScores(parent);
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