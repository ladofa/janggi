using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Janggi;
using Janggi.Ai;
using static Janggi.StoneHelper;

namespace Runner.Process
{
	public class DevelopmentTest
	{
		public DevelopmentTest()
		{
			for (int i = 0; i < 32; i++)
			{
				uint stone = (uint)1 << i;
				Console.WriteLine(((Stones)stone).ToString() + ", " + GetPoint(stone));
			}

			Board board = new Board(Board.Tables.Outer, Board.Tables.Left, true);

			PrimaryUcb primaryUcb = new PrimaryUcb();
			Mcts mcts = new Mcts(primaryUcb);


			mcts.Init(board);

			while (!board.IsMyWin && !board.IsYoWin)
			{
				var t = mcts.SearchNextAsync();
				Console.WriteLine("Thinking ... ");
				t.Wait();
				Node node = t.Result;
				board.MoveNext(node.prevMove);
				mcts.SetMove(node.prevMove);
				board.PrintStones();


				Console.WriteLine("\n");
			}
		}
	}
}
