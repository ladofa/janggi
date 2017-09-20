using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Janggi;

using Janggi.Ai;


namespace Runner.Process
{
	public class Reinforcement
	{
		bool running;

		public void Stop()
		{
			running = false;
		}

		List<Tuple<Board, Move>> recWinPolicy = new List<Tuple<Board, Move>>();
		List<Tuple<Board, float>> recWinValue = new List<Tuple<Board, float>>();
		System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);
		Janggi.TensorFlow.TcpCommClient tcpCommClient = new Janggi.TensorFlow.TcpCommClient();

		string policyNetName = "policy128";
		string valueNetName = "value128";

		public Reinforcement()
		{
			System.Timers.Timer timer = new System.Timers.Timer(300000);
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
			while (!tcpCommClient.Connect("localhost", 9999))
			{
				Console.WriteLine("ConnectionFailed.");
				System.Threading.Thread.Sleep(1000);
			}

			Console.WriteLine("Connected!");

			bool succeed = tcpCommClient.LoadModel(Janggi.TensorFlow.NetworkKinds.Policy, policyNetName, policyNetName);
			if (succeed)
			{
				Console.WriteLine("LOAD succeed : " + policyNetName);
			}
			else
			{
				Console.WriteLine("CREATE new network : " + policyNetName);
			}

			succeed = tcpCommClient.LoadModel(Janggi.TensorFlow.NetworkKinds.Value, valueNetName, valueNetName);
			if (succeed)
			{
				Console.WriteLine("LOAD succeed : " + valueNetName);
			}
			else
			{
				Console.WriteLine("CREATE new network : " + valueNetName);
			}

			////////////////////////////////////////////////////			

			//학습

			//자료 모으기 프로세스
			Task.Run(()=>
			{
				int makingTurn = 0;
				while (running)
				{
					if (recWinPolicy.Count > 10000)
					{
						System.Threading.Thread.Sleep(1000);
					}
					else
					{
						
						genMcts();
						signal.Set();//만들었으니 학습을 시도하시오 파란불 반짝
					}

					makingTurn++;
				}
			});

			//학습 프로세스
			running = true;
			while (running)
			{
				const int setCount = 255 * 3;
				if (recWinPolicy.Count >= setCount)
				{
					Console.WriteLine("train policy ... " + DateTime.Now.ToString());
					var dataPolicy = recWinPolicy.GetRange(0, setCount);
					tcpCommClient.TrainPolicy(dataPolicy, policyNetName);

					lock (recWinPolicy)
					{
						recWinPolicy.RemoveRange(0, setCount);
						//Console.WriteLine("  remain : " + recWin.Count);
					}

					while (recWinValue.Count >= setCount && recWinValue.Count > recWinPolicy.Count)
					{
						Console.WriteLine("train value ... " + DateTime.Now.ToString());
						var dataValue = recWinValue.GetRange(0, setCount);
						tcpCommClient.TrainValue(dataValue, valueNetName);

						lock (recWinPolicy)
						{
							recWinValue.RemoveRange(0, setCount);
							//Console.WriteLine("  remain : " + recWin.Count);
						}
					}

					
					Console.WriteLine("train OK.");
				}
				else
				{
					signal.Reset();
					signal.WaitOne();
				}
			}

			tcpCommClient.Disconnect();
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine("save ...");
			tcpCommClient.SaveModel(Janggi.TensorFlow.NetworkKinds.Policy, policyNetName, policyNetName);
			tcpCommClient.SaveModel(Janggi.TensorFlow.NetworkKinds.Value, valueNetName, valueNetName);
			Console.WriteLine("save OK. #######################################################");
		}


		int confirmRandom = 0;

		void genRandom()
		{
			//Console.WriteLine("genRandom ... ");
			//게임 한 판 시작 ---------------------------
			List<Tuple<Board, Move>> rec = new List<Tuple<Board, Move>>();

			//랜덤으로 보드 생성
			//상대방 선수로 놓는다. 어차피 시작하자마자 GetOpposite로 돌릴 거다.
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), false);

			//먼저시작하는 쪽이 p1이든 p2든 상관없다.
			bool isP1Turn = false;
			bool isP1Win = false;

			float correctionRate = 0;
			int emptyCount = 0;
			int correctionCount = 0;
			for (int turn = 0; turn < 20; turn++)
			{
				//빙글 돌려서 p2->p1->p2 가 플레이할 수 있도록 해준다.
				//policy network는 항상 아래쪽 세력을 움직여야할 기물로 여기기 때문.
				board = board.GetOpposite();
				isP1Turn = !isP1Turn;

				Move move;
				if (turn < 10 && confirmRandom % 20 == 0)
				{
					var proms = tcpCommClient.EvaluatePolicy(board, board.GetAllPossibleMoves(), policyNetName);

					//proms를 기반으로 랜덤으로 고른다.
					Move move2 = board.GetRandomMove(proms, out float total);
					correctionRate += total;
					correctionCount++;
					if (move2.IsEmpty)
					{
						emptyCount++;
					}
				}

				List<Move> possibleMoves = board.GetAllPossibleMoves();
				int r = Global.Rand.Next(possibleMoves.Count);
				move = possibleMoves[r];

				if (turn > 10)
				{
					rec.Add(new Tuple<Board, Move>(board, move));
				}

				//다음보드로.
				board = board.GetNext(move);

				//겜이 끝났는지 확인
				if (board.IsFinished)
				{
					//p1턴이었는데 나 쪽(아래쪽)이 이긴 거면 p1이 이긴거
					//p1턴이 아니었는데 반대쪽(위쪽)이 이긴 것도 p1이 이긴거
					//나머지는 p2가 이긴거
					isP1Win = board.IsMyWin == isP1Turn;
					break;
				}

				//board.PrintStones();
			}

			//턴제한으로 끝났으면 점수로
			if (!board.IsFinished)
			{
				isP1Win = board.Point > 0 == isP1Turn;
			}

			lock (recWinPolicy)
			{
				var flip = from r in rec select (new Tuple<Board, Move>(r.Item1.GetFlip(), r.Item2.GetFlip()));
				recWinPolicy.AddRange(rec);
				//recWinPolicy.AddRange(flip);

				var list1 = from r in rec select (new Tuple<Board, float>(r.Item1, r.Item1.Point > 0 ? 1 : 0));
				var list2 = from r in flip select (new Tuple<Board, float>(r.Item1, r.Item1.Point > 0 ? 1 : 0));

				recWinValue.AddRange(list1);
				//recWinValue.AddRange(list2);

				//Console.WriteLine("  remain : " + recWin.Count);
				if (correctionCount > 0)
				{
					Console.WriteLine("  * correction rate : " + (correctionRate / correctionCount) + ", empty rate : " + ((float)emptyCount / correctionCount));
				}
			}

			confirmRandom++;
		}

		void genPolicy()
		{
			Console.WriteLine("genPolicy ... ");
			//게임 한 판 시작 ---------------------------
			List<Tuple<Board, Move>> recP1 = new List<Tuple<Board, Move>>();
			List<Tuple<Board, Move>> recP2 = new List<Tuple<Board, Move>>();

			//랜덤으로 보드 생성
			//상대방 선수로 놓는다. 어차피 시작하자마자 GetOpposite로 돌릴 거다.
			
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), false);

			//먼저시작하는 쪽이 p1이든 p2든 상관없다.
			bool isP1Turn = false;

			bool isP1Win = false;

			float correctionRate = 0;
			int correctionCount = 0;
			for (int turn = 0; turn < 255; turn++)
			{
				//판을 빙글 돌려서 p2->p1->p2 가 플레이할 수 있도록 해준다.
				//policy network는 항상 아래쪽 세력을 움직여야할 기물로 여기기 때문.
				board = board.GetOpposite();
				isP1Turn = !isP1Turn;

				var proms = tcpCommClient.EvaluatePolicy(board, board.GetAllPossibleMoves(), policyNetName);

				//proms를 기반으로 랜덤으로 고른다.
				Move move = board.GetRandomMove(proms, out float total);
				correctionRate += total;
				correctionCount++;
				

				//움직임을 저장해주고
				if (isP1Turn)
				{
					recP1.Add(new Tuple<Board, Move>(board, move));
				}
				else
				{
					recP2.Add(new Tuple<Board, Move>(board, move));
				}

				//다음보드로.
				board = board.GetNext(move);

				//겜이 끝났는지 확인
				if (board.IsFinished)
				{
					//p1턴이었는데 나 쪽(아래쪽)이 이긴 거면 p1이 이긴거
					//p1턴이 아니었는데 반대쪽(위쪽)이 이긴 것도 p1이 이긴거
					//나머지는 p2가 이긴거
					isP1Win = board.IsMyWin == isP1Turn;
					break;
				}

				//board.PrintStones();
			}

			//턴제한으로 끝났으면 점수로
			if (!board.IsFinished)
			{
				isP1Win = (board.Point > 0) == isP1Turn;
			}

			lock (recWinPolicy)
			{
				if (isP1Win)
				{	
					recWinPolicy.AddRange(recP1);
				}
				else
				{
					recWinPolicy.AddRange(recP2);
				}
				//Console.WriteLine("  remain : " + recWin.Count);
				Console.WriteLine("  correction rate : " + (correctionRate / correctionCount));
			}
		}

		void genMcts()
		{
			Console.WriteLine("genMcts ... ");
			//게임 한 판 시작 ---------------------------
			List<Tuple<Board, Move>> recP1 = new List<Tuple<Board, Move>>();
			List<Tuple<Board, Move>> recP2 = new List<Tuple<Board, Move>>();

			//랜덤으로 보드 생성
			//상대방 선수로 놓는다. 어차피 시작하자마자 GetOpposite로 돌릴 거다.

			RealYame yame = new RealYame(tcpCommClient);
			OnlyPolicy policy = new OnlyPolicy(tcpCommClient);

			Mcts mcts1 = new Mcts(yame);
			Mcts mcts2 = new Mcts(policy);

			mcts1.MaxVisitCount = 500;
			mcts2.MaxVisitCount = 1;

			//먼저시작하는 쪽이 p1이든 p2든 상관없다.
			bool isMyFirst = Global.Rand.NextDouble() > 0.5;
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), isMyFirst);

			bool isMyWin = false;

			mcts1.Init(board);
			mcts2.Init(board);

			for (int turn = 0; turn < 255; turn++)
			{
				Move move;
				Task<Node> task;
				if (board.IsMyTurn)
				{
					task = mcts1.SearchNextAsync();
				}
				else
				{
					task = mcts2.SearchNextAsync();
				}
				task.Wait();
				move = task.Result.prevMove;

				//움직임을 저장해주고
				if (isMyFirst)
				{
					recP1.Add(new Tuple<Board, Move>(board, move));
				}
				else
				{
					recP2.Add(new Tuple<Board, Move>(board, move));
				}

				mcts1.SetMove(move);
				mcts2.SetMove(move);

				board = board.GetNext(move);


				board.PrintStones();

				//겜이 끝났는지 확인
				if (board.IsFinished)
				{
					isMyWin = board.IsMyWin;
					break;
				}
			}

			

			//턴제한으로 끝났으면 점수로
			if (!board.IsFinished)
			{
				isMyWin = (board.Point > 0);
			}

			lock (recWinPolicy)
			{
				if (isMyWin)
				{
					Console.WriteLine("    Collect data : my win");
					var flip = from rec in recP1 select (new Tuple<Board, Move>(rec.Item1.GetFlip(), rec.Item2.GetFlip()));
					recWinPolicy.AddRange(recP1);
					recWinPolicy.AddRange(flip);
					

					var list1 = from rec in recP1 select (new Tuple<Board, float>(rec.Item1, 1.0f));
					var list2 = from rec in recP2 select (new Tuple<Board, float>(rec.Item1.GetOpposite(), 0f));

					var list1Flip = from rec in list1 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));
					var list2Flip = from rec in list2 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));

					recWinValue.AddRange(list1);
					recWinValue.AddRange(list2);
					recWinValue.AddRange(list1Flip);
					recWinValue.AddRange(list2Flip);
				}
				else
				{
					Console.WriteLine("    Collect data : YO win");
					var flip = from rec in recP2 select (new Tuple<Board, Move>(rec.Item1.GetFlip(), rec.Item2.GetFlip()));
					recWinPolicy.AddRange(recP2);
					recWinPolicy.AddRange(flip);


					var list1 = from rec in recP2 select (new Tuple<Board, float>(rec.Item1, 1.0f));
					var list2 = from rec in recP1 select (new Tuple<Board, float>(rec.Item1.GetOpposite(), 0f));

					var list1Flip = from rec in list1 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));
					var list2Flip = from rec in list2 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));

					recWinValue.AddRange(list1);
					recWinValue.AddRange(list2);
					recWinValue.AddRange(list1Flip);
					recWinValue.AddRange(list2Flip);
				}
			}
		}
	}
}
