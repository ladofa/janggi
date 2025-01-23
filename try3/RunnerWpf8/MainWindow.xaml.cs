using Janggi.Game;
using Janggi;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using static Janggi.Game.Board;
using static Janggi.Game.Stones;

namespace RunnerWpf8
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
            TextBoxMaxVisitCount.Text = "50000";
        }

        Board _mainBoard;
        Board mainBoard
        {
            get { return _mainBoard; }
            set
            {
                _mainBoard = value;
                MainBoard_Changed(value);
            }
        }

        public enum Controllers
        {
            AI,
            Human,
        }

        Controllers myController;
        Controllers yoController;


        private void ButtonNewGame_Click(object sender, RoutedEventArgs e)
        {
            Startings myTable;
            Startings yoTable;

            bool isMyFirst;


            if (RadioButtonMyInner.IsChecked ?? false)
            {
                myTable = Startings.SMMS;
            }
            else if (RadioButtonMyOuter.IsChecked ?? false)
            {
                myTable = Startings.MSSM;
            }
            else if (RadioButtonMyLeft.IsChecked ?? false)
            {
                myTable = Startings.MSMS;
            }
            else
            {
                myTable = Startings.SMSM;
            }

            if (RadioButtonYoInner.IsChecked ?? false)
            {
                yoTable = Startings.SMMS;
            }
            else if (RadioButtonYoOuter.IsChecked ?? false)
            {
                yoTable = Startings.MSSM;
            }
            else if (RadioButtonYoLeft.IsChecked ?? false)
            {
                yoTable = Startings.MSMS;
            }
            else
            {
                yoTable = Startings.SMSM;
            }

            if (RadioButtonMyFirst.IsChecked ?? false)
            {
                isMyFirst = true;
            }
            else
            {
                isMyFirst = false;
            }

            mainBoard = new Board(myTable, yoTable);

            StageMain.Board = mainBoard;

            if (RadioButtonMyControllerAI.IsChecked ?? false)
            {
                myController = Controllers.AI;
            }
            else if (RadioButtonMyControllerHuman.IsChecked ?? false)
            {
                myController = Controllers.Human;
            }

            if (RadioButtonYoControllerAI.IsChecked ?? false)
            {
                yoController = Controllers.AI;
            }
            else if (RadioButtonYoControllerHuman.IsChecked ?? false)
            {
                yoController = Controllers.Human;
            }


            //종료 절차
            
            isRunning = false;
            if (userWaiter != null) userWaiter.Set();
            mcts.ProgressUpdated -= Mcts_ProgressUpdated;
            //ResumeSearching();
            mcts.StopAndWaitSearchNodes();
            thread?.Join();

            mcts.Init(mainBoard);
            mcts.ProgressUpdated += Mcts_ProgressUpdated;
            //ResumeSearching();

            thread = new Thread(runner);
            thread.Start();
        }

        //int maxVisitCount = 500;

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
            List<Move> moves = mainBoard.moves;
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
                    timer.Elapsed += Timer_Elapsed1;
                    timer.Interval = maxGivenTime * 1000;
                    timer.Start();
                }

                //최대 탐색 노드 설정
                
                //탐색 시작
                mcts.StartSearchNodes(maxVisitCount);

                if (stepByStep)
                {
                    PauseSearching();
                }

                mcts.WaitSearchNodes();
                timer?.Stop();
                timer = null;

                var r =  mcts.root.BestChoice();
                return r.move;
            }
            else if (controller == Controllers.Human)
            {
                if (thinkAlways)
                {
                    maxVisitCount = int.MaxValue;
                    mcts.StartSearchNodes(maxVisitCount);
                }

                StageMain.IsMovable = true;
                //사용자 입력을 기다린다.
                userWaiter.WaitOne();
                StageMain.IsMovable = false;

                mcts.StopSearchNodes();
                mcts.WaitSearchNodes();

                return userMove;
            }
            else
            {
                throw new Exception("undefined controller");
            }
        }

        private void Timer_Elapsed1(object? sender, System.Timers.ElapsedEventArgs e)
        {
            mcts.StopSearchNodes();
        }

      
        Thread thread;
        AutoResetEvent userWaiter;
        Move userMove;
        Mcts mcts = new Mcts();

        bool isRunning;

        void runner()
        {
            userWaiter = new AutoResetEvent(false);
            Move move = Move.Rest;

            isRunning = true;

            while (isRunning)
            {
                if (IS_SIDE(mainBoard.player, CHO))
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
                mcts.Step(move);
                //현재 게임의 상태를 변경.
                mainBoard = mcts.root.board;
                

                if (mainBoard.winner == 1)
                {
                    MessageBox.Show("내가 이겼다.");
                    break;
                }
                else if (mainBoard.winner == -1)
                {
                    MessageBox.Show("상대가 이겼다.");
                    break;
                }

                TreeViewerMain.Load(mcts);
            }
        }

        private void ButtonForceAI_Click(object sender, RoutedEventArgs e)
        {
            mcts.StopSearchNodes();
        }


        int maxVisitCount;
        private void TextBoxMaxVisitCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
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
                if (mcts.IsSearching == false)
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
            mcts.StopSearchNodes();

            Dispatcher.Invoke(() =>
            {
                ButtonStartAI.Content = "AI 생각 시작";
            });
        }

        void ResumeSearching()
        {
            mcts.StartSearchNodes(maxVisitCount);

            Dispatcher.Invoke(() =>
            {
                ButtonStartAI.Content = "AI 잠시 멈춤";
            });
        }
    }
}