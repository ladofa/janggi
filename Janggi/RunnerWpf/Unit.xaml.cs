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

		public uint Stone
		{
			set
			{
				//이미지 변경
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
				}
			}

			get
			{
				return (GridMarker.Children[0] as Polygon).Fill;
			}
		}
	}
}
