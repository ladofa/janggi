using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public struct Pos
	{
		public int X;
		public int Y;

		public Pos(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	public struct Move
	{
		public Pos From;
		public Pos To;

		public Move(int fromX, int fromY, int toX, int toY)
		{
			From = new Pos(fromX, fromY);
			To = new Pos(toX, toY);
		}

		public Move(Pos from, Pos to)
		{
			From = from;
			To = to;
		}
	}
}
