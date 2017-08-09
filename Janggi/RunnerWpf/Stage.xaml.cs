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

namespace RunnerWpf
{
	/// <summary>
	/// Interaction logic for Stage.xaml
	/// </summary>
	public partial class Stage : UserControl
	{
		public Stage()
		{
			InitializeComponent();
			SizeChanged += Stage_SizeChanged;
		}

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
			List<Tuple<double, double, double, double>> castlePoints = new List<Tuple<double, double, double, double>>
			{
				new Tuple<double, double, double, double>(3, 0, 5, 2),
				new Tuple<double, double, double, double>(3, 2, 5, 0),
				new Tuple<double, double, double, double>(3, 7, 5, 9),
				new Tuple<double, double, double, double>(3, 9, 5, 7),
			};

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
			List<Tuple<double, double>> makrPoints = new List<Tuple<double, double>>
			{
				new Tuple<double, double>(0, 3), new Tuple<double, double>(0, 6),
				new Tuple<double, double>(2, 3), new Tuple<double, double>(2, 6),
				new Tuple<double, double>(4, 3), new Tuple<double, double>(4, 6),
				new Tuple<double, double>(6, 3), new Tuple<double, double>(6, 6),
				new Tuple<double, double>(8, 3), new Tuple<double, double>(8, 6),
				new Tuple<double, double>(1, 2), new Tuple<double, double>(1, 7),
				new Tuple<double, double>(7, 2), new Tuple<double, double>(7, 7),
			};

			foreach (var point in makrPoints)
			{
				double x = woodWidth * (point.Item1 * 2 + 1) / 18;
				double y = woodHeight * (point.Item2 *2 + 1) / 20;

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
	}
}
