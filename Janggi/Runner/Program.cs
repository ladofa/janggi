using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Janggi;
using Janggi.Ai;

using static Janggi.StoneHelper;

namespace Runner
{
	class Program
	{
		static void Main(string[] args)
		{
			for (int i = 0; i < 32; i++)
			{
				uint stone = (uint)1 << i;
				Console.WriteLine(((Stones)stone).ToString() + ", " + GetPoint(stone));
			}

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
