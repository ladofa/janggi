using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Board;

namespace Janggi
{
	public static class StoneHelper
	{
		public struct Stones
		{
			public const uint Empty = 0,
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

			Jol    = 0x00_1F_00_1F,
			MyJol  = 0x00_00_00_1F,
			YoJol  = 0x00_1F_00_00,

			Sang   = 0x00_60_00_60,
			MySang = 0x00_00_00_60,
			YoSang = 0x00_60_00_00,

			Ma     = 0x01_80_01_80,
			MyMa   = 0x00_00_01_80,
			YoMa   = 0x01_80_00_00,

			Po     = 0x06_00_06_00,
			MyPo   = 0x00_00_06_00,
			YoPo   = 0x06_00_00_00,

			Cha    = 0x18_00_18_00,
			MyCha  = 0x00_00_18_00,
			YoCha  = 0x18_00_00_00,

			Sa     = 0x60_00_60_00,
			MySa   = 0x00_00_60_00,
			YoSa   = 0x60_00_00_00,

			King   = 0x80_00_80_00,

			Mine   = 0x00_00_FF_FF,
			Yours  = 0xFF_FF_00_00;
		};

		public static bool IsEmpty(uint s) => s == (uint)Stones.Empty;
		public static bool IsJol(uint s) => (s & (uint)Stones.Jol) != 0;
		public static bool IsMyJol(uint s) => (s & (uint)Stones.MyJol) != 0;
		public static bool IsYoJol(uint s) => (s & (uint)Stones.YoJol) != 0;
		public static bool IsSang(uint s) => (s & (uint)Stones.Sang) != 0;
		public static bool IsMa(uint s) => (s & (uint)Stones.Ma) != 0;
		public static bool IsPo(uint s) => (s & (uint)Stones.Po) != 0;
		public static bool IsMyPo(uint s) => (s & (uint)Stones.MyPo) != 0;
		public static bool IsYoPo(uint s) => (s & (uint)Stones.YoPo) != 0;
		public static bool IsCha(uint s) => (s & (uint)Stones.Cha) != 0;
		public static bool IsMyCha(uint s) => (s & (uint)Stones.MyCha) != 0;
		public static bool IsYoCha(uint s) => (s & (uint)Stones.YoCha) != 0;
		public static bool IsSa(uint s) => (s & (uint)Stones.Sa) != 0;
		public static bool IsKing(uint s) => (s & (uint)Stones.King) != 0;
		public static bool IsMine(uint s) => (s & (uint)Stones.Mine) != 0;
		public static bool IsYours(uint s) => (s & (uint)Stones.Yours) != 0;

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

		public static uint[,] GetStones(Tables myTable, Tables yoTable)
		{
			uint[,] stones = new uint[Height, Width];
			stones[0, 0] = (uint)Stones.YoCha1;
			stones[0, 3] = (uint)Stones.YoSa1;
			stones[0, 5] = (uint)Stones.YoSa2;
			stones[0, 8] = (uint)Stones.YoCha2;
			stones[1, 4] = (uint)Stones.YoKing;
			stones[2, 1] = (uint)Stones.YoPo1;
			stones[2, 7] = (uint)Stones.YoPo2;
			stones[3, 0] = (uint)Stones.YoJol1;
			stones[3, 2] = (uint)Stones.YoJol2;
			stones[3, 4] = (uint)Stones.YoJol3;
			stones[3, 6] = (uint)Stones.YoJol4;
			stones[3, 8] = (uint)Stones.YoJol5;

			stones[6, 0] = (uint)Stones.MyJol1;
			stones[6, 2] = (uint)Stones.MyJol2;
			stones[6, 4] = (uint)Stones.MyJol3;
			stones[6, 6] = (uint)Stones.MyJol4;
			stones[6, 8] = (uint)Stones.MyJol5;
			stones[7, 1] = (uint)Stones.MyPo1;
			stones[7, 7] = (uint)Stones.MyPo2;
			stones[8, 4] = (uint)Stones.MyKing;
			stones[9, 0] = (uint)Stones.MyCha1;
			stones[9, 3] = (uint)Stones.MySa1;
			stones[9, 5] = (uint)Stones.MySa2;
			stones[9, 8] = (uint)Stones.MyCha2;

			if (myTable == Tables.Inner)
			{
				stones[9, 1] = (uint)Stones.MyMa1;
				stones[9, 2] = (uint)Stones.MySang1;
				stones[9, 6] = (uint)Stones.MySang2;
				stones[9, 7] = (uint)Stones.MyMa2;
			}
			else if (myTable == Tables.Outer)
			{
				stones[9, 1] = (uint)Stones.MySang1;
				stones[9, 2] = (uint)Stones.MyMa1;
				stones[9, 6] = (uint)Stones.MyMa2;
				stones[9, 7] = (uint)Stones.MySang2;
			}
			else if (myTable == Tables.Left)
			{
				stones[9, 1] = (uint)Stones.MySang1;
				stones[9, 2] = (uint)Stones.MyMa1;
				stones[9, 6] = (uint)Stones.MySang2;
				stones[9, 7] = (uint)Stones.MyMa2;
			}
			else
			{
				stones[9, 1] = (uint)Stones.MyMa1;
				stones[9, 2] = (uint)Stones.MySang1;
				stones[9, 6] = (uint)Stones.MyMa2;
				stones[9, 7] = (uint)Stones.MySang2;
			}

			if (yoTable == Tables.Inner)
			{
				stones[0, 1] = (uint)Stones.YoMa1;
				stones[0, 2] = (uint)Stones.YoSang1;
				stones[0, 6] = (uint)Stones.YoSang2;
				stones[0, 7] = (uint)Stones.YoMa2;
			}
			else if (yoTable == Tables.Outer)
			{
				stones[0, 1] = (uint)Stones.YoSang1;
				stones[0, 2] = (uint)Stones.YoMa1;
				stones[0, 6] = (uint)Stones.YoMa2;
				stones[0, 7] = (uint)Stones.YoSang2;
			}
			else if (yoTable == Tables.Left)
			{
				stones[0, 1] = (uint)Stones.YoMa1;
				stones[0, 2] = (uint)Stones.YoSang1;
				stones[0, 6] = (uint)Stones.YoMa2;
				stones[0, 7] = (uint)Stones.YoSang2;
			}
			else
			{
				stones[0, 1] = (uint)Stones.YoSang1;
				stones[0, 2] = (uint)Stones.YoMa1;
				stones[0, 6] = (uint)Stones.YoSang2;
				stones[0, 7] = (uint)Stones.YoMa2;
			}

			return stones;
		}

		public static uint[,] GetFlipVer(uint[,] stones)
		{
			uint[,] nuStones = new uint[Height, Width];
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					//회전된 새로운 위치
					int nx = Width - x - 1;
					int ny = Height - y - 1;

					//편을 바꿔서 넣는다.
					nuStones[ny, nx] = stones[y, x];
				}
			}

			return nuStones;
		}

		public static uint[,] GetFlip(uint[,] stones)
		{
			uint[,] nuStones = new uint[Height, Width];
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					//회전된 새로운 위치
					int nx = Width - x - 1;
					int ny = y;

					//편 바꾸지 않고 그냥 넣어야지.
					nuStones[ny, nx] = stones[y, x];
				}
			}

			return nuStones;
		}


		public static uint Index2Stone(int index)
		{
			return (uint)1 << index;
		}

		private static int[] lookupStone2Index = new int[0x8001];

		/// <summary>
		/// stone 종류를 0부터 32까지의 숫자로 나타낸다.
		/// </summary>
		/// <param name="stone"></param>
		/// <returns></returns>
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

		public static string GetLetter(uint stone, bool myDum)
		{
			return GetLetter(Stone2Index(stone), myDum);
		}

		private static string[] lookupLetter = {
			"＋",
			"卒","卒","卒","卒","卒",
			"象","象","馬", "馬","包","包", "車", "車","士","士", "楚",
			"兵","兵","兵","兵","兵",
			"象","象","馬", "馬","包","包", "車", "車","士","士", "漢",
		};
	

		public static string GetLetter(int index, bool myDum)
		{
			//초한을 서로 바꿔준다...
			if (myDum)
			{
				if (index == 0)
				{
					return lookupLetter[0];
				}
				if (index > 16)
				{
					return lookupLetter[index - 16];
				}
				else
				{
					return lookupLetter[index + 16];
				}
			}
			else
			{
				return lookupLetter[index];
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
