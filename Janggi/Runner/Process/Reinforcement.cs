using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Janggi;




namespace Runner.Process
{
	public class Reinforcement
	{
		bool running;

		public void Stop()
		{
			running = false;
		}

		List<Tuple<Board, Move>> recWin = new List<Tuple<Board, Move>>();
		System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);
		Janggi.TensorFlow.TcpCommClient tcpCommClient = new Janggi.TensorFlow.TcpCommClient();

		string netName = "random128";

		public Reinforcement()
		{
			System.Timers.Timer timer = new System.Timers.Timer(300000);
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
			while (!tcpCommClient.Connect("localhost", 9999))
			{
				Console.WriteLine("ConnectionFailed.");
				System.Threading.Thread.Sleep(2000);
			}

			Console.WriteLine("Connected!");

			bool succeed = tcpCommClient.LoadModel(Janggi.TensorFlow.TcpCommClient.NetworkKinds.Policy, netName, netName);
			if (succeed)
			{
				Console.WriteLine("LOAD succeed.");
			}
			else
			{
				Console.WriteLine("CREATE new network.");
			}

			////////////////////////////////////////////////////			

			//학습
			Console.WriteLine("train...");

			//자료 모으기 프로세스
			Task.Run(()=>
			{
				while (running)
				{
					if (recWin.Count > 10000)
					{
						System.Threading.Thread.Sleep(1000);
					}
					else
					{
						genRandom();
						signal.Set();
					}
				}
			});

			//학습 프로세스
			running = true;
			while (running)
			{
				const int setCount = 255 * 3;
				if (recWin.Count >= setCount)
				{
					Console.WriteLine("train ... " + DateTime.Now.ToString());
					tcpCommClient.TrainPolicy(recWin.GetRange(0, setCount), netName);

					lock (recWin)
					{
						recWin.RemoveRange(0, setCount);
						//Console.WriteLine("  remain : " + recWin.Count);
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
			tcpCommClient.SaveModel(Janggi.TensorFlow.TcpCommClient.NetworkKinds.Policy, netName, netName);
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
			int correctionCount = 0;
			for (int turn = 0; turn < 200; turn++)
			{
				//빙글 돌려서 p2->p1->p2 가 플레이할 수 있도록 해준다.
				//policy network는 항상 아래쪽 세력을 움직여야할 기물로 여기기 때문.
				board = board.GetOpposite();
				isP1Turn = !isP1Turn;

				Move move;
				if (turn < 10 && confirmRandom % 20 == 0)
				{
					var proms = tcpCommClient.EvaluatePolicy(board, netName);

					//proms를 기반으로 랜덤으로 고른다.
					Move move2 = board.GetRandomMove(proms, out float total);
					correctionRate += total;
					correctionCount++;
				}

				List<Move> possibleMoves = board.GetAllPossibleMoves();
				int r = Global.Rand.Next(possibleMoves.Count);
				move = possibleMoves[r];

				if (turn > 20)
					rec.Add(new Tuple<Board, Move>(board, move));

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

			lock (recWin)
			{
				recWin.AddRange(rec);
				//Console.WriteLine("  remain : " + recWin.Count);
				if (correctionCount > 0)
				{
					Console.WriteLine("  * correction rate : " + (correctionRate / correctionCount));
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
				//빙글 돌려서 p2->p1->p2 가 플레이할 수 있도록 해준다.
				//policy network는 항상 아래쪽 세력을 움직여야할 기물로 여기기 때문.
				board = board.GetOpposite();
				isP1Turn = !isP1Turn;

				
				
				var proms = tcpCommClient.EvaluatePolicy(board, netName);

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

			lock (recWin)
			{
				if (isP1Win)
				{	
					recWin.AddRange(recP1);
				}
				else
				{
					recWin.AddRange(recP2);
				}
				//Console.WriteLine("  remain : " + recWin.Count);
				Console.WriteLine("  correction rate : " + (correctionRate / correctionCount));
			}
		}
	}
}
