using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public struct Stone
	{
		//기물을 잃었을 때의 점수
		static int[] points = { 0, -20, -30, -50, -70, -130, -30, -10000, 20, 30, 50, 70, 130, 30, 10000 };
		public int Point => points[(byte)val];

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

		static public explicit operator int(Stone stone)
		{
			return (int)stone.val;
		}

		public bool IsMy => val != 0 && (byte)val <= 7;

		public bool IsYo => (byte)val > 7;

		public bool IsAlliesWith(Stone stone)
		{
			if (val == 0 || stone.val == 0)
			{
				return false;
			}

			return !((byte)val > 7 ^ (byte)stone.val > 7);
		}

		public bool IsCha => val == Val.MyCha || val == Val.YoCha;

		public bool IsPo => val == Val.MyPo || val == Val.YoPo;

		public bool IsMa => val == Val.MyMa || val == Val.YoMa;

		public bool IsSang => val == Val.MySang || val == Val.YoSang;

		public bool IsSa => val == Val.MySa || val == Val.YoSa;

		public bool IsJolt => val == Val.MyJol || val == Val.YoJol;

		public bool IsGoong => val == Val.MyGoong || val == Val.YoGoong;

		public bool IsEmpty => val == Val.Empty;

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
	}
}
