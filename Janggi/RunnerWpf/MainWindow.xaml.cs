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


			//종료 절차
			if (mcts != null)
			{
				isRunning = false;
				userWaiter.Set();
				mcts.ProgressUpdated -= Mcts_ProgressUpdated;
				ResumeSearching();
				mcts.ForceStopSearch();
				thread?.Join();
			}

			PrimaryUcb primaryUcb = new PrimaryUcb();
			mcts = new Mcts(primaryUcb);
			TextBoxMaxVisitCount.Text = mcts.MaxVisitCount.ToString();
			mcts.Init(mainBoard);
			mcts.ProgressUpdated += Mcts_ProgressUpdated;
			ResumeSearching();

			thread = new Thread(runner);
			thread.Start();
		}

		private void Mcts_ProgressUpdated(Mcts mcts, int visit, double rate)
		{
			Dispatcher.Invoke(() =>
			{
				ProgressBarVisitCount.Value = rate * 100;
				TextVisitCount.Text = visit.ToString();
			});
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


		Move waitInput(Controllers controller)
		{
			if (controller == Controllers.AI)
			{
				//타이머
				System.Timers.Timer timer = null;
				if (isCheckedTimer)
				{
					timer = new System.Timers.Timer();
					timer.Elapsed += Timer_Elapsed;
					timer.Interval = maxGivenTime * 1000;
					timer.Start();
				}

				//최대 탐색 노드 설정
				mcts.MaxVisitCount = maxVisitCount;

				//탐색 시작
				var task = mcts.SearchNextAsync();

				if (stepByStep)
				{
					PauseSearching();
				}

				task.Wait();		

				timer?.Stop();
				timer = null;
				return task.Result.prevMove;
			}
			else if (myController == Controllers.Human)
			{
				Task<Node> task = null;
				if (thinkAlways)
				{
					mcts.MaxVisitCount = int.MaxValue;
					task = mcts.SearchNextAsync();
				}
				
				StageMain.IsMovable = true;
				//사용자 입력을 기다린다.
				userWaiter.WaitOne();
				StageMain.IsMovable = false;

				mcts.ForceStopSearch();
				task?.Wait();

				return userMove;
			}
			else
			{
				throw new Exception("undefined controller");
			}
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			mcts.ForceStopSearch();
		}

		Thread thread;
		AutoResetEvent userWaiter;
		Move userMove;
		Mcts mcts;

		bool isRunning;

		void runner()
		{
			userWaiter = new AutoResetEvent(false);
			Move move = new Move();

			isRunning = true;

			while (isRunning)
			{
				if (mainBoard.IsMyTurn)
				{
					move = waitInput(myController);
				}
				else
				{
					move = waitInput(yoController);
				}

				if (!isRunning)
				{
					break;
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

				TreeViewerMain.Load(mcts);
			}
		}

		private void ButtonForceAI_Click(object sender, RoutedEventArgs e)
		{
			mcts.ForceStopSearch();
		}

		int maxVisitCount;
		private void TextBoxMaxVisitCount_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			int.TryParse(textBox.Text, out maxVisitCount);
		}

		bool isCheckedTimer;
		int maxGivenTime;

		private void CheckBoxTimer_Checked(object sender, RoutedEventArgs e)
		{
			isCheckedTimer = true;
			TextBoxTimer.Text = "5";
		}

		private void CheckBoxTimer_Unchecked(object sender, RoutedEventArgs e)
		{
			isCheckedTimer = false;
		}

		private void TextBoxTimer_TextChanged(object sender, TextChangedEventArgs e)
		{
			int.TryParse(TextBoxTimer.Text, out maxGivenTime);
		}

		bool thinkAlways;
		private void CheckBoxThinkAlways_Checked(object sender, RoutedEventArgs e)
		{
			thinkAlways = true;
		}

		private void CheckBoxThinkAlways_Unchecked(object sender, RoutedEventArgs e)
		{
			thinkAlways = false;
		}

		bool stepByStep;
		private void CheckBoxStepByStep_Checked(object sender, RoutedEventArgs e)
		{
			stepByStep = false;
		}

		private void CheckBoxStepByStep_Unchecked(object sender, RoutedEventArgs e)
		{
			stepByStep = true;
		}

		private void ButtonStartAI_Click(object sender, RoutedEventArgs e)
		{
			if (mcts != null)
			{
				if (mcts.IsPaused)
				{
					ResumeSearching();
				}
				else
				{
					PauseSearching();
				}
			}
		}

		void PauseSearching()
		{
			if (mcts != null)
			{
				mcts.PauseSearching();
			}

			Dispatcher.Invoke(() =>
			{
				ButtonStartAI.Content = "AI 생각 시작";
			});
		}

		void ResumeSearching()
		{
			if (mcts != null)
			{
				mcts.ResumeSearching();
			}

			Dispatcher.Invoke(() =>
			{
				ButtonStartAI.Content = "AI 잠시 멈춤";
			});
		}
	}
}
