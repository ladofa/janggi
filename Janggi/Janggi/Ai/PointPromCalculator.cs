using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Ai.Mcts;

namespace Janggi.Ai
{
	class PointPromCalculator : IPromCalculator
	{
		public List<double> Calc(Node node)
		{
			var children = node.GetChildren();
			List<double> proms = new List<double>(children.Count);

			Func<int, int> Filter;
			if (node.board.IsMyTurn)
			{
				Filter = (x) => x;
			}
			else
			{
				//내 턴이 아니라면 점수를 반대로.
				Filter = (x) => -x;
			}

			//최소 점수
			int min = int.MaxValue;
			
			foreach (Node e in children)
			{
				int p = Filter(e.board.Judge());

				if (min > p)
				{
					min = p;
				}

				proms.Add((double)p);
			}

			double sum = 0;
			for (int i = 0; i < proms.Count; i++)
			{
				proms[i] = proms[i] - min + 10;
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
