using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Janggi;

namespace Runner.Process
{
	public class GiboTest
	{
		public GiboTest()
		{

			Console.WriteLine("read gibos...");

			string[] allfiles = Directory.GetFiles("d:/dataset/gibo", "*.gib", SearchOption.AllDirectories);

			foreach (string path in allfiles)
			{
				Console.WriteLine(path);
				List<Gibo> gibos = Gibo.Read(path);

				List<Board> boards = gibos[0].GetParsed();

				if (boards.Count > 0)
				{
					Console.WriteLine("IS MY WIN : " + gibos[0].isMyWin);
					boards.Last().PrintStones();
				}
			}


			
		}
	}
}
