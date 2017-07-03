using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.StoneHelper;

namespace Janggi
{
	public class Board
	{
		uint[,] stones;
		uint[,] targets;
		uint[,] blocks;

		Pos[] positions;

		bool isMyTurn;
		public int Point;

		Move prevMove  = Move.Rest;
		public Move PrevMove
		{
			get => prevMove;
		}

		public bool IsMyTurn
		{
			get => isMyTurn;
		}

		readonly public static int Width = 9;
		readonly public static int Height = 10;
		readonly public static int StoneCount = 33;

		public Board(Board board)
		{
			stones = (uint[,])board.stones.Clone();
			targets = (uint[,])board.targets.Clone();
			blocks = (uint[,])board.blocks.Clone();
			positions = (Pos[])board.positions.Clone();

			Point = board.Point;
			isMyTurn = board.isMyTurn;
		}

		public enum Tables
		{
			Inner,
			Outer,
			Left,
			Right
		}

		public Board()
		{
			SetUp();
		}

		public void SetUp()
		{
			stones = new uint[Height, Width];
			targets = new uint[Height, Width];
			blocks = new uint[Height, Width];
			positions = new Pos[StoneCount];

			Point = 0;
		}

		public Board(Tables myTable, Tables yoTable, bool myFirst)
		{
			SetUp(myTable, yoTable, myFirst);
		}

		public bool IsMyFirst
		{
			get;set;
		}

		public void SetUp(Tables myTable, Tables yoTable, bool myfirst)
		{
			SetUp();

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

			if (myfirst)
			{
				Point = -15;
				isMyTurn = true;
			}
			else
			{
				Point = 15;
				isMyTurn = false;
			}

			IsMyFirst = myfirst;
		}

		public bool Equals(Board b)
		{
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (stones[y, x] != b.stones[y, x])
					{
						return false;
					}
				}
			}

			return true;
		}

		//상대방 입장에서 보도록 회전시킨다.
		public Board GetOpposite()
		{
			Board nuBoard = new Board();
			//이전 포석을 보관하고

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					//회전된 새로운 위치
					int nx = Width - x - 1;
					int ny = Height - y - 1;

					//편을 바꿔서 넣는다.
					nuBoard.stones[ny, nx] = Opposite(stones[y, x]);
				}
			}

			nuBoard.Point = -Point;

			return nuBoard;
		}

		public List<Move> GetAllMoves()
		{
			if (IsMyTurn)
			{
				return GetAllMyMoves();
			}
			else
			{
				return GetAllYoMoves();
			}
		}
		public List<Move> GetAllMyMoves()
		{
			List<Move> moves = new List<Move>();

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (IsMine(stones[y, x]))
					{
						moves.AddRange(GetAllMoves(new Pos(x, y)));
					}
				}
			}

			moves.Add(Move.Rest);
			return moves;
		}
		public List<Move> GetAllYoMoves()
		{
			List<Move> moves = new List<Move>();

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (IsYours(stones[y, x]))
					{
						moves.AddRange(GetAllMoves(new Pos(x, y)));
					}
				}
			}

			moves.Add(Move.Rest);
			return moves;
		}

		public uint this[Pos pos]
		{
			get => stones[pos.Y, pos.X];
		}

		public uint this[int y, int x]
		{
			get => stones[y, x];
		}

		static Tuple<Pos, Pos>[] wayAndBlockMa = new Tuple<Pos, Pos>[8]
					{new Tuple<Pos, Pos>(new Pos(-2, 1), new Pos(-1, 0))
				,new Tuple<Pos, Pos>(new Pos(-2, -1), new Pos(-1, 0))
				,new Tuple<Pos, Pos>(new Pos(-1, -2), new Pos(0, -1))
				,new Tuple<Pos, Pos>(new Pos(1, -2), new Pos(0, -1))
				,new Tuple<Pos, Pos>(new Pos(2, -1), new Pos(1, 0))
				,new Tuple<Pos, Pos>(new Pos(2, 1), new Pos(1, 0))
				,new Tuple<Pos, Pos>(new Pos(1, 2), new Pos(0, 1))
				,new Tuple<Pos, Pos>(new Pos(-1, 2), new Pos(0, 1))
						};
		static Tuple<Pos, Pos, Pos>[] wayAndBlockSang = new Tuple<Pos, Pos, Pos>[8]
					{
				new Tuple<Pos, Pos, Pos>(new Pos(-3, 2), new Pos(-2, 1), new Pos(-1, 0))
				,new Tuple<Pos, Pos, Pos>(new Pos(-3, -2), new Pos(-2, -1), new Pos(-1, 0))
				,new Tuple<Pos, Pos, Pos>(new Pos(-2, -3), new Pos(-1, -2), new Pos(0, -1))
				,new Tuple<Pos, Pos, Pos>(new Pos(2, -3), new Pos(1, -2), new Pos(0, -1))
				,new Tuple<Pos, Pos, Pos>(new Pos(3, -2), new Pos(2, -1), new Pos(1, 0))
				,new Tuple<Pos, Pos, Pos>(new Pos(3, 2), new Pos(2, 1), new Pos(1, 0))
				,new Tuple<Pos, Pos, Pos>(new Pos(2, 3), new Pos(1, 2), new Pos(0, 1))
				,new Tuple<Pos, Pos, Pos>(new Pos(-2, 3), new Pos(-1, 2), new Pos(0, 1))
					};
		static List<Pos>[,] wayInGoong = new List<Pos>[,]
			{
				{ new List<Pos>(){ new Pos(1, 0), new Pos(1, 1), new Pos(0, 1) },
					new List<Pos>(){ new Pos(2, 0), new Pos(1, 1), new Pos(0, 0) },
					new List<Pos>(){ new Pos(2, 1), new Pos(1, 1), new Pos(1, 0) }
				},

				{ new List<Pos>(){ new Pos(0, 0), new Pos(1, 1), new Pos(0, 2) },
					new List<Pos>(){ new Pos(0, 1), new Pos(0, 0), new Pos(1, 0), new Pos(2, 0), new Pos(2, 1), new Pos(2, 2), new Pos(1, 2), new Pos(0, 2)},
					new List<Pos>(){ new Pos(1, 1), new Pos(2, 0), new Pos(2, 2) }
				},

				{ new List<Pos>(){ new Pos(0, 1), new Pos(1, 1), new Pos(1, 2) },
					new List<Pos>(){ new Pos(0, 2), new Pos(1, 1), new Pos(2, 2) },
					new List<Pos>(){ new Pos(1, 2), new Pos(1, 1), new Pos(2, 1) }
				}
			};
		static Pos[] wayJol = {
			new Pos(-1, 0), new Pos(1, 0), new Pos(0, -1), new Pos(0, 1)
		};

		public void SetMoves(Pos pos)
		{
			int px = pos.X;
			int py = pos.Y;

			uint stoneFrom = this[pos];
			if (stoneFrom == 0)
			{
				return;
			}
			else if (IsCha(stoneFrom))
			{
				bool confirmAndAdd(int x, int y)
				{
					uint stoneTo = stones[y, x];
					if (stones[y, x] == 0)
					{
						targets[y, x] |= stoneFrom;
						return true;
					}
					else
					{
						if (!IsAllied(stoneFrom, stoneTo))
						{
							targets[y, x] |= stoneFrom;
						}
						else
						{
							blocks[y, x] |= stoneFrom;
						}
						return false;
					}
				}
				for (int y = py - 1; y >= 0; y--)
				{
					if (!confirmAndAdd(px, y))
					{
						break;
					}
				}

				for (int y = py + 1; y < Height; y++)
				{
					if (!confirmAndAdd(px, y))
					{
						break;
					}
				}

				for (int x = px - 1; x >= 0; x--)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}

				for (int x = px + 1; x < Width; x++)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}

				//궁 안에서 대각선 움직임 검사
				//좌상
				if (px == 3 && (py == 0 || py == 7))
				{
					for (int x = px + 1, y = py + 1; x < 6; x++, y++)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//좌하
				else if (px == 3 && (py == 2 || py == 9))
				{
					for (int x = px + 1, y = py - 1; x < 6; x++, y--)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//우상
				else if (px == 5 && (py == 0 || py == 7))
				{
					for (int x = px - 1, y = py + 1; x >= 3; x--, y++)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//우하
				else if (px == 5 && (py == 2 || py == 9))
				{
					for (int x = px - 1, y = py - 1; x >= 3; x--, y--)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//TODO : 궁 가운데 있을 경우
			}
			else if (IsPo(stoneFrom))
			{
				bool dari = false;
				bool confirmAndAdd(int x, int y)
				{
					uint stoneTo = stones[y, x];
					//다리가 없으면 다리를 발견한다.
					if (dari == false)
					{
						blocks[y, x] |= stoneFrom;
						if (stoneTo == 0)
						{
							return true;
						}
						else if (!IsPo(stoneTo))
						{	
							dari = true;
							return true;
						}
						else
						{
							return false;
						}
					}
					//다리를 발견한 뒤로는 차와 같은데 포만 못 먹는다.
					else
					{
						if (stones[y, x] == 0)
						{
							targets[y, x] |= stoneFrom;
							return true;
						}
						else
						{
							if (!IsAllied(stoneFrom, stoneTo) && !IsPo(stoneTo))
							{
								targets[y, x] |= stoneFrom;
							}
							else
							{
								blocks[y, x] |= stoneFrom;
							}
							return false;
						}
					}
				}

				dari = false;
				for (int y = py - 1; y >= 0; y--)
				{
					if (!confirmAndAdd(px, y))
					{
						break;
					}
				}

				dari = false;
				for (int y = py + 1; y < Height; y++)
				{
					if (!confirmAndAdd(px, y))
					{
						break;
					}
				}

				dari = false;
				for (int x = px - 1; x >= 0; x--)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}

				dari = false;
				for (int x = px + 1; x < Width; x++)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}

				//궁 안에서 대각선 움직임 검사
				//좌상
				if (px == 3 && (py == 0 || py == 7))
				{
					dari = false;
					for (int x = px + 1, y = py + 1; x < 6; x++, y++)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//좌하
				else if (px == 3 && (py == 2 || py == 9))
				{
					dari = false;
					for (int x = px + 1, y = py - 1; x < 6; x++, y--)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//우상
				else if (px == 5 && (py == 0 || py == 7))
				{
					dari = false;
					for (int x = px - 1, y = py + 1; x >= 3; x--, y++)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//우하
				else if (px == 5 && (py == 2 || py == 9))
				{
					dari = false;
					for (int x = px - 1, y = py - 1; x >= 3; x--, y--)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}

				//TODO : 궁 가운데 있을 경우
			}
			else if (IsMa(stoneFrom))
			{
				//8개의 착수 가능점을 일일이 확인한다.
				//좌-하부터 시계방향으로

				//길과 멱의 상대적 위치
				for (int i = 0; i < 8; i++)
				{
					Pos nu = pos + wayAndBlockMa[i].Item1;
					//경계 밖으로 나갈 경우
					if (nu.X < 0 || nu.X >= Width || nu.Y < 0 || nu.Y >= Height)
					{
						continue;
					}

					Pos block = pos + wayAndBlockMa[i].Item2;
					if (this[block] != 0)
					{
						blocks[block.Y, block.X] |= stoneFrom;
						continue;
					}

					targets[nu.Y, nu.X] |= stoneFrom;
				}
			}
			else if (IsSang(stoneFrom))
			{
				//길과 멱의 상대적 위치
				for (int i = 0; i < 8; i++)
				{
					Pos to = pos + wayAndBlockSang[i].Item1;
					//경계 밖으로 나갈 경우
					if (to.X < 0 || to.X >= Width || to.Y < 0 || to.Y >= Height)
					{
						continue;
					}

					Pos block1 = pos + wayAndBlockSang[i].Item2;
					bool blocked = false;
					if (this[block1] != 0)
					{
						blocks[block1.Y, block1.X] |= stoneFrom;
						blocked = true;
					}

					Pos block2 = pos + wayAndBlockSang[i].Item3;
					if (this[block2] != 0)
					{
						blocks[block2.Y, block2.X] |= stoneFrom;
						blocked = true;
					}

					if (!blocked)
					{
						targets[to.Y, to.X] |= stoneFrom;
					}
				}
			}
			//궁/사
			else if (IsKing(stoneFrom) || IsSa(stoneFrom))
			{
				Pos origin;

				if (IsMine(stoneFrom))
				{
					origin = new Pos(3, 7);
				}
				else
				{
					origin = new Pos(3, 0);
				}

				Pos relPos = pos - origin;
				foreach (var e in wayInGoong[relPos.Y, relPos.X])
				{
					Pos to = origin + e;
					uint stoneTo = this[to];
					if (stoneTo == 0 || !IsAllied(stoneFrom, stoneTo))
					{
						targets[to.Y, to.X] |= stoneFrom;
					}
					else
					{
						blocks[to.Y, to.X] |= stoneFrom;
					}
				}
			}
			//졸
			else if (IsJol(stoneFrom) && IsMine(stoneFrom))
			{
				//TODO : else를 만들어야 하는데...
				if (px - 1 >= 0 && !IsAllied(stoneFrom, stones[py, px - 1]))
				{
					targets[px - 1, py] |= stoneFrom;
				}

				if (px + 1 < Width && !IsAllied(stoneFrom, stones[py, px + 1]))
				{
					targets[px + 1, py] |= stoneFrom;
				}

				if (py - 1 >= 0 && !IsAllied(stoneFrom, stones[py - 1, px]))
				{
					targets[px, py - 1] |= stoneFrom;
				}

				//우상으로 진출
				if (pos.Equals(3, 2) && !IsAllied(stoneFrom, stones[1, 4]))
				{
					targets[4, 1] |= stoneFrom;
				}
				else if (pos.Equals(4, 1) && !IsAllied(stoneFrom, stones[0, 5]))
				{
					targets[5, 0] |= stoneFrom;
				}
				//좌상으로 진출
				else if (pos.Equals(5, 2) && !IsAllied(stoneFrom, stones[1, 4]))
				{
					targets[4, 1] |= stoneFrom;
				}
				else if (pos.Equals(4, 1) && !IsAllied(stoneFrom, stones[0, 3]))
				{
					targets[3, 0] |= stoneFrom;
				}
			}
			else if (IsJol(stoneFrom) && IsYours(stoneFrom))
			{
				if (px - 1 >= 0 && !IsAllied(stoneFrom, stones[py, px - 1]))
				{
					targets[px - 1, py] |= stoneFrom;
				}

				if (px + 1 < Width && !IsAllied(stoneFrom, stones[py, px + 1]))
				{
					targets[px + 1, py] |= stoneFrom;
				}

				if (py + 1 < Height && !IsAllied(stoneFrom, stones[py + 1, px]))
				{
					targets[px, py + 1] |= stoneFrom;
				}

				//우하로 진출
				if (pos.Equals(3, 7) && !IsAllied(stoneFrom, stones[8, 4]))
				{
					targets[4, 8] |= stoneFrom;
				}
				else if (pos.Equals(4, 8) && !IsAllied(stoneFrom, stones[9, 5]))
				{
					targets[5, 9] |= stoneFrom;
				}
				//좌하로 진출
				else if (pos.Equals(5, 7) && !IsAllied(stoneFrom, stones[8, 4]))
				{
					targets[4, 8] |= stoneFrom;
				}
				else if (pos.Equals(4, 8) && !IsAllied(stoneFrom, stones[9, 3]))
				{
					targets[3, 9] |= stoneFrom;
				}
			}
			else
			{
				throw new Exception("ERROR");
			}
		}

		public void MoveNext(Move move)
		{
			prevMove = move;
			if (!move.IsRest)
			{
				Point += GetPoint(this[move.To]);
				stones[move.To.Y, move.To.X] = stones[move.From.Y, move.From.X];
				stones[move.From.Y, move.From.X] = 0;
			}
			
			isMyTurn = !isMyTurn;
		}

		public Board GetNext(Move move)
		{
			Board board = new Board(this);
			board.MoveNext(move);
			return board;
		}

		public bool IsMyWin
		{
			get => Point > 5000;
		}

		public bool IsYoWin
		{
			get => Point < -5000;
		}


		#region 정책망 관련

		public int ExpectedPoint(Move move)
		{
			return Point + GetPoint(this[move.To]);
		}

		//총 잡을 수 있는 기물
		public int CountTarget(List<Move> moves)
		{
			int sum = 0;
			foreach (var move in moves)
			{
				if (!move.IsRest)
				{
					if (this[move.To] != 0)
					{
						sum++;
					}
				}
			}

			return sum;
		}

		#endregion


		#region 텍스트 출력

		static string[] lettersCho = {
			"┼",
			"卒", "象", "馬", "包", "車", "士", "楚",
			"兵", "象", "馬", "包", "車", "士", "漢",
		};

		static string[] lettersHan = {
			"┼",
			"兵", "象", "馬", "包", "車", "士", "漢",
			"卒", "象", "馬", "包", "車", "士", "楚",
		};

		public string ToStringStones()
		{
			string[] letters;
			if (IsMyTurn)
			{
				letters = lettersCho;
			}
			else
			{
				letters = lettersHan;
			}
			string result = "";
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					result += (letters[(int)this[y, x]] + " ");
				}
				result += '\n';
			}

			return result;
		}

		public void PrintStones()
		{
			string[] letters;
			bool colorInverse;
			if (IsMyFirst)
			{
				letters = lettersCho;
				colorInverse = false;
			}
			else
			{
				letters = lettersHan;
				colorInverse = true;
			}
			string result = "";
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					uint stone = this[y, x];

					if (prevMove.To.Equals(x, y))
					{
						Console.BackgroundColor = ConsoleColor.DarkYellow;
					}
					else
					{
						Console.BackgroundColor = ConsoleColor.Black;
					}

					if (stone == 0)
					{
						Console.ForegroundColor = ConsoleColor.Gray;
						
					}
					else if (IsMine(stone) ^ colorInverse)
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
					}

					
					Console.Write(GetLetter(stone, IsMyFirst));
					
					
					Console.BackgroundColor = ConsoleColor.Black;
					Console.Write(" ");
				}
				Console.WriteLine();
			}

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Black;
		}

		#endregion
	}
}
