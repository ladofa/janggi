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
		public List<List<Board>> historyList;
		public List<int> isMyWinList;
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


			historyList = new List<List<Board>>();
			isMyWinList = new List<int>();

			//컴플릿 셋팅
			bool completeTable = false;
			bool isMyFirst = true;
			Board.Tables myTable = Board.Tables.Inner;
			Board.Tables yoTable = Board.Tables.Inner;

			//접장기의 경우
			uint[,] stones = new uint[Board.Height, Board.Width];

			//기타 기본
			Board board = null;
			int totalCount = 0;
			List<Board> history = null;
			int isMyWin = -1;

			while (true)
			{
				string line = reader.ReadLine();
				//Console.WriteLine(line);

				if (line.Length == 0)
				{
					//초기 셋팅
					if (stones != null)
					{
						if (completeTable)
						{
							board = new Board(myTable, yoTable, isMyFirst);
						}
						else
						{
							board = new Board();
							board.Set(stones, isMyFirst, isMyFirst);
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
							historyList.Add(history);
							isMyWinList.Add(isMyWin);
						}

						stones = new uint[Board.Height, Board.Width];
						completeTable = false;
					}
				}
				else if (line[0] == '{')
				{
					while (true)
					{
						if (line.IndexOf('}') != -1)
						{
							break;
						}
						line = reader.ReadLine();
					}
					continue;
				}
				else if (line[0] == '[')
				{
					int tagEnd = line.IndexOf(' ');
					string tag = line.Substring(1, tagEnd - 1);
					int quBegin = line.IndexOf('\"');
					int quEnd = line.LastIndexOf(']');
					string content = line.Substring(quBegin + 1, quEnd - quBegin - 2);
					if (tag == "판")
					{
						//귀찮.
					}
					else if (tag == "총수")
					{
						totalCount = int.Parse(content);
					}
					else if (tag == "대국결과")
					{
						if (content[0] == '초')
						{
							isMyWin = 1;
						}
						else if (content[0] == '한')
						{
							isMyWin = 0;
						}
						else
						{
							isMyWin = -1;
						}
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


				//나머지 수순
				else
				{
					string[] words = line.Split('.');

					for (int k = 1; k < words.Length; k++)
					{
						string word = words[k];

						Move move;
						if (word[1] == '한')//수쉼
						{
							move = Move.Empty;
						}
						else
						{
							sbyte[] posList = new sbyte[4];
							int posIndex = 0;
							for (int i = 1; i < word.Length && posIndex < 4; i++)
							{
								bool succeed = int.TryParse(word[i].ToString(), out int result);
								if (succeed)
								{
									result--;
									if (result == -1) result = 9;
									posList[posIndex++] = (sbyte)result;
								}
							}

							move = new Move(posList[1], posList[0], posList[3], posList[2]);
						}
						board = board.GetNext(move);
						history.Add(board);
					}
				}

				if (reader.EndOfStream)
				{
					break;
				}
			}

			//foreach (var h in historyList)
			//{
			//	foreach (var b in h)
			//	{
			//		b.PrintStones();
			//		Console.WriteLine();
			//		Console.ReadKey();
			//	}
			//}

		}
	}
}
