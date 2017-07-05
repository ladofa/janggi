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
		public List<double> Calc(Node node)
		{
			List<Move> moves = node.GetMoves();
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
