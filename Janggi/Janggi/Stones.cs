using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public static class StoneHelper
	{
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

			Jol =  0x00_1F_00_1F,
			MyJol = 0x00_00_00_1F,
			YoJol = 0x00_1F_00_00,
			Sang = 0x00_60_00_60,
			Ma =   0x01_80_01_80,
			Po =   0x06_00_06_00,
			MyPo = 0x00_00_06_00,
			YoPo = 0x06_00_00_00,
			Cha =  0x18_00_18_00,
			MyCha = 0x00_00_18_00,
			YoCha = 0x18_00_00_00,
			Sa =   0x60_00_60_00,
			King = 0x80_00_80_00,
			Mine = 0x00_00_FF_Ff,
			Yours = 0xFF_FF_00_00
		}

		public static bool IsEmpty(uint s) => s == (uint)Stones.Empty;
		public static bool IsJol(uint s) => (s & (uint)Stones.Jol) > 0;
		public static bool IsMyJol(uint s) => (s & (uint)Stones.MyJol) > 0;
		public static bool IsYoJol(uint s) => (s & (uint)Stones.YoJol) > 0;
		public static bool IsSang(uint s) => (s & (uint)Stones.Sang) > 0;
		public static bool IsMa(uint s) => (s & (uint)Stones.Ma) > 0;
		public static bool IsPo(uint s) => (s & (uint)Stones.Po) > 0;
		public static bool IsMyPo(uint s) => (s & (uint)Stones.MyPo) > 0;
		public static bool IsYoPo(uint s) => (s & (uint)Stones.YoPo) > 0;
		public static bool IsCha(uint s) => (s & (uint)Stones.Cha) > 0;
		public static bool IsMyCha(uint s) => (s & (uint)Stones.MyCha) > 0;
		public static bool IsYoCha(uint s) => (s & (uint)Stones.YoCha) > 0;
		public static bool IsSa(uint s) => (s & (uint)Stones.Sa) > 0;
		public static bool IsKing(uint s) => (s & (uint)Stones.King) > 0;
		public static bool IsMine(uint s) => (s & (uint)Stones.Mine) > 0;
		public static bool IsYours(uint s) => (s & (uint)Stones.Yours) > 0;

		public static bool IsAllied(uint s1, uint s2) => (s1 != 0 && s2 != 0 && (s1 > 0x80_00) ^ (s2 < 0x01_00_00));
		public static bool IsEnemy(uint s1, uint s2) => (s1 != 0 && s2 != 0 && (s1 > 0x80_00) ^ (s2 > 0x80_00));
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

		public static uint Index2Stone(int index)
		{
			return (uint)1 << index;
		}

		private static int[] lookupStone2Index = new int[0x8001];

		public static int Stone2Index(uint stone)
		{
			if (stone > 0x8000)
			{
				return lookupStone2Index[stone >> 16] + 16;
			}
			else
			{
				return lookupStone2Index[stone];
			}
		}


		private static int[] lookupPoint = new int[0x8001];

		public static int GetPoint(uint stone)
		{
			if (stone == 0)
			{
				return 0;
			}
			else if (stone > 0x8000)
			{
				return lookupPoint[stone >> 16];
			}
			else
			{
				return -lookupPoint[stone];
			}
		}

		public static string GetLetter(uint stone, bool myFirst)
		{
			return GetLetter(Stone2Index(stone), myFirst);
		}

		private static string[] lookupLetter = {
			"┼",
			"卒","卒","卒","卒","卒",
			"象","象","馬", "馬","包","包",  "車", "車","士","士", "楚",
			"兵","兵","兵","兵","兵",
			"象","象","馬", "馬","包","包",  "車", "車","士","士", "漢",
		};
	

		public static string GetLetter(int index, bool myFirst)
		{
			if (myFirst)
			{
				return lookupLetter[index];
			}
			else
			{
				if (index > 16)
				{
					return lookupLetter[index - 16];
				}
				else
				{
					return lookupLetter[index + 16];
				}
			}
		}


		static StoneHelper()
		{
			lookupStone2Index[0] = 0;
			lookupStone2Index[0x01] = 1;
			lookupStone2Index[0x02] = 2;
			lookupStone2Index[0x04] = 3;
			lookupStone2Index[0x08] = 4;
			lookupStone2Index[0x10] = 5;
			lookupStone2Index[0x20] = 6;
			lookupStone2Index[0x40] = 7;
			lookupStone2Index[0x80] = 8;
			lookupStone2Index[0x0100] = 9;
			lookupStone2Index[0x0200] = 10;
			lookupStone2Index[0x0400] = 11;
			lookupStone2Index[0x0800] = 12;
			lookupStone2Index[0x1000] = 13;
			lookupStone2Index[0x2000] = 14;
			lookupStone2Index[0x4000] = 15;
			lookupStone2Index[0x8000] = 16;

			lookupPoint[0] = 0;
			lookupPoint[0x01] = 20;
			lookupPoint[0x02] = 20;
			lookupPoint[0x04] = 20;
			lookupPoint[0x08] = 20;
			lookupPoint[0x10] = 20;

			lookupPoint[0x20] = 30;
			lookupPoint[0x40] = 30;

			lookupPoint[0x80] = 50;
			lookupPoint[0x0100] = 50;

			lookupPoint[0x0200] = 70;
			lookupPoint[0x0400] = 70;

			lookupPoint[0x0800] = 130;
			lookupPoint[0x1000] = 130;

			lookupPoint[0x2000] = 30;
			lookupPoint[0x4000] = 30;

			lookupPoint[0x8000] = 10_000;
		}
	}
}
