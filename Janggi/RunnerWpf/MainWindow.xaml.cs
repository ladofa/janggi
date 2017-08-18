using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;

using Janggi;
using Janggi.Ai;

namespace RunnerWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			StageMain.UnitMoved += StageMain_UnitMoved;
		}

		Board mainBoard;

		public enum Controllers
		{
			AI,
			Human,
		}

		Controllers myController;
		Controllers yoController;

		private void ButtonNewGame_Click(object sender, RoutedEventArgs e)
		{
			Board.Tables myTable;
			Board.Tables yoTable;

			bool isMyFirst;


			if (RadioButtonMyInner.IsChecked.Value)
			{
				myTable = Board.Tables.Inner;
			}
			else if (RadioButtonMyOuter.IsChecked.Value)
			{
				myTable = Board.Tables.Outer;
			}
			else if (RadioButtonMyLeft.IsChecked.Value)
			{
				myTable = Board.Tables.Left;
			}
			else
			{
				myTable = Board.Tables.Right;
			}

			if (RadioButtonYoInner.IsChecked.Value)
			{
				yoTable = Board.Tables.Inner;
			}
			else if (RadioButtonYoOuter.IsChecked.Value)
			{
				yoTable = Board.Tables.Outer;
			}
			else if (RadioButtonYoLeft.IsChecked.Value)
			{
				yoTable = Board.Tables.Left;
			}
			else
			{
				yoTable = Board.Tables.Right;
			}

			if (RadioButtonMyFirst.IsChecked.Value)
			{
				isMyFirst = true;
			}
			else
			{
				isMyFirst = false;
			}

			mainBoard = new Board(myTable, yoTable, isMyFirst);

			StageMain.Board = mainBoard;
			mainBoard.Changed += MainBoard_Changed;

			if (RadioButtonMyControllerAI.IsChecked.Value)
			{
				myController = Controllers.AI;
			}
			else if (RadioButtonMyControllerHuman.IsChecked.Value)
			{
				myController = Controllers.Human;
			}

			if (RadioButtonYoControllerAI.IsChecked.Value)
			{
				yoController = Controllers.AI;
			}
			else if (RadioButtonYoControllerHuman.IsChecked.Value)
			{
				yoController = Controllers.Human;
			}

			thread = new Thread(runner);
			thread.Start();
		}

		private void MainBoard_Changed(Board board)
		{
			StageMain.Board = board;
		}

		private void StageMain_UnitMoved(Move move)
		{
			List<Move> moves = mainBoard.GetAllPossibleMoves();
			if (moves.Contains(move))
			{
				userMove = move;
				//사용자 입력이 완료되었음을 알린다.
				userWaiter?.Set();
			}
		}

		Thread thread;
		AutoResetEvent userWaiter;
		Move userMove;
		Mcts mcts;

		void runner()
		{
			mcts = new Mcts();
			PrimaryUcb primaryUcb = new PrimaryUcb();

			mcts.Init(primaryUcb);
			mcts.Init(mainBoard);

			userWaiter = new AutoResetEvent(false);
			Move move = new Move();

			while (true)
			{
				if (mainBoard.IsMyTurn)
				{
					if (myController == Controllers.AI)
					{
						move = mcts.SearchNext().prevMove;
					}
					else if (myController == Controllers.Human)
					{
						StageMain.IsMovable = true;
						//사용자 입력을 기다린다.
						userWaiter.WaitOne();
						move = userMove;

						StageMain.IsMovable = false;
					}
				}
				else
				{
					if (yoController == Controllers.AI)
					{
						move = mcts.SearchNext().prevMove;
					}
					else if (yoController == Controllers.Human)
					{
						StageMain.IsMovable = true;
						//wait human controller
						userWaiter.WaitOne();
						move = userMove;

						StageMain.IsMovable = false;
					}
				}

				//mcts의 상태를 변경해준다. board와는 따로 동작한다.
				//반드시 보드보다 먼저 실행해야 한다. 좀 복잡하네 ㅋㅋ
				mcts.SetMove(move);
				//현재 게임의 상태를 변경.
				mainBoard.MoveNext(move);
				
				
				

				if (mainBoard.IsMyWin)
				{
					MessageBox.Show("내가 이겼다.");
					break;
				}
				else if (mainBoard.IsYoWin)
				{
					MessageBox.Show("상대가 이겼다.");
					break;
				}
			}
		}
	}
}
