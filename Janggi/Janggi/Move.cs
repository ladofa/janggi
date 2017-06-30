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

		public Pos(System.Collections.IEnumerable arr)
		{
			var e = arr.GetEnumerator();
			X = (int)e.Current;
			e.MoveNext();
			Y = (int)e.Current;
		}

		static public Pos operator +(Pos p1, Pos p2)
		{
			return new Pos(p1.X + p2.X, p1.Y + p2.Y);
		}

		static public Pos operator -(Pos p1, Pos p2)
		{
			return new Pos(p1.X - p2.X, p1.Y - p2.Y);
		}

		public bool Equals(int x, int y)
		{
			return X == x && Y == y;
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

		public static Move Rest = new Move(-1, -1, -1, -1);

		public bool IsRest
		{
			get => From.X == -1;
		}

		public override string ToString()
		{
			return $"({From.X},  {From.Y}) -> ({To.X}, {To.Y})";
		}
	}
}
