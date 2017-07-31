using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Janggi.Ai.Mcts;
using static Janggi.StoneHelper;

namespace Janggi.Ai
{
	class PlacePromCalculator : IPromCalculator
	{
		public double[] Calc(Node node)
		{
			node.GetMoves();
			List<Move> moves = node.moves;
			Board board = node.board;

			foreach (var move in moves)
			{
				uint from = board[move.From];
				int point = GetPoint(from);
			}

			throw new NotImplementedException();
		}
	}
}
