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

		static List<Tuple<double, double, double, double, double, double>> markerPositionsBig = new List<Tuple<double, double, double, double, double, double>>
		{
			new Tuple<double, double, double, double, double, double>(0, 0, 0.1, 0, 0, 0.1),
			new Tuple<double, double, double, double, double, double>(1, 0, 0.9, 0, 1, 0.1),
			new Tuple<double, double, double, double, double, double>(0, 1, 0.1, 1, 0, 0.9),
			new Tuple<double, double, double, double, double, double>(1, 1, 0.9, 1, 1, 0.9)
		};

		static List<Tuple<double, double, double, double, double, double>> markerPositionsMid = new List<Tuple<double, double, double, double, double, double>>
		{
			new Tuple<double, double, double, double, double, double>(0.12, 0.12, 0.22, 0.12, 0.12, 0.23),
			new Tuple<double, double, double, double, double, double>(0.88, 0.12, 0.78, 0.12, 0.88, 0.23),
			new Tuple<double, double, double, double, double, double>(0.12, 0.88, 0.22, 0.88, 0.12, 0.77),
			new Tuple<double, double, double, double, double, double>(0.88, 0.88, 0.78, 0.88, 0.88, 0.77)
		};

		static List<Tuple<double, double, double, double, double, double>> markerPositionsSmall = new List<Tuple<double, double, double, double, double, double>>
		{
			new Tuple<double, double, double, double, double, double>(0.21, 0.21, 0.31, 0.21, 0.21, 0.31),
			new Tuple<double, double, double, double, double, double>(0.79, 0.21, 0.69, 0.21, 0.79, 0.31),
			new Tuple<double, double, double, double, double, double>(0.21, 0.79, 0.31, 0.79, 0.21, 0.69),
			new Tuple<double, double, double, double, double, double>(0.79, 0.79, 0.69, 0.79, 0.79, 0.69)
		};

		static Dictionary<uint, int> sizeIndex = new Dictionary<uint, int>
		{
			{(uint)Stones.Empty, 4 },
			{(uint)Stones.MyJol1, 0 },
			{(uint)Stones.MyJol2, 0 },
			{(uint)Stones.MyJol3, 0 },
			{(uint)Stones.MyJol4, 0 },
			{(uint)Stones.MyJol5, 0 },
			{(uint)Stones.MySang1, 1 },
			{(uint)Stones.MySang2, 1 },
			{(uint)Stones.MyMa1, 1 },
			{(uint)Stones.MyMa2, 1 },
			{(uint)Stones.MyPo1, 1 },
			{(uint)Stones.MyPo2, 1 },
			{(uint)Stones.MyCha1, 1 },
			{(uint)Stones.MyCha2, 1 },
			{(uint)Stones.MySa1, 0 },
			{(uint)Stones.MySa2, 0 },
			{(uint)Stones.MyKing, 2 },
			{(uint)Stones.YoJol1, 0 },
			{(uint)Stones.YoJol2, 0 },
			{(uint)Stones.YoJol3, 0 },
			{(uint)Stones.YoJol4, 0 },
			{(uint)Stones.YoJol5, 0 },
			{(uint)Stones.YoSang1, 1 },
			{(uint)Stones.YoSang2, 1 },
			{(uint)Stones.YoMa1, 1 },
			{(uint)Stones.YoMa2, 1 },
			{(uint)Stones.YoPo1, 1 },
			{(uint)Stones.YoPo2, 1 },
			{(uint)Stones.YoCha1, 1 },
			{(uint)Stones.YoCha2, 1 },
			{(uint)Stones.YoSa1, 0 },
			{(uint)Stones.YoSa2, 0 },
			{(uint)Stones.YoKing, 2 },
		};

		//유닛 크기가 변할때마다 마커 크기 변경
		void changeMarkerSize()
		{
			double width = ActualHeight;

			//--써클 사이즈
			EllipseMarker.Width = width * 0.7;
			EllipseMarker.Height = width * 0.7;
			EllipseMarkerTarget.Width = width * 0.6;
			EllipseMarkerTarget.Height = width * 0.6;
			EllipseMarkerBlock.Width = width * 0.5;
			EllipseMarkerBlock.Height = width * 0.5;

			//--마커 사이즈
			List<Tuple<double, double, double, double, double, double>> markerPositions;
			int index = sizeIndex[Stone];
			if (index == 4)
			{
				//빈공간은 몰라~
				return;
			}
			else if (index == 0)
			{
				markerPositions = markerPositionsSmall;
			}
			else if (index == 1)
			{
				markerPositions = markerPositionsMid;
			}
			else
			{
				markerPositions = markerPositionsBig;
			}

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

		private void Unit_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			changeMarkerSize();
		}

		static Dictionary<uint, int> imageIndex = new Dictionary<uint, int>
		{
			{(uint)Stones.Empty, 0 },
			{(uint)Stones.MyJol1, 1 },
			{(uint)Stones.MyJol2, 1 },
			{(uint)Stones.MyJol3, 1 },
			{(uint)Stones.MyJol4, 1 },
			{(uint)Stones.MyJol5, 1 },
			{(uint)Stones.MySang1, 2 },
			{(uint)Stones.MySang2, 2 },
			{(uint)Stones.MyMa1, 3 },
			{(uint)Stones.MyMa2, 3 },
			{(uint)Stones.MyPo1, 4 },
			{(uint)Stones.MyPo2, 4 },
			{(uint)Stones.MyCha1, 5 },
			{(uint)Stones.MyCha2, 5 },
			{(uint)Stones.MySa1, 6 },
			{(uint)Stones.MySa2, 6 },
			{(uint)Stones.MyKing, 7 },
			{(uint)Stones.YoJol1, 8 },
			{(uint)Stones.YoJol2, 8 },
			{(uint)Stones.YoJol3, 8 },
			{(uint)Stones.YoJol4, 8 },
			{(uint)Stones.YoJol5, 8 },
			{(uint)Stones.YoSang1, 9 },
			{(uint)Stones.YoSang2, 9 },
			{(uint)Stones.YoMa1, 10 },
			{(uint)Stones.YoMa2, 10 },
			{(uint)Stones.YoPo1, 11 },
			{(uint)Stones.YoPo2, 11 },
			{(uint)Stones.YoCha1, 12 },
			{(uint)Stones.YoCha2, 12 },
			{(uint)Stones.YoSa1, 13 },
			{(uint)Stones.YoSa2, 13 },
			{(uint)Stones.YoKing, 14 },
		};

		string[] imgaeNames = new string[]
		{
			"Empty",
			"ChoJol",
			"ChoSang",
			"ChoMa",
			"ChoPo",
			"ChoCha",
			"ChoSa",
			"ChoKing",
			"HanJol",
			"HanSang",
			"HanMa",
			"HanPo",
			"HanCha",
			"HanSa",
			"HanKing",
		};

		static bool isFlip;
		public static bool IsFlip
		{
			set
			{
				isFlip = value;
			}
			get
			{
				return isFlip;
			}
		}

		uint stone;
		public uint Stone
		{
			get
			{
				return stone;
			}
		}

		public void SetStone(uint stone, bool isMyFirst)
		{
			this.stone = stone;

			bool flip = IsYours(stone) && IsFlip;
			if (!isMyFirst)
			{
				stone = Opposite(stone);
			}
			int index = imageIndex[stone];
			string imageName = imgaeNames[index];
			if (flip)
			{
				imageName += " - Copy";
			}
			string path = "Images/" + imageName + ".png";
			ImageUnit.Source = new BitmapImage(new Uri(path, UriKind.Relative));

			changeMarkerSize();
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


		public bool IsCircled
		{
			set
			{
				EllipseMarker.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}

			get
			{
				return EllipseMarker.Visibility == Visibility.Visible;
			}
		}

		public bool IsMarkedTarget
		{
			set
			{
				EllipseMarkerTarget.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}

			get
			{
				return EllipseMarkerTarget.Visibility == Visibility.Visible;
			}
		}

		public bool IsMarkedBlock
		{
			set
			{
				EllipseMarkerBlock.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
			}

			get
			{
				return EllipseMarkerBlock.Visibility == Visibility.Visible;
			}
		}

		public Brush CircleBrush
		{
			set
			{
				EllipseMarker.Stroke = value;
			}

			get
			{
				return EllipseMarker.Stroke;
			}
		}

		public Janggi.Pos Pos
		{
			get;
			set;
		}
	}
}
