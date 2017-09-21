using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Janggi;

namespace Runner.Process
{
	public class GiboTest
	{
		public GiboTest()
		{
			Gibo gibo = new Gibo();
			gibo.Read(@"c:\Users\ladofa\Downloads\카카오장기기보\카카오장기기보001.gib");
		}
	}
}
