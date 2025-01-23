using Janggi.Game;
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

using static Janggi.Game.Stones;

namespace RunnerWpf8
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

        static Dictionary<byte, int> sizeIndex = new Dictionary<byte, int>
        {
            {EMPTY, 4 },
            {CHO_JOL, 0 },
            {CHO_SANG,1},
            {CHO_MA,        1},
            {CHO_PO,        1},
            {CHO_CHA,        1},
            {CHO_SA,       0},
            {CHO_KING,        2},
            {HAN_JOL, 0},
            {HAN_SANG, 1},
            {HAN_MA, 1},
            {HAN_PO, 1},
            {HAN_CHA, 1},
            {HAN_SA, 1},
            {HAN_KING,  2},
        };

        List<Tuple<double, double, double, double, double, double>> lastMarkerPositions;

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
                if (lastMarkerPositions == null)
                {
                    markerPositions = markerPositionsSmall;
                }
                else
                {
                    markerPositions = lastMarkerPositions;
                }
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

            lastMarkerPositions = markerPositions;

            double x0 = ActualWidth / 2 - width / 2;
            double y0 = 0;

            double[] x = new double[3];
            for (int i = 0; i < 4; i++)
            {
                Polygon poly = (Polygon)GridMarker.Children[i];

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

        static Dictionary<byte, int> imageIndex = new Dictionary<byte, int>
         {
            {EMPTY, 0 },
            {CHO_JOL, 1 },
            {CHO_SANG,2},
            {CHO_MA,  3},
            {CHO_PO,  4},
            {CHO_CHA, 5},
            {CHO_SA,  6},
            {CHO_KING, 7},
            {HAN_JOL, 8},
            {HAN_SANG, 9},
            {HAN_MA, 10},
            {HAN_PO, 11},
            {HAN_CHA, 12},
            {HAN_SA, 13},
            {HAN_KING,  14},
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

        byte stone;
        public byte Stone
        {
            get
            {
                return stone;
            }
        }

        public void SetStone(byte stone)
        {
            this.stone = stone;

            int index = imageIndex[stone];
            string imageName = imgaeNames[index];
            string path = "Images/" + imageName + ".png";
            ImageUnit.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));

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
                return ((Polygon)GridMarker.Children[0]).Fill;
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

        public (int y, int x) Pos
        {
            get;
            set;
        }
    }
}
