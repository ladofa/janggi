using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using static Janggi.StoneHelper;

namespace Janggi
{
	public class Gibo
	{
		public List<List<Board>> list;
		private object totalCount;

		public Gibo()
		{

		}

		public Gibo(string path)
		{
			Read(path);
		}

		public void Read(string path)
		{
			Encoding euckr = Encoding.GetEncoding(51949);
			FileStream stream = new FileStream(path, FileMode.Open);
			StreamReader reader = new StreamReader(stream, euckr);
			
			try
			{
				list = new List<List<Board>>();

				//컴플릿 셋팅
				bool completeTable = false;
				bool isMyFirst = false;
				bool isMyTurn = false;
				Board.Tables myTable = Board.Tables.Inner;
				Board.Tables yoTable = Board.Tables.Inner;

				//접장기의 경우
				uint[,] stones = null;

				//기타 기본
				int totalCount = 0;


				List<Board> history = null; 

				while (true)
				{
					string line = reader.ReadLine();
					Console.WriteLine(line);

					if (line[0] == '[')
					{
						int tagEnd = line.IndexOf(' ');
						string tag = line.Substring(1, tagEnd - 1);
						int quBegin = line.IndexOf('\"');
						int quEnd = line.LastIndexOf('\"');
						string content = line.Substring(quBegin + 1, quEnd - quBegin - 1);
						if (tag == "판")
						{
							//귀찮.
						}
						else if (tag == "총수")
						{
							totalCount = int.Parse(content);
						}
						else if (tag == "초차림")
						{
							if (content == "마상마상")
							{
								myTable = Board.Tables.Right;
							}
							else if (content == "상마상마")
							{
								myTable = Board.Tables.Left;
							}
							else if (content == "마상상마")
							{
								myTable = Board.Tables.Inner;
							}
							else
							{
								myTable = Board.Tables.Outer;
							}
							completeTable = true;
						}
						else if (tag == "한차림")
						{
							if (content == "마상마상")
							{
								yoTable = Board.Tables.Left;
							}
							else if (content == "상마상마")
							{
								yoTable = Board.Tables.Right;
							}
							else if (content == "마상상마")
							{
								yoTable = Board.Tables.Inner;
							}
							else
							{
								yoTable = Board.Tables.Outer;
							}
							completeTable = true;
						}
					}

					if (line.Length == 0)
					{
						//초기 셋팅
						if (stones != null)
						{
							Board board;
							if (completeTable)
							{
								board = new Board(myTable, yoTable, isMyFirst);
							}
							else
							{
								board = new Board();
								board.Set(stones, isMyFirst, isMyTurn);
							}

							history = new List<Board>();
							history.Add(board);

							stones = null;
						}
						//마무리
						else
						{
							if (completeTable != false)
							{
								list.Add(history);
							}

							stones = new uint[Board.Height, Board.Width];
							completeTable = false;
						}
					}
					//나머지 수순
					else
					{

					}

					
					




					if (reader.EndOfStream)
					{
						break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
