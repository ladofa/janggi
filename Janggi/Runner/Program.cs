using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Janggi;
using Janggi.Ai;

namespace Runner
{
	class Program
	{
		static void Main(string[] args)
		{
			Board board = new Board(Board.Tables.Outer, Board.Tables.Left, true);

			Mcts mcts = new Mcts();
			mcts.Init(board);

			while (!board.IsMyWin && !board.IsYoWin)
			{
				Mcts.Node node = mcts.SearchNext();
				board.MoveNext(node.prevMove);
				mcts.SetMove(node.prevMove);
				board.PrintStones();


				Console.WriteLine("\n");
			}




		}
	}
}
