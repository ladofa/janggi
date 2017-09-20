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
using Janggi.Ai;

namespace RunnerWpf
{
	/// <summary>
	/// Interaction logic for TreeViewer.xaml
	/// </summary>
	public partial class TreeViewer : UserControl
	{
		public TreeViewer()
		{
			InitializeComponent();
			KeyDown += TreeViewer_KeyDown;
		}

		private void TreeViewer_KeyDown(object sender, KeyEventArgs e)
		{
			if (mcts == null) return;

			mcts.PauseSearching();
			mcts.WaitCycle();
			if (e.Key == Key.B)
			{
				if (currentNode != null)
				{
					if (currentNode.parent != null && mcts.root != currentNode)
					{
						show(currentNode.parent);
					}
				}
			}
			mcts.ResumeSearching();
		}

		Mcts mcts;

		public void Load(Mcts mcts)
		{
			this.mcts = mcts;
			mcts.PauseSearching();
			mcts.WaitCycle();
			if (mcts.root.parent != null)
			{
				listUpTree(mcts.root.parent);
				show(mcts.root.parent);
			}
			else
			{
				listUpTree(mcts.root);
				show(mcts.root);
			}
			mcts.ResumeSearching();

			
		}

		void listUpTree(Node node)
		{
			if (node == null)
			{
				return;
			}

			Dispatcher.Invoke(() =>
			{
				StackPanelTree.Children.Clear();

				while (true)
				{
					Button button = makeNodeButton(node);
					StackPanelTree.Children.Insert(0, button);

					if (node == mcts.root.parent)
					{
						break;
					}

					if (node.parent == null)
					{
						Console.WriteLine("asdfasdf");
						break;
					}

					node = node.parent;
				}
			});
		}

		Button makeNodeButton(Node node)
		{
			Button button = new Button();
			
			button.Tag = node;
			button.Click += Button_Click_Tree;

			if (node != null)
			{

				string letter = "";
				if (!node.prevMove.Equals(Move.Empty))
				{
					uint stone = node.board[node.prevMove.To];
					letter = StoneHelper.GetLetter(stone, node.board.IsMyFirst);
				}
				else
				{
					letter = "不";
				}

				if (node.board.IsMyTurn)
				{
					letter = "My" + letter;
				}
				else
				{
					letter = "Yo" + letter;
				}

				button.Content = letter;
			}
			else
			{
				button.Content = "N";
			}

			return button;
		}

		private void Button_Click_Tree(object sender, RoutedEventArgs e)
		{
			mcts.PauseSearching();
			mcts.WaitCycle();
			Button button = sender as Button;
			if (button.Tag == null) return;
			Node node = button.Tag as Node;
			show(node);
			mcts.ResumeSearching();
		}

		FrameworkElement MakeNodeButtonAndState(Node node, double maxVisited)
		{
			Button button = new Button();

			button.Tag = node;
			button.Click += Button_Click_Moves;

			if (node != null)
			{
				string letter = "";
				if (!node.prevMove.Equals(Move.Empty))
				{
					uint stone = node.board[node.prevMove.To];
					letter = StoneHelper.GetLetter(stone, node.board.IsMyFirst);
				}
				else
				{
					letter = "不";
				}

				button.Content = letter;
			}
			else
			{
				button.Content = "N";
			}

			button.Width = 23;
			button.Height = 23;

			Grid grid = new Grid();

			ProgressBar progress = new ProgressBar();
			TextBlock textBlock = new TextBlock();
			progress.Width = 104;
			progress.Height = 23;

			textBlock.HorizontalAlignment = HorizontalAlignment.Center;
			textBlock.VerticalAlignment = VerticalAlignment.Center;

			if (node != null)
			{
				progress.Value = node.win / maxVisited * 100;
				progress.Tag = node.prevMove;
				progress.MouseEnter += Progress_MouseEnter;

				textBlock.Text = node.win.ToString("f1") + "/" + node.visited.ToString();
			}

			grid.Children.Add(progress);
			grid.Children.Add(textBlock);

			StackPanel panel = new StackPanel();
			panel.Children.Add(button);
			panel.Children.Add(grid);
			panel.Orientation = Orientation.Horizontal;
			panel.Margin = new Thickness(1);

			return panel;
		}

		private void Progress_MouseEnter(object sender, MouseEventArgs e)
		{
			ProgressBar progress = sender as ProgressBar;
			Move move = (Move)progress.Tag;
			StageCurrent.Mark(move);
		}

		private void Button_Click_Moves(object sender, RoutedEventArgs e)
		{
			mcts.PauseSearching();
			mcts.WaitCycle();
			Button button = sender as Button;
			Node node = button.Tag as Node;

			listUpTree(node);
			show(node);

			mcts.ResumeSearching();
		}

		Node currentNode;
		void show(Node node)
		{
			if (node == null) return;

			currentNode = node;

			Dispatcher.Invoke(() =>
			{
				StackPanelMoves.Children.Clear();
				if (node.children != null)
				{
					Node[] children = node.children;

					float maxVisited = 1;
					for (int i = 0; i < children.Length; i++)
					{
						if (children[i] != null)
						{
							float visited = children[i].win;
							if (visited > maxVisited)
							{
								maxVisited = visited;
							}
						}
					}

					for (int i = 0; i < children.Length; i++)
					{
						var move = MakeNodeButtonAndState(children[i], maxVisited);
						StackPanelMoves.Children.Add(move);
					}
					
				}
				else
				{
					
				}

				StageCurrent.Board = node.board;
			});

			
		}

		private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			//StageCurrent.Width = ActualWidth;
			//StageCurrent.Height = ActualHeight;
		}
	}
}
