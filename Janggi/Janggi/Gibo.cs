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
		public List<Move> moves;
		public int isMyWin;
		public Dictionary<string, string> matchInfo;

		public Gibo()
		{

		}

		public Gibo(string path)
		{
			Read(path);
		}

		public static List<Gibo> Read(string path)
		{
			FileStream stream = new FileStream(path, FileMode.Open);
			MemoryStream ms = new MemoryStream();

			while (true)
			{
				int val = stream.ReadByte();
				if (val < 0) break; //case of -1
				if (val == 255) continue; //case of 0xff
				ms.WriteByte((byte)val);
			}

			string raw = Encoding.GetEncoding(51949).GetString(ms.GetBuffer());

			List<Gibo> gibos = new List<Gibo>();

			string[] lines = raw.Split('\n');

			Dictionary<string, string> matchInfo = new Dictionary<string, string>();
			List<Move> moves = new List<Move>();

			bool commentFound = false;

			foreach (string line in lines)
			{
				//Console.WriteLine(line);

				//코멘트가 발견되면
				if (line.Contains('{'))
				{
					commentFound = true;
				}

				//종료 문자가 나올 때까지 스킵한다.
				if (commentFound)
				{
					if (line.Contains('}'))
					{
						commentFound = false;
					}
					continue;
				}

				//모든 정보를 다 읽었으므로 저장한다.
				if (line.Length == 0)
				{
					//초기 셋팅
					if (moves.Count > 0)
					{
						Gibo gibo = new Gibo();
						gibo.matchInfo = matchInfo;
						gibo.moves = moves;
						gibos.Add(gibo);

						matchInfo = new Dictionary<string, string>();
						moves = new List<Move>();
					}
				}
				else if (line[0] == '[')
				{
					int tagEnd = line.IndexOf(' ');
					string tag = line.Substring(1, tagEnd - 1);
					int quBegin = line.IndexOf('\"');
					int quEnd = line.LastIndexOf(']');
					string content = line.Substring(quBegin + 1, quEnd - quBegin - 2);

					matchInfo[tag] = content;					
				}
				//나머지 수순
				else
				{
					string[] words = line.Split(' ');
					//<0>과 같은 문자를 제거한다.
					words = (from word in words
							where (word != "<0>" & word != "\x1a")
							select word).ToArray();
					
					for (int k = 0; k < words.Length; k += 2)
					{
						string wordNum = words[k];
						string wordMove = words[k + 1];

						Move move;
						if (wordMove[0] == '한')//수쉼
						{
							move = Move.Empty;
						}
						else
						{
							int fromY = int.Parse(wordMove[0].ToString()) - 1;
							int fromX = int.Parse(wordMove[1].ToString()) - 1;

							int toY = int.Parse(wordMove[3].ToString()) - 1;
							int toX = int.Parse(wordMove[4].ToString()) - 1;

							if (fromY == -1) fromY = 9;
							if (toY == -1) toY = 9;

							move = new Move((sbyte)fromX, (sbyte)fromY, (sbyte)toX, (sbyte)toY);
						}
						moves.Add(move);
					}
				}
			}

			return gibos;
		}

		public List<Board> GetParsed()
		{
			bool completeTable = false;
			Board.Tables myTable = Board.Tables.Inner;
			Board.Tables yoTable = Board.Tables.Inner;
			uint[,] stones = null;
   
			foreach (KeyValuePair<string, string> pair in matchInfo)
			{
				string tag = pair.Key;
				string content = pair.Value;
				if (tag == "판")
				{
					stones = new uint[Board.Height, Board.Width];
					string[] lines = content.Split('/');

					int x = 0;
					int y = 0;

					HashSet<uint> existingStones = new HashSet<uint>();

					foreach (string line in lines)
					{
						foreach (char letter in line)
						{
							bool parsed = int.TryParse(letter.ToString(), out int result);
							//숫자라면
							if (parsed)
							{
								x += result;
							}
							//숫자가 아니면
							else
							{
								void insertStone(uint[] inputStones)
								{
									foreach (uint stone in inputStones)
									{
										if (!existingStones.Contains(stone))
										{
											stones[y, x] = stone;
											existingStones.Add(stone);
											break;
										}
									}
								}
								if (letter == '차')
									insertStone(new []{ Stones.MyCha1, Stones.MyCha2});

								else if (letter == '상')
									insertStone(new[] { Stones.MySang1, Stones.MySang2 });

								else if (letter == '마')
									insertStone(new[] { Stones.MyMa1, Stones.MyMa2 });

								else if (letter == '포')
									insertStone(new[] { Stones.MyPo1, Stones.MyPo2 });

								else if (letter == '사')
									insertStone(new[] { Stones.MySa1, Stones.MySa2 });

								else if (letter == '장')
									insertStone(new[] { Stones.MyKing });

								else if (letter == '졸')
									insertStone(new[] { Stones.MyJol1, Stones.MyJol2,
										Stones.MyJol3, Stones.MyJol4, Stones.MyJol5 });

								else if (letter == '車')
									insertStone(new[] { Stones.YoCha1, Stones.YoCha2 });

								else if (letter == '象')
									insertStone(new[] { Stones.YoSang1, Stones.YoSang2 });

								else if (letter == '馬')
									insertStone(new[] { Stones.YoMa1, Stones.YoMa2 });

								else if (letter == '包')
									insertStone(new[] { Stones.YoPo1, Stones.YoPo2 });

								else if (letter == '士')
									insertStone(new[] { Stones.YoSa1, Stones.YoSa2 });

								else if (letter == '將')
									insertStone(new[] { Stones.YoKing });

								else if (letter == '兵')
									insertStone(new[] { Stones.YoJol1, Stones.YoJol2,
										Stones.YoJol3, Stones.YoJol4, Stones.YoJol5 });

								else if (letter == ' ') //그리고 아마도 한 접장기
									break;

								else
									throw new Exception("unknown letter! : " + letter);
								
							}
							x++;
						}
						y++;
					}
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
				else if (tag == "초차림" || tag == "초포진")
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
				else if (tag == "한차림" || tag == "한포진")
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
			} //end of foreach


			bool myFirst = true;
			if (completeTable)
			{
				stones = GetStones(myTable, yoTable);
			}
			Pos starting = moves[0].From;

			//한이 아래쪽에 있으면 일단 바꿈
			if (stones[8, 4] == Stones.YoKing)
			{
				stones = GetOpposite(stones);
				moves = Move.GetOpposite(moves);
			}

			//상대방부터 시작하면
			if (IsYours(stones[starting.Y, starting.X]))
			{
				myFirst = false;
			}

			Board initBoard = new Board(stones, myFirst, false);

			List<Board> boards = new List<Board>();

			boards.Add(initBoard);

			return boards;
		}
	}
}
