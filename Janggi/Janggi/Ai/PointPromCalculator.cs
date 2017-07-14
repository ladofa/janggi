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
			if (board.IsMyTurn)
			{
				Judge = (stoneFrom, stoneTo, target) =>
				{
					//일단 상대를 따먹으면 10점
					int takingPoint = GetPoint(stoneTo);
					return takingPoint + ((IsYours(target) ? GetPoint(stoneFrom) : 0) + (takingPoint != 0 ? 10 : 0));
				};
			}
			else
			{
				Judge = (stoneFrom, stoneTo, target) =>
				{
					int takingPoint = -GetPoint(stoneTo);
					return takingPoint + ((IsMine(target) ? -GetPoint(stoneFrom) : 0) + (takingPoint != 0 ? 10 : 0));
				};
			}

			//최소 점수
			int max = int.MinValue;
			int min = int.MaxValue;
			
			//마지막 rest빼고.
			for (int i = 0; i < moves.Count - 1; i++)
			{
				Move move = moves[i];
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
			int diff = Math.Min(max - min, 130);
			int diff0 = diff / 10;
			for (int i = 0; i < proms.Count; i++)
			{
				proms[i] = proms[i] - max + diff;
				if (proms[i] < diff0) proms[i] = diff0;
				proms[i] += 2;
				sum += proms[i];
			}

			//rest에 대한 추가
			proms.Add(diff0);
			sum += diff0;


			for (int i = 0; i < proms.Count; i++)
			{
				proms[i] = proms[i] / sum;
			}

			return proms;
			//throw new NotImplementedException();
		}
	}
}
