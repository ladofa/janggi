using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Janggi;

namespace Runner.Process
{
	class PossibleMove
	{
		public PossibleMove()
		{
			//openServerAsync();
			Janggi.TensorFlow.TcpCommClient tcpCommClient = new Janggi.TensorFlow.TcpCommClient();
			while (!tcpCommClient.Connect("localhost", 9999))
			{
				Console.WriteLine("ConnectionFailed.");
				System.Threading.Thread.Sleep(2000);
			}

			Console.WriteLine("Connected!");

			bool succeed = tcpCommClient.LoadModel(Janggi.TensorFlow.TcpCommClient.NetworkKinds.Policy, "possible_move", "possible_move");
			if (succeed)
			{
				Console.WriteLine("LOAD succeed.");
			}
			else
			{
				Console.WriteLine("CREATE new network.");
			}

			int patch = 0;

			//자료를 모으고
			while (true)
			{
				List<Tuple<Board, Move>> recWin = new List<Tuple<Board, Move>>();

				//대충 2550개정도로 모아볼까.
				while (recWin.Count < 255 * 50)
				{
					//게임 한 판 시작 ---------------------------
					List<Tuple<Board, Move>> recP1 = new List<Tuple<Board, Move>>();
					List<Tuple<Board, Move>> recP2 = new List<Tuple<Board, Move>>();

					//랜덤으로 보드 생성
					//상대방 선수로 놓는다. 어차피 시작하자마자 GetOpposite로 돌릴 거다.
					Console.WriteLine("new game start");
					Board board = new Board((Board.Tables)Global.Rand.Next(4), (Board.Tables)Global.Rand.Next(4), false);

					//먼저시작하는 쪽이 p1이든 p2든 상관없다.
					bool isP1Turn = false;

					bool isP1Win = false;

					for (int turn = 0; turn < 255; turn++)
					{
						
						//빙글 돌려서 p2->p1->p2 가 플레이할 수 있도록 해준다.
						//policy network는 항상 아래쪽 세력을 움직여야할 기물로 여기기 때문.
						board = board.GetOpposite();
						isP1Turn = !isP1Turn;

						Move move;
						if (turn > 10)
						{

							List<Move> possibleMoves = board.GetAllPossibleMoves();
							foreach (Move e in possibleMoves)
							{
								recWin.Add(new Tuple<Board, Move>(board, e));
							}
							int r = Global.Rand.Next(possibleMoves.Count);
							move = possibleMoves[r];
						}
						else
						{
							Console.Write("turn : " + turn + "   ");
							var proms = tcpCommClient.EvaluatePolicy(board, "possible_move");

							//proms를 기반으로 랜덤으로 고른다.
							move = board.GetRandomMove(proms);
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

					board.PrintStones();


					lock (recWin)
					{
						if (isP1Win)
						{
							Console.WriteLine("winner - p1");
							recWin.AddRange(recP1);
						}
						else
						{
							Console.WriteLine("winner - p2");
							recWin.AddRange(recP2);
						}
						Console.WriteLine("collected count : " + recWin.Count);
					}
				}

				//학습
				Console.WriteLine("train..." + patch);
				tcpCommClient.TrainPolicy(recWin, "possible_move");

				if (patch++ % 5 == 0)
				{
					Console.WriteLine("and save...");
					tcpCommClient.SaveModel(Janggi.TensorFlow.TcpCommClient.NetworkKinds.Policy, "possible_move", "possible_move");
				}
			}

			tcpCommClient.Disconnect();
		}
	}
}
