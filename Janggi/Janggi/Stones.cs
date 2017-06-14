using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public struct Stone
	{
		public enum Val : byte
		{
			Empty = 0,
			MyJol = 1 + 128,
			MySang = 2 + 128,
			MyMa = 3 + 128,
			MyPo = 4 + 128,
			MyCha = 5 + 128,
			MySa = 6 + 128,
			MyGoong = 7 + 128,
			YoJol = 1 + 64,
			YoSang = 2 + 64,
			YoMa = 3 + 64,
			YoPo = 4 + 64,
			YoCha = 5 + 64,
			YoSa = 6 + 64,
			YoGoong = 7 + 64
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
			get => (byte)val > 128;
		}

		public bool IsYo
		{
			get => val != 0 && (byte)val < 128;
		}

		public bool IsAlliesWith(Stone stone)
		{
			if (val == 0 || stone.val == 0)
			{
				return false;
			}

			return !((byte)val > 128 ^ (byte)stone.val > 128);
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
				else if ((byte)val > 128)
				{
					return new Stone(val - 64);
				}
				else
				{
					return new Stone(val + 64);
				}
			}
		}
	}
}
