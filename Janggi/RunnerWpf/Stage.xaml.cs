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


using Janggi;
using static Janggi.StoneHelper;

namespace RunnerWpf
{
	/// <summary>
	/// Interaction logic for Stage.xaml
	/// </summary>
	public partial class Stage : UserControl
	{
		public Unit[,] units = new Unit[Board.Height, Board.Width];

		public Stage()
		{
			InitializeComponent();

			//유닛 배치
			for (int y = 0; y < Board.Height; y++)
			{
				for (int x = 0; x < Board.Width; x++)
				{
					Unit unit = new Unit();
					unit.SetValue(Grid.RowProperty, y);
					unit.SetValue(Grid.ColumnProperty, x);
					unit.Pos = new Pos(x, y);
					unit.MouseLeftButtonDown += Unit_MouseLeftButtonDown;
					unit.MouseLeftButtonUp += Unit_MouseLeftButtonUp;

					units[y, x] = unit;
					GridStage.Children.Add(unit);
				}
			}

			SizeChanged += Stage_SizeChanged;
			MouseLeave += Stage_MouseLeave;
			MouseMove += Stage_MouseMove;
			MouseLeftButtonUp += Stage_MouseLeftButtonUp;
		}

		

		public delegate void UnitMovedHandler(Move move);
		public event UnitMovedHandler UnitMoved;

		bool isUnitMoving;
		Pos moveFrom;
		Pos moveTo;

		public bool IsMovable
		{
			get; set;
		}

		void showPossibleMove(Pos pos)
		{
			List<Pos> moves = board.GetAllMoves(pos);
			Brush brush;

			if (IsMine(board[pos]))
			{
				brush = Brushes.Blue;
			}
			else
			{
				brush = Brushes.Red;
			}

			foreach (Pos t in moves)
			{
				units[t.Y, t.X].IsCircled = true;
				units[t.Y, t.X].CircleBrush = brush;
			}
		}

		void hideAllPossibleMove()
		{
			foreach (Unit unit in units)
			{
				unit.IsCircled = false;
			}
		}

		private void Unit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Unit unit = sender as Unit;

			if (IsEmpty(unit.Stone) || !IsMovable)
			{
				return;
			}

			moveFrom = unit.Pos;

			UnitMoving.Visibility = Visibility.Visible;
			showPossibleMove(moveFrom);
			isUnitMoving = true;

			//오버레이 UI 설정
			UnitMoving.SetStone(unit.Stone, true);
			UnitMoving.Width = unit.ActualWidth;
			UnitMoving.Height = unit.ActualHeight;

			//오버레이 초기 위치
			Point pos = e.GetPosition(GridOverlay);
			UnitMoving.Margin = new Thickness(pos.X - UnitMoving.Width / 2, pos.Y - UnitMoving.Height / 2, 0, 0);

			//디버그용
			DrawEtcMarkers();
		}

		void DrawEtcMarkers()
		{
			var movingStone = UnitMoving.Stone;
			for (int y = 0; y < Board.Height; y++)
			{
				for (int x = 0; x < Board.Width; x++)
				{
					if ((board.targets[y, x] & movingStone) != 0)
					{
						units[y, x].IsMarkedTarget = true;
					}
					else
					{
						units[y, x].IsMarkedTarget = false;
					}

					if ((board.blocks[y, x] & movingStone) != 0)
					{
						units[y, x].IsMarkedBlock = true;
					}
					else
					{
						units[y, x].IsMarkedBlock = false;
					}
				}
			}
		}

		private void Unit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (isUnitMoving)
			{
				Unit unit = sender as Unit;
				moveTo = unit.Pos;
				//UnitMoving.SetStone(unit.Stone, board.IsMyFirst);
				UnitMoving.Width = unit.ActualWidth;
				UnitMoving.Height = unit.ActualHeight;

				UnitMoving.Visibility = Visibility.Collapsed;
				hideAllPossibleMove();
				isUnitMoving = false;

				UnitMoved?.Invoke(new Move(moveFrom, moveTo));
			}
		}		

		private void Stage_MouseLeave(object sender, MouseEventArgs e)
		{
			UnitMoving.Visibility = Visibility.Collapsed;
			hideAllPossibleMove();
			isUnitMoving = false;
		}

		private void Stage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			UnitMoving.Visibility = Visibility.Collapsed;
			hideAllPossibleMove();
			isUnitMoving = false;
		}

		private void Stage_MouseMove(object sender, MouseEventArgs e)
		{
			if (isUnitMoving)
			{
				//오버레이 위치 변경
				Point pos = e.GetPosition(GridOverlay);
				UnitMoving.Margin = new Thickness(pos.X - UnitMoving.Width / 2, pos.Y - UnitMoving.Height / 2, 0, 0);
			}
		}

		static List<Tuple<double, double, double, double>> castlePoints = new List<Tuple<double, double, double, double>>
			{
				new Tuple<double, double, double, double>(3, 0, 5, 2),
				new Tuple<double, double, double, double>(3, 2, 5, 0),
				new Tuple<double, double, double, double>(3, 7, 5, 9),
				new Tuple<double, double, double, double>(3, 9, 5, 7),
			};

		static List<Tuple<double, double>> makrPoints = new List<Tuple<double, double>>
			{
				new Tuple<double, double>(0, 3), new Tuple<double, double>(0, 6),
				new Tuple<double, double>(2, 3), new Tuple<double, double>(2, 6),
				new Tuple<double, double>(4, 3), new Tuple<double, double>(4, 6),
				new Tuple<double, double>(6, 3), new Tuple<double, double>(6, 6),
				new Tuple<double, double>(8, 3), new Tuple<double, double>(8, 6),
				new Tuple<double, double>(1, 2), new Tuple<double, double>(1, 7),
				new Tuple<double, double>(7, 2), new Tuple<double, double>(7, 7),
			};

		private void Stage_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (ActualHeight == 0)
			{
				return;
			}

			const double originalRate = 1.08;
			double currentRate = ActualWidth / ActualHeight;

			double woodWidth;
			double woodHeight;

			if (currentRate > originalRate)
			{
				//가로가 길다면 세로에 맞춰준다.
				woodHeight = ActualHeight;
				woodWidth = woodHeight * originalRate;
			}
			else
			{
				woodWidth = ActualWidth;
				woodHeight = woodWidth / originalRate;
			}

			GridWood.Width = woodWidth;
			GridWood.Height = woodHeight;

			double markLength = woodWidth / 100;

			GridLine.Children.Clear();
			for (int y = 0; y < 10; y++)
			{
				Line line = new Line();
				line.X1 = woodWidth * 1 / 18;
				line.Y1 = woodHeight * (2 * y + 1) / 20;
				line.X2 = woodWidth * 17 / 18;
				line.Y2 = line.Y1;
				line.Stroke = Brushes.Black;
				line.StrokeThickness = 1;
				GridLine.Children.Add(line);
			}

			List<Line> lines = new List<Line>();

			//격자라인
			for (int x = 0; x < 9; x++)
			{
				Line line = new Line();
				line.X1 = woodWidth * (2 * x + 1) / 18;
				line.Y1 = woodHeight * 1 / 20;
				line.X2 = line.X1;
				line.Y2 = woodHeight * 19 / 20;
				lines.Add(line);
			}

			//궁라인

			foreach (var castlePoint in castlePoints)
			{
				Line line = new Line();
				line.X1 = woodWidth * (2 * castlePoint.Item1 + 1) / 18;
				line.Y1 = woodHeight * (2 * castlePoint.Item2 + 1) / 20;
				line.X2 = woodWidth * (2 * castlePoint.Item3 + 1) / 18;
				line.Y2 = woodHeight * (2 * castlePoint.Item4 + 1) / 20;
				lines.Add(line);
			}

			//마커


			foreach (var point in makrPoints)
			{
				double x = woodWidth * (point.Item1 * 2 + 1) / 18;
				double y = woodHeight * (point.Item2 * 2 + 1) / 20;

				Line line1 = new Line();
				line1.X1 = x - markLength;
				line1.Y1 = y - markLength;
				line1.X2 = x + markLength;
				line1.Y2 = y + markLength;
				lines.Add(line1);

				Line line2 = new Line();
				line2.X1 = x - markLength;
				line2.Y1 = y + markLength;
				line2.X2 = x + markLength;
				line2.Y2 = y - markLength;
				lines.Add(line2);
			}

			lines.ForEach(line =>
			{
				line.Stroke = Brushes.Black;
				line.StrokeThickness = 0.5;
				GridLine.Children.Add(line);
			});
		}

		Board board;
		public Board Board
		{
			set
			{
				board = value;

				this.Dispatcher.Invoke(() =>
				{
					for (int y = 0; y < Board.Height; y++)
					{
						for (int x = 0; x < Board.Width; x++)
						{
							var stone = value[y, x];
							units[y, x].SetStone(stone, true);
						}
					}

					TextBlockInfo.Text = "Point : " + board.Point.ToString();

					Mark(board.PrevMove);
				});
			}
		}

		public void Mark(Move move)
		{
			for (int y = 0; y < Board.Height; y++)
			{
				for (int x = 0; x < Board.Width; x++)
				{
					units[y, x].IsMarked = false;
				}
			}

			Pos from = move.From;
			if (!from.IsEmpty)
			{
				units[from.Y, from.X].IsMarked = true;
				Pos to = move.To;
				units[to.Y, to.X].IsMarked = true;
			}
		}
	}
}
