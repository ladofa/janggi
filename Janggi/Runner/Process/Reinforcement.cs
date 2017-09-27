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

		//학습시킬 원본
		List<Tuple<Board, Move>> dataPolicy = new List<Tuple<Board, Move>>();
		List<Tuple<Board, float>> dataValue = new List<Tuple<Board, float>>();

		//flip, orientation을 거친 최종본
		List<Tuple<Board, Move>> bufferPolicy = new List<Tuple<Board, Move>>();
		List<Tuple<Board, float>> bufferValue = new List<Tuple<Board, float>>();

		System.Threading.ManualResetEvent signal = new System.Threading.ManualResetEvent(false);
		Janggi.TensorFlow.TcpCommClient tcpCommClient = new Janggi.TensorFlow.TcpCommClient();

		string policyNetName = "policy192";
		string valueNetName = "value192";

		public Reinforcement()
		{
			//genGibo();
			System.Timers.Timer timer = new System.Timers.Timer(600000);
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

			//자료 생성 프로세스
			Task.Run(() =>
			{
				int makingTurn = 0;
				while (running)
				{
					if (dataPolicy.Count > 5000 || dataValue.Count > 5000)
					{
						System.Threading.Thread.Sleep(1000);
					}
					else
					{
						genRanPseudo();
						signal.Set();//만들었으니 학습을 시도하시오 파란불 반짝
					}

					makingTurn++;
				}
			});

			//생성된 자료를 뒤집어서 데이터를 추가하는 프로세스
			Task.Run(() =>
			{
				while (running)
				{
					while (bufferPolicy.Count < 10000 && dataPolicy.Count >= 255)
					{
						List<Tuple<Board, Move>> sub;
						lock (dataPolicy)
						{
							sub = dataPolicy.GetRange(0, 255);
							dataPolicy.RemoveRange(0, 255);
						}

						var sub2 = from e in sub select new Tuple<Board, Move>(e.Item1.GetFlip(), e.Item2.GetFlip());

						lock (bufferPolicy)
						{
							bufferPolicy.AddRange(sub);
							bufferPolicy.AddRange(sub2);
							signal.Set();//만들었으니 학습을 시도하시오 파란불 반짝
						}

					}

					while (bufferValue.Count < 10000 && dataValue.Count >= 255)
					{
						List<Tuple<Board, float>> sub;
						lock (dataValue)
						{
							sub = dataValue.GetRange(0, 255);
							dataValue.RemoveRange(0, 255);
						}

						var subFlip = from e in sub select new Tuple<Board, float>(e.Item1.GetFlip(), e.Item2);
						

						lock (bufferValue)
						{
							bufferValue.AddRange(sub);
							bufferValue.AddRange(subFlip);
							signal.Set();//만들었으니 학습을 시도하시오 파란불 반짝
						}
					}
				}
			});

			//학습 프로세스
			running = true;
			while (running)
			{
				const int setCount = 255 * 2;

				if (bufferPolicy.Count < setCount && bufferValue.Count < setCount)
				{
					signal.Reset();
					signal.WaitOne();
				}
				else if (bufferPolicy.Count > bufferValue.Count)
				{
					Console.WriteLine("train policy ... " + DateTime.Now.ToString());
					List<Tuple<Board, Move>> sub;
					lock (bufferPolicy)
					{
						sub = bufferPolicy.GetRange(0, setCount);
						bufferPolicy.RemoveRange(0, setCount);
					}

					tcpCommClient.TrainPolicy(sub, policyNetName);
					maxVisitedCount += 10;
					Console.WriteLine("    maxVisitedCount : " + maxVisitedCount);
				}
				else
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
					tcpCommClient.TrainValue(sub, valueNetName);
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
			for (int turn = 0; turn < 100; turn++)
			{
				//판을 빙글 돌려서 p2->p1->p2 가 플레이할 수 있도록 해준다.
				//policy network는 항상 아래쪽 세력을 움직여야할 기물로 여기기 때문.
				board = board.GetOpposite();
				isP1Turn = !isP1Turn;

				List<Move> moves = board.GetAllPossibleMoves();
				var proms = tcpCommClient.EvaluatePolicy(board, moves, policyNetName);

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

		void genMcts()
		{
			Console.WriteLine("genMcts ... ");
			//게임 한 판 시작 ---------------------------
			List<Tuple<Board, Move>> recP1 = new List<Tuple<Board, Move>>();
			List<Tuple<Board, Move>> recP2 = new List<Tuple<Board, Move>>();

			//랜덤으로 보드 생성
			//상대방 선수로 놓는다. 어차피 시작하자마자 GetOpposite로 돌릴 거다.

			RealYame yame = new RealYame(tcpCommClient);
			OnlyPolicy policy = new OnlyPolicy(tcpCommClient, policyNetName);

			Mcts mcts1 = new Mcts(yame);
			Mcts mcts2 = new Mcts(policy);

			mcts1.MaxVisitCount = 300;
			mcts2.MaxVisitCount = 1;

			//누가 먼저 시작하나.
			bool isMyFirst = Global.Rand.NextDouble() > 0.5;
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), isMyFirst);

			bool isMyWin = false;

			mcts1.Init(board);
			mcts2.Init(board);

			for (int turn = 0; turn < 100; turn++)
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

				if (move.IsEmpty)
				{
					var moves = board.GetAllPossibleMoves();
					move = moves[Global.Rand.Next(moves.Count - 1)];
				}

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

			lock (dataPolicy)
			{
				if (isMyWin)
				{
					Console.WriteLine("    Collect data : my win");
					var flip = from rec in recP1 select (new Tuple<Board, Move>(rec.Item1.GetFlip(), rec.Item2.GetFlip()));
					dataPolicy.AddRange(recP1);
					dataPolicy.AddRange(flip);


					var list1 = from rec in recP1 select (new Tuple<Board, float>(rec.Item1, 1.0f));
					var list2 = from rec in recP2 select (new Tuple<Board, float>(rec.Item1.GetOpposite(), 0f));

					var list1Flip = from rec in list1 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));
					var list2Flip = from rec in list2 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));

					dataValue.AddRange(list1);
					dataValue.AddRange(list2);
					dataValue.AddRange(list1Flip);
					dataValue.AddRange(list2Flip);
				}
				else
				{
					Console.WriteLine("    Collect data : YO win");
					var flip = from rec in recP2 select (new Tuple<Board, Move>(rec.Item1.GetFlip(), rec.Item2.GetFlip()));
					dataPolicy.AddRange(recP2);
					dataPolicy.AddRange(flip);


					var list1 = from rec in recP1 select (new Tuple<Board, float>(rec.Item1, 0f));
					var list2 = from rec in recP2 select (new Tuple<Board, float>(rec.Item1.GetOpposite(), 1.0f));

					var list1Flip = from rec in list1 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));
					var list2Flip = from rec in list2 select (new Tuple<Board, float>(rec.Item1.GetFlip(), rec.Item2));

					dataValue.AddRange(list1);
					dataValue.AddRange(list2);
					dataValue.AddRange(list1Flip);
					dataValue.AddRange(list2Flip);
				}
			}
		}


		List<string> pathList;
		int pathIndex = 0;
		List<Gibo> gibos = new List<Gibo>();


		void genGibo()
		{
			if (pathList == null)
			{
				Console.WriteLine("read gibos...");
				pathList = new List<string>();

				Search("d:/temp");

				void Search(string path)
				{
					Console.WriteLine("search : " + path);
					string[] files = System.IO.Directory.GetFiles(path, "*.gib");

					foreach (string file in files)
					{
						pathList.Add(file);
					}

					string[] dirs = System.IO.Directory.GetDirectories(path);
					foreach (string dir in dirs)
					{
						Search(dir);
					}
				}
			}


			string curPath = pathList[pathIndex++];
			if (pathIndex == pathList.Count)
			{
				pathIndex = 0;
			}

			Console.WriteLine(" read new Path .. " + curPath);

			Gibo gibo = new Gibo();
			gibo.Read(curPath);

			List<Tuple<Board, Move>> giboPolicy = new List<Tuple<Board, Move>>();
			List<Tuple<Board, float>> giboValue = new List<Tuple<Board, float>>();

			//Parallel.For(0, gibo.historyList.Count, (k) =>
			for (int k = 0; k < gibo.historyList.Count; k++)
			{
				List<Board> history = gibo.historyList[k];
				int isMyWin = gibo.isMyWinList[k];

				for (int i = 0; i < history.Count - 1; i++)
				{
					Board board = history[i];
					Move move = history[i + 1].PrevMove;

					if (board.IsMyTurn)
					{
						giboPolicy.Add(new Tuple<Board, Move>(board, move));
					}
					else
					{
						//policy는 내가 움직인 것으로 돌려서 저장
						giboPolicy.Add(new Tuple<Board, Move>(board.GetOpposite(), move.GetOpposite()));
					}

					//모든 상태를 저장.
					if (isMyWin == -1)
					{
						//상태값 없음.
					}
					else if (board.IsMyTurn)
					{
						giboValue.Add(new Tuple<Board, float>(board, isMyWin));
					}
					else 
					{
						giboValue.Add(new Tuple<Board, float>(board.GetOpposite(), isMyWin == 1 ? 0 : 1));
					}
					
				}
			}//);
			Console.WriteLine($"    {giboPolicy.Count} policies, {giboValue.Count} values.");


			//순서대로 그냥 계속 쳐 넣는다.


			var subPolicy = giboPolicy.GetRange(0, Math.Min(2500, giboPolicy.Count));
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
			MinMax minMax = new MinMax(tcpCommClient, valueNetName);

			bool isMyFirst = Global.Rand.NextDouble() > 0.5;
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), isMyFirst);

			bool isMyWin = false;

			for (int i = 0; i < 80; i++)
			{
				if (board.IsMyTurn)
				{

				}
			}
		}

		List<float> winGames = new List<float>();

		int maxVisitedCount = 500;

		void genRanPseudo()
		{
			Mcts mcts = new Mcts(new PseudoYame());
			mcts.MaxVisitCount = maxVisitedCount;

			bool isMyFirst = Global.Rand.NextDouble() > 0.5;
			Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), isMyFirst);

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

			winGames.Add(isMyWin ? 1 : 0);
			while (winGames.Count > 100)
			{
				winGames.RemoveAt(0);
			}

			float rate = winGames.Average();

			Console.WriteLine("    winning rate : " + rate);
		}
	}
}
