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
			int sum = 0;
			foreach (Node e in children)
			{
				//내 점술르 더한다.
				int p = Filter(e.board.Point);
				//상대 움직임
				List<Move> yoMoves = e.GetMoves();
				//상대가 잡을 수 있는 기물수를 뺀다.
				p -= e.board.CountTarget(yoMoves);

				proms.Add((double)p);
				if (min > p)
				{
					min = p;
				}
				sum += p;
			}


			double normFactor = (sum) / proms.Count - min;


			throw new NotImplementedException();
		}
	}
}
