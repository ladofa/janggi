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
			gibo.Read(@"C:\gib\아마고수기보1\아마고수기보001.gib");
		}
	}
}
