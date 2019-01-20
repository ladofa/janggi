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

		public static Pos Empty = new Pos(12, 27); //255

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

		public Pos(byte b)
		{
			X = (sbyte)(b % Board.Width);
			Y = (sbyte)(b / Board.Width);
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

		public bool IsEmpty
		{
			get => X == Pos.Empty.X && Y == Pos.Empty.Y;
		}

		public byte Byte
		{
			set
			{
				X = (sbyte)(value % Board.Width);
				Y = (sbyte)(value / Board.Width);
			}

			get
			{
				return (byte)(X + Y * Board.Width);
			}
		}

		public Pos GetOpposite()
		{
			if (IsEmpty)
			{
				return Empty;
			}
			else
			{
				return new Pos(Board.Width - X - 1, Board.Height - Y - 1);
			}
		}

		internal Pos GetFlip()
		{
			if (IsEmpty)
			{
				return Empty;
			}
			else
			{
				return new Pos(Board.Width - X - 1, Y);
			}
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

		public static Move Empty = new Move(Pos.Empty, Pos.Empty);

		public bool IsEmpty
		{
			get => From.IsEmpty && To.IsEmpty;
		}

		public override string ToString()
		{
			return $"({From.X},  {From.Y}) -> ({To.X}, {To.Y})";
		}


		public Move GetOpposite()
		{
			Move op = new Move
			{
				From = From.GetOpposite(),
				To = To.GetOpposite()
			};
			return op;
		}

		public Move GetFlip()
		{
			Move flip = new Move
			{
				From = From.GetFlip(),
				To = To.GetFlip()
			};
			return flip;
		}

		public static List<Move> GetOpposite(List<Move> moves)
		{
			List<Move> ops = new List<Move>();

			foreach (var move in moves)
			{
				ops.Add(move.GetOpposite());
			}

			return ops;
		}
	}
}
