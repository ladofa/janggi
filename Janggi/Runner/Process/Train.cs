using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Janggi;

using Janggi.Ai;


namespace Runner.Process
{
	public class Train
	{
		const int setCount = 512;

		bool running;

		public void Stop()
		{
			running = false;
		}

		//학습시킬 원본
		List<Tuple<Board, Move>> dataPolicy = new List<Tuple<Board, Move>>();
		List<Tuple<Board, float>> dataValue = new List<Tuple<Board, float>>();

		//augmentation을 거친 최종본
		List<Tuple<Board, Move>> bufferPolicy = new List<Tuple<Board, Move>>();
		List<Tuple<Board, float>> bufferValue = new List<Tuple<Board, float>>();

		System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);
		Janggi.TensorFlow.TcpCommClient tcpCommClient = new Janggi.TensorFlow.TcpCommClient();

		public Train()
		{
			//일정 시간에 모델 저장
			System.Timers.Timer saveTimer = new System.Timers.Timer(10/*분*/ * 60 * 1000);
			saveTimer.Elapsed += saveTimer_Elapsed;
			saveTimer.Start();

			Console.WriteLine("Connected!");

			////////////////////////////////////////////////////			

			//학습
			//CPU를 최대로 활용하기 위해 여러 단계로 프로세스를 나눔

			//generation
			Task.Run(() =>
			{
				int makingTurn = 0;
				while (running)
				{
					//자료가 한 쪽이라도 남아있으면 쉰다.
					if (dataPolicy.Count > 5000  || dataValue.Count > 5000)
					{
						System.Threading.Thread.Sleep(1000);
					}
					else
					{
						genGibo();
						//genReinforce();
						signal.Set();//만들었으니 학습을 시도하시오 파란불 반짝
					}

					makingTurn++;
				}
			});

			//Augmentation
			Task.Run(() =>
			{
				while (running)
				{
					while (bufferPolicy.Count < setCount * 20 && dataPolicy.Count >= 5000)
					{
						List<Tuple<Board, Move>> sub;
						lock (dataPolicy)
						{
							sub = dataPolicy.GetRange(0, 5000);
							dataPolicy.RemoveRange(0, 5000);
						}

						var sub2 = from e in sub select new Tuple<Board, Move>(e.Item1.GetFlip(), e.Item2.GetFlip());

						lock (bufferPolicy)
						{
							bufferPolicy.AddRange(sub);
							bufferPolicy.AddRange(sub2);
							signal.Set();//만들었으니 학습을 시도하시오 파란불 반짝
						}
					}

					while (bufferValue.Count < 10000 && dataValue.Count >= 5000)
					{
						List<Tuple<Board, float>> sub;
						lock (dataValue)
						{
							sub = dataValue.GetRange(0, 5000);
							dataValue.RemoveRange(0, 5000);
						}

						var subFlip = from e in sub select new Tuple<Board, float>(e.Item1.GetFlip(), e.Item2);

						lock (bufferValue)
						{
							bufferValue.AddRange(sub);
							bufferValue.AddRange(subFlip);
							signal.Set();//만들었으니 학습을 시도하시오 파란불 반짝
						}
					}

					lock (bufferPolicy)
					{
						Shuffle(bufferPolicy);
					}

					lock (bufferValue)
					{
						Shuffle(bufferValue);
					}

					

					System.Threading.Thread.Sleep(1000);
				}
			});


			//학습 프로세스
			running = true;
			while (running)
			{
				

				while (bufferPolicy.Count < setCount && bufferValue.Count < setCount)
				{
					signal.Reset();
					signal.WaitOne();
				}

				if (bufferPolicy.Count > setCount)
				{
					Console.WriteLine("train policy ... " + DateTime.Now.ToString());
					List<Tuple<Board, Move>> sub;
					lock (bufferPolicy)
					{
						sub = bufferPolicy.GetRange(0, setCount);
						bufferPolicy.RemoveRange(0, setCount);
					}

					tcpCommClient.TrainPolicy(sub);
				}

				if (bufferValue.Count > setCount)
				{
					Console.WriteLine("train value ... " + DateTime.Now.ToString());
					List<Tuple<Board, float>> sub;
					lock (bufferValue)
					{
						sub = bufferValue.GetRange(0, setCount);
						if (sub[0] == null) throw new Exception("???");

						bufferValue.RemoveRange(0, setCount);
						if (sub[0] == null) throw new Exception("???");
					}

					if (sub[0] == null) throw new Exception("???");
					tcpCommClient.TrainValue(sub);
				}
			}

			tcpCommClient.Disconnect();
		}

		private void saveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.WriteLine("save ...");
			tcpCommClient.SaveModel(Janggi.TensorFlow.NetworkKinds.Policy);
			tcpCommClient.SaveModel(Janggi.TensorFlow.NetworkKinds.Value);
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
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), true, false);

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
					var proms = tcpCommClient.EvaluatePolicy(board, board.GetAllPossibleMoves());

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
				int r = Global.Rand.Next(possibleMoves.Count - 1);
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

			lock (dataPolicy)
			{
				var flip = from r in rec select (new Tuple<Board, Move>(r.Item1.GetFlip(), r.Item2.GetFlip()));
				dataPolicy.AddRange(rec);
				dataPolicy.AddRange(flip);

				var list1 = from r in rec select (new Tuple<Board, float>(r.Item1, r.Item1.Point > 0 ? 1 : 0));
				var list2 = from r in flip select (new Tuple<Board, float>(r.Item1, r.Item1.Point > 0 ? 1 : 0));

				var list1_op = from r in rec select (new Tuple<Board, float>(r.Item1.GetOpposite(), r.Item1.Point > 0 ? 0 : 1));
				var list2_op = from r in flip select (new Tuple<Board, float>(r.Item1.GetOpposite(), r.Item1.Point > 0 ? 0 : 1));

				dataValue.AddRange(list1);
				dataValue.AddRange(list2);
				dataValue.AddRange(list1_op);
				dataValue.AddRange(list2_op);

				//Console.WriteLine("  remain : " + recWin.Count);
				if (correctionCount > 0)
				{
					Console.WriteLine("  * correction rate : " + (correctionRate / correctionCount) + ", empty rate : " + ((float)emptyCount / correctionCount));
				}
			}

			confirmRandom++;
		}

		/// <summary>
		/// policy network만으로 서로 경기를 한다.
		/// </summary>
		void genPolicy()
		{
			Console.WriteLine("genPolicy ... ");
			//게임 한 판 시작 ---------------------------
			List<Tuple<Board, Move>> recP1 = new List<Tuple<Board, Move>>();
			List<Tuple<Board, Move>> recP2 = new List<Tuple<Board, Move>>();

			//랜덤으로 보드 생성
			//상대방 선수로 놓는다. 어차피 시작하자마자 GetOpposite로 돌릴 거다.

			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), true, false);

			//먼저시작하는 쪽이 p1이든 p2든 상관없다.
			bool isP1Turn = false;

			bool isP1Win = false;

			float correctionRate = 0;
			int correctionCount = 0;
			for (int turn = 0; turn < 100; turn++)
			{
				//판을 빙글 돌려서 p2->p1->p2 가 플레이할 수 있도록 해준다.
				//policy network는 항상 아래쪽 세력을 움직여야할 기물로 여기기 때문.
				board = board.GetOpposite();
				isP1Turn = !isP1Turn;

				List<Move> moves = board.GetAllPossibleMoves();
				var proms = tcpCommClient.EvaluatePolicy(board, moves);

				//proms를 기반으로 랜덤으로 고른다.
				Move move = board.GetRandomMove(proms, out float total);
				if (move.IsEmpty)
				{
					move = moves[Global.Rand.Next(moves.Count - 1)];
				}
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

			lock (dataPolicy)
			{
				if (isP1Win)
				{
					dataPolicy.AddRange(recP1);
				}
				else
				{
					dataPolicy.AddRange(recP2);
				}
				//Console.WriteLine("  remain : " + recWin.Count);
				Console.WriteLine("  correction rate : " + (correctionRate / correctionCount));
			}
		}

		List<string> pathList;

		void genGibo()
		{
			if (pathList == null || pathList.Count == 0)
			{
				Console.WriteLine("read gibos...");

				string[] allfiles = Directory.GetFiles("d:/dataset/gibo", "*.gib", SearchOption.AllDirectories);
				pathList = allfiles.ToList();
			}

			//pop
			string curPath = pathList[0];
			pathList.RemoveAt(0);

			Console.WriteLine(" read new Path .. " + curPath);

			List<Gibo> gibos = Gibo.Read(curPath);

			List<Tuple<Board, Move>> giboPolicy = new List<Tuple<Board, Move>>();
			List<Tuple<Board, float>> giboValue = new List<Tuple<Board, float>>();


			foreach (Gibo gibo in gibos)
			{
				List<Board> history = gibo.GetParsed();
				int isMyWin = gibo.isMyWin;
				int isYoWin = 1 - isMyWin;

				for (int i = 0; i < history.Count; i++)
				{
					Board board = history[i];

					if (board.IsMyTurn)
					{
						//Console.WriteLine("MY TURN");
						//board.PrintStones();
						if (i < history.Count - 1)
						{
							Move move = history[i + 1].PrevMove;
							giboPolicy.Add(new Tuple<Board, Move>(board, move));
						}

						if (i > 10 && isMyWin != -1)
						{
							giboValue.Add(new Tuple<Board, float>(board, isMyWin));
						}
					}
					else
					{
						
						//항상 두려고 하는 쪽을 아래에 배치한다.
						board = board.GetOpposite();

						//Console.WriteLine("YO TURN");
						//board.PrintStones();
						if (i < history.Count - 1)
						{
							Move move = history[i + 1].PrevMove.GetOpposite();
							giboPolicy.Add(new Tuple<Board, Move>(board, move));
						}

						if (i > 10 && isMyWin != -1)
						{
							giboValue.Add(new Tuple<Board, float>(board, isYoWin));
						}
					}
				}
			}

			Console.WriteLine($"    {giboPolicy.Count} policies, {giboValue.Count} values.");

			

			//순서대로 그냥 계속 쳐 넣는다.
			//var subPolicy = giboPolicy.GetRange(0, Math.Min(2500, giboPolicy.Count));
			lock (dataPolicy)
			{
				dataPolicy.AddRange(giboPolicy);
			}

			lock (dataValue)
			{
				dataValue.AddRange(giboValue);
			}
		}

		void genMinMax()
		{
			MinMax minMax = new MinMax(tcpCommClient);

			//bool isMyFirst = Global.Rand.NextDouble() > 0.5;
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), true, false);

			bool isMyWin = false;

			for (int i = 0; i < 80; i++)
			{
				if (board.IsMyTurn)
				{

				}
			}
		}

		List<float> winGames2 = new List<float>();
		List<float> winGames3 = new List<float>();

		int maxVisitedCount = 500;

		void genRanPseudo()
		{
			Mcts mcts = new Mcts(new PseudoYame());
			mcts.MaxVisitCount = maxVisitedCount;

			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), true, false);

			mcts.Init(board);

			List<Tuple<Board, Move>> moves1 = new List<Tuple<Board, Move>>();
			List<Tuple<Board, Move>> moves2 = new List<Tuple<Board, Move>>();

			bool isMyWin = false;


			for (int i = 0; i < 80; i++)
			{
				Move move;
				if (board.IsMyTurn)
				{
					var task = mcts.SearchNextAsync();
					task.Wait();
					move = task.Result.prevMove;
					moves1.Add(new Tuple<Board, Move>(board, move));
				}
				else
				{
					mcts.root.PrepareMoves();
					List<Move> moves = mcts.root.moves;
					move = moves[Global.Rand.Next(moves.Count)];
					moves2.Add(new Tuple<Board, Move>(board, move));
				}

				mcts.SetMove(move);

				board = board.GetNext(move);

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

			lock (dataPolicy)
			{
				if (isMyWin)
				{
					dataPolicy.AddRange(moves1);
				}
				else
				{
					dataPolicy.AddRange(moves2);
				}
			}

			lock (dataValue)
			{
				if (isMyWin)
				{
					var vals1 = from e in moves1 select new Tuple<Board, float>(e.Item1, 1);
					dataValue.AddRange(vals1);
				}
				else
				{
					var vals1 = from e in moves1 select new Tuple<Board, float>(e.Item1, 0);
					dataValue.AddRange(vals1);
				}
			}

			winGames2.Add(isMyWin ? 1 : 0);
			while (winGames2.Count > 100)
			{
				winGames2.RemoveAt(0);
			}

			winGames3.Add(isMyWin ? 1 : 0);
			while (winGames3.Count > 1000)
			{
				winGames3.RemoveAt(0);
			}

			float rate2 = winGames2.Average();
			float rate3 = winGames3.Average();

			Console.WriteLine("    winning rate : " + rate2 + ", " + rate3);
		}

		/// <summary>
		/// 두 네트워크가 서로 싸움을 해서 그 기보를 학습 데이터로 내보낸다.
		/// </summary>
		void genReinforce()
		{
			//Mcts mcts1 = new Mcts(new OnlyPolicy(tcpCommClient));
			Mcts mcts1 = new Mcts(new RealYame(tcpCommClient));
			Mcts mcts2 = new Mcts(new RealYame(tcpCommClient));
			mcts1.MaxVisitCount = 500;
			mcts2.MaxVisitCount = 500;

			//bool isMyFirst = Global.Rand.Next() % 2 == 0;
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), true, false);


			mcts1.Init(new Board(board));
			mcts2.Init(new Board(board));

			List<Tuple<Board, Move>> moves1 = new List<Tuple<Board, Move>>();
			List<Tuple<Board, Move>> moves2 = new List<Tuple<Board, Move>>();

			bool isMyWin = false;


			for (int i = 0; i < 80; i++)
			{
				Move move;
				if (board.IsMyTurn)
				{
					var task = mcts1.SearchNextAsync();
					task.Wait();
					move = task.Result.prevMove;
					moves1.Add(new Tuple<Board, Move>(board, move));
				}
				else
				{
					var task = mcts2.SearchNextAsync();
					task.Wait();
					move = task.Result.prevMove;
					moves2.Add(new Tuple<Board, Move>(board, move));
				}

				mcts1.SetMove(move);
				mcts2.SetMove(move);

				board = board.GetNext(move);

				//겜이 끝났는지 확인
				if (board.IsFinished)
				{
					isMyWin = board.IsMyWin;
					break;
				}

				//mcts2.MaxVisitCount += 200;
			}

			//턴제한으로 끝났으면 점수로
			if (!board.IsFinished)
			{
				isMyWin = (board.Point > 0);
			}

			lock (dataPolicy)
			{
				if (isMyWin)
				{
					dataPolicy.AddRange(moves1);
				}
				else
				{
					dataPolicy.AddRange(moves2);
				}
			}

			lock (dataValue)
			{
				if (isMyWin)
				{
					var vals1 = from e in moves1 select new Tuple<Board, float>(e.Item1, 1);
					var vals2 = from e in moves2 select new Tuple<Board, float>(e.Item1.GetOpposite(), 0);
					dataValue.AddRange(vals1);
					dataValue.AddRange(vals2);
				}
				else
				{
					var vals1 = from e in moves1 select new Tuple<Board, float>(e.Item1, 0);
					var vals2 = from e in moves2 select new Tuple<Board, float>(e.Item1.GetOpposite(), 1);
					dataValue.AddRange(vals1);
					dataValue.AddRange(vals2);
				}
			}

		}

		public Random random = new Random();

		void Shuffle<T>(List<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n -= 1;
				int k = random.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
