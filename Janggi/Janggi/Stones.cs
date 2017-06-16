using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public struct Stone
	{
		static int[] points = { 0, -20, -30, -50, -70, -130, -30, -10000, 20, 30, 50, 70, 130, 30, 10000 };

		public enum Val : byte
		{
			Empty = 0,
			MyJol = 1,
			MySang = 2,
			MyMa = 3,
			MyPo = 4,
			MyCha = 5,
			MySa = 6,
			MyGoong = 7,
			YoJol = 1 + 7,
			YoSang = 2 + 7,
			YoMa = 3 + 7,
			YoPo = 4 + 7,
			YoCha = 5 + 7,
			YoSa = 6 + 7,
			YoGoong = 7 + 7
		}

		Val val;

		public Val Value
		{
			get => val;
		}

		public Stone(Val value)
		{
			val = value;
		}

		public bool IsMy
		{
			get => val != 0 && (byte)val <= 7;
		}

		public bool IsYo
		{
			get => (byte)val > 7;
		}

		public bool IsAlliesWith(Stone stone)
		{
			if (val == 0 || stone.val == 0)
			{
				return false;
			}

			return !((byte)val > 7 ^ (byte)stone.val > 7);
		}

		public bool IsCha
		{
			get => val == Val.MyCha || val == Val.YoCha;
		}

		public bool IsPo
		{
			get => val == Val.MyPo || val == Val.YoPo;
		}

		public bool IsMa
		{
			get => val == Val.MyMa || val == Val.YoMa;
		}

		public bool IsSang
		{
			get => val == Val.MySang || val == Val.YoSang;
		}

		public bool IsSa
		{
			get => val == Val.MySa || val == Val.YoSa;
		}

		public bool IsJol
		{
			get => val == Val.MyJol || val == Val.YoJol;
		}

		public bool IsGoong
		{
			get => val == Val.MyGoong || val == Val.YoGoong;
		}

		public bool IsEmpty
		{
			get => val == Val.Empty;
		}

		public Stone Opposite
		{
			get
			{
				if (val == 0)
				{
					return new Stone();
				}
				else if ((byte)val > 7)
				{
					return new Stone(val - 7);
				}
				else
				{
					return new Stone(val + 7);
				}
			}
		}

		public int Point
		{
			get
			{
				return points[(byte)val];
			}
		}
	}
}
