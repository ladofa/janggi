using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public enum Stones : byte
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
}
