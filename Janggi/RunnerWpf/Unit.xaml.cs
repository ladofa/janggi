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

using static Janggi.StoneHelper;

namespace RunnerWpf
{
	/// <summary>
	/// Interaction logic for Unit.xaml
	/// </summary>
	public partial class Unit : UserControl
	{
		public Unit()
		{
			InitializeComponent();
			SizeChanged += Unit_SizeChanged;
		}

		static List<Tuple<double, double, double, double, double, double>> markerPositions = new List<Tuple<double, double, double, double, double, double>>
		{
			new Tuple<double, double, double, double, double, double>(0, 0, 0.1, 0, 0, 0.1),
			new Tuple<double, double, double, double, double, double>(1, 0, 0.9, 0, 0, 0.1),
			new Tuple<double, double, double, double, double, double>(0, 1, 0.1, 0, 0, 0.9),
			new Tuple<double, double, double, double, double, double>(1, 1, 0.9, 0, 0, 0.9)
		};

		private void Unit_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (IsMarked)
			{
				double width = ActualHeight;
				double x0 = ActualWidth / 2 - width / 2;
				double y0 = 0;

				double[] x = new double[3];
				for (int i = 0; i < 4; i++)
				{
					Polygon poly = GridMarker.Children[i] as Polygon;

					var p = markerPositions[i];

					poly.Points = new PointCollection(new Point[]{
					new Point(p.Item1 * width + x0, p.Item2 * width),
					new Point(p.Item3 * width + x0, p.Item4 * width),
					new Point(p.Item5 * width + x0, p.Item6 * width) });
				}
			}
		}

		static Dictionary<uint, int> imageIndex = new Dictionary<uint, int>
		{
			{(uint)Stones.Empty, 0 },
			{(uint)Stones.MyJol1, 0 },
			{(uint)Stones.MyJol2, 0 },
			{(uint)Stones.MyJol3, 0 },
			{(uint)Stones.MyJol4, 0 },
			{(uint)Stones.MyJol5, 0 },
			{(uint)Stones.MySang1, 0 },
			{(uint)Stones.MySang2, 0 },
			{(uint)Stones.MyMa1, 0 },
			{(uint)Stones.MyMa2, 0 },
			{(uint)Stones.MyPo1, 0 },
			{(uint)Stones.MyPo2, 0 },
			{(uint)Stones.MyCha1, 0 },
			{(uint)Stones.MyCha2, 0 },
			{(uint)Stones.MySa1, 0 },
			{(uint)Stones.MySa2, 0 },
			{(uint)Stones.MyKing, 0 },
			{(uint)Stones.YoJol1, 0 },
			{(uint)Stones.YoJol2, 0 },
			{(uint)Stones.YoJol3, 0 },
			{(uint)Stones.YoJol4, 0 },
			{(uint)Stones.YoJol5, 0 },
			{(uint)Stones.YoSang1, 0 },
			{(uint)Stones.YoSang2, 0 },
			{(uint)Stones.YoMa1, 0 },
			{(uint)Stones.YoMa2, 0 },
			{(uint)Stones.YoPo1, 0 },
			{(uint)Stones.YoPo2, 0 },
			{(uint)Stones.YoCha1, 0 },
			{(uint)Stones.YoCha2, 0 },
			{(uint)Stones.YoSa1, 0 },
			{(uint)Stones.YoSa2, 0 },
			{(uint)Stones.YoKing, 0 },
		};

		public void SetImage(uint stone, bool isMyFirst)
		{
			if (IsEmpty(stone))
			{
			}
			else if (IsCha(stone))
			{

			}
		}

		public bool IsMarked
		{
			set
			{
				GridMarker.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}

			get
			{
				return GridMarker.Visibility == Visibility.Visible;
			}
		}

		public Brush MarkerBrush
		{
			set
			{
				foreach (Polygon polygon in GridMarker.Children)
				{
					polygon.Fill = value;
					//커밋용 대충
				}
			}

			get
			{
				return (GridMarker.Children[0] as Polygon).Fill;
			}
		}
	}
}
