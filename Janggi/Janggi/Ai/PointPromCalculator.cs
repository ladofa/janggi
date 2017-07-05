using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Ai.Mcts;
using static Janggi.StoneHelper;

namespace Janggi.Ai
{
	class PointPromCalculator : IPromCalculator
	{
		public List<double> Calc(Node node)
		{
			var moves = node.GetMoves();
			Board board = node.board;
			List<double> proms = new List<double>(moves.Count);

			Func<uint, uint, uint, int> Judge;
			if (node.board.IsMyTurn)
			{
				Judge = (stoneFrom, stoneTo, target) =>
				{
					//잃으면 점수 마이너스..인데 그래도 뭘 해봤으니 +10.
					return GetPoint(stoneTo) + (IsYours(target) ? GetPoint(stoneFrom) + 10 : 0);
				};
			}
			else
			{
				Judge = (stoneFrom, stoneTo, target) =>
				{
					//잃으면 점수 마이너스..인데 그래도 뭘 해봤으니 +10.
					return -GetPoint(stoneTo) + (IsYours(target) ? -GetPoint(stoneFrom) + 10 : 0);
				};
			}

			//최소 점수
			int max = int.MinValue;
			int min = int.MaxValue;
			
			foreach (Move move in moves)
			{
				if (move.Equals(Move.Rest))
				{
					proms.Add(0);
					continue;
				}

				uint stoneFrom = board[move.From];
				uint stoneTo = board[move.To];
				uint target = board.targets[move.To.Y, move.To.X];

				int judge = Judge(stoneFrom, stoneTo, target);
				proms.Add((double)judge);

				if (judge > max)
				{
					max = judge;
				}

				if (judge < min)
				{
					min = judge;
				}
			}

			double sum = 0;
			int diff = Math.Min(max - min, 100);
			int diff0 = diff / 5;
			for (int i = 0; i < proms.Count; i++)
			{
				proms[i] = proms[i] - max + diff;
				if (proms[i] < diff0) proms[i] = diff0;
				sum += proms[i];
			}

			for (int i = 0; i < proms.Count; i++)
			{
				proms[i] = proms[i] / sum;
			}

			return proms;
			//throw new NotImplementedException();
		}
	}
}
