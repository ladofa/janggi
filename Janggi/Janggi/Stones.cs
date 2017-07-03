using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public static class StoneHelper
	{
		/* 0 = empty
		 * 1 = 궁
		 * 2, 3 = 사
		 * 4, 5 = 차
		 * 6, 7 = 마
		 * 8, 9 = 상
		 * 10, 11 = 포
		 * 12, 13, 14, 15, 16 = 졸
		 */
		public enum Stones : uint
		{
			Empty = 0,
			MyJol1 = 0x01,
			MyJol2 = 0x02,
			MyJol3 = 0x04,
			MyJol4 = 0x08,
			MyJol5 = 0x10,
			MySang1 = 0x20,
			MySang2 = 0x40,
			MyMa1 = 0x80,
			MyMa2 = 0x01_00,
			MyPo1 = 0x02_00,
			MyPo2 = 0x04_00,
			MyCha1 = 0x08_00,
			MyCha2 = 0x10_00,
			MySa1 = 0x20_00,
			MySa2 = 0x40_00,
			MyKing = 0x80_00,
			YoJol1 = 0x01_00_00,
			YoJol2 = 0x02_00_00,
			YoJol3 = 0x04_00_00,
			YoJol4 = 0x08_00_00,
			YoJol5 = 0x10_00_00,
			YoSang1 = 0x20_00_00,
			YoSang2 = 0x40_00_00,
			YoMa1 = 0x80_00_00,
			YoMa2 = 0x01_00_00_00,
			YoPo1 = 0x02_00_00_00,
			YoPo2 = 0x04_00_00_00,
			YoCha1 = 0x08_00_00_00,
			YoCha2 = 0x10_00_00_00,
			YoSa1 = 0x20_00_00_00,
			YoSa2 = 0x40_00_00_00,
			YoKing = 0x80_00_00_00,

			Jol = 0x00_1F_00_1F,
			Sang = 0x00_60_00_60,
			Ma = 0x01_80_01_80,
			Po = 0x06_00_06_80,
			Cha = 0x18_00_18_80,
			Sa = 0x60_00_60_80,
			King = 0x80_00_80_80,
			Mine = 0x00_00_FF_Ff,
			Yours = 0xFF_FF_00_00
		}

		public static bool IsEmpty(uint s) => s == (uint)Stones.Empty;
		public static bool IsJol(uint s) => (s & (uint)Stones.Jol) > 0;
		public static bool IsSang(uint s) => (s & (uint)Stones.Sang) > 0;
		public static bool IsMa(uint s) => (s & (uint)Stones.Ma) > 0;
		public static bool IsPo(uint s) => (s & (uint)Stones.Po) > 0;
		public static bool IsCha(uint s) => (s & (uint)Stones.Cha) > 0;
		public static bool IsSa(uint s) => (s & (uint)Stones.Sa) > 0;
		public static bool IsKing(uint s) => (s & (uint)Stones.King) > 0;
		public static bool IsMine(uint s) => (s & (uint)Stones.Mine) > 0;
		public static bool IsYours(uint s) => (s & (uint)Stones.Yours) > 0;

		public static bool IsAllied(uint s1, uint s2) => ((s1 > 0x80_00) ^ (s2 < 0x01_00_00));
		public static uint Opposite(uint s)
		{
			if (s == 0)
			{
				return 0;
			}
			else if (s > 0x80_00)
			{
				return s >> 16;
			}
			else
			{
				return s << 16;
			}
		}
		
	}

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
