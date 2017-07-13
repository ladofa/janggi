using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public struct Pos
	{
		public sbyte X;
		public sbyte Y;

		public Pos(sbyte x, sbyte y)
		{
			X = x;
			Y = y;
		}

		public Pos(int x, int y)
		{
			X = (sbyte)x;
			Y = (sbyte)y;
		}


		public Pos(System.Collections.IEnumerable arr)
		{
			var e = arr.GetEnumerator();
			X = (sbyte)e.Current;
			e.MoveNext();
			Y = (sbyte)e.Current;
		}

		static public Pos operator +(Pos p1, Pos p2)
		{
			return new Pos((sbyte)(p1.X + p2.X), (sbyte)(p1.Y + p2.Y));
		}

		static public Pos operator -(Pos p1, Pos p2)
		{
			return new Pos((sbyte)(p1.X - p2.X), (sbyte)(p1.Y - p2.Y));
		}

		public bool Equals(sbyte x, sbyte y)
		{
			return X == x && Y == y;
		}

		public bool Equals(int x, int y)
		{
			return X == (sbyte)x && Y == (sbyte)y;
		}
	}

	public struct Move
	{
		public Pos From;
		public Pos To;

		public Move(sbyte fromX, sbyte fromY, sbyte toX, sbyte toY)
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
