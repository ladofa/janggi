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
		}

		Board board;

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

			board = new Board(myTable, yoTable, isMyFirst);

			StageMain.Board = board;
		}

		private void StageMain_UnitMoved(Move move)
		{
			board.MoveNext(move);
			StageMain.Board = board;
		}
	}
}
