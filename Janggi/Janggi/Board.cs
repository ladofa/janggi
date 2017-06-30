using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public class Board
	{
		Stone[,] stones;
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

		public static int Width = 9;
		public static int Height = 10;

		public Board(Board board)
		{
			stones = new Stone[Height, Width];
			for (int i = 0; i < Height; i++)
			{
				for (int k = 0; k < Width; k++)
				{
					stones[i, k] = board.stones[i, k];
				}
			}

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
			stones = new Stone[Height, Width];
			for (int i = 0; i < Height; i++)
			{
				for (int k = 0; k < Width; k++)
				{
					stones[i, k] = new Stone();
				}
			};

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

			stones[0, 0] = new Stone(Stone.Val.YoCha);
			stones[0, 3] = new Stone(Stone.Val.YoSa);
			stones[0, 5] = new Stone(Stone.Val.YoSa);
			stones[0, 8] = new Stone(Stone.Val.YoCha);
			stones[1, 4] = new Stone(Stone.Val.YoGoong);
			stones[2, 1] = new Stone(Stone.Val.YoPo);
			stones[2, 7] = new Stone(Stone.Val.YoPo);
			stones[3, 0] = new Stone(Stone.Val.YoJol);
			stones[3, 2] = new Stone(Stone.Val.YoJol);
			stones[3, 4] = new Stone(Stone.Val.YoJol);
			stones[3, 6] = new Stone(Stone.Val.YoJol);
			stones[3, 8] = new Stone(Stone.Val.YoJol);

			stones[6, 0] = new Stone(Stone.Val.MyJol);
			stones[6, 2] = new Stone(Stone.Val.MyJol);
			stones[6, 4] = new Stone(Stone.Val.MyJol);
			stones[6, 6] = new Stone(Stone.Val.MyJol);
			stones[6, 8] = new Stone(Stone.Val.MyJol);
			stones[7, 1] = new Stone(Stone.Val.MyPo);
			stones[7, 7] = new Stone(Stone.Val.MyPo);
			stones[8, 4] = new Stone(Stone.Val.MyGoong);
			stones[9, 0] = new Stone(Stone.Val.MyCha);
			stones[9, 3] = new Stone(Stone.Val.MySa);
			stones[9, 5] = new Stone(Stone.Val.MySa);
			stones[9, 8] = new Stone(Stone.Val.MyCha);

			if (myTable == Tables.Inner)
			{
				stones[9, 1] = new Stone(Stone.Val.MyMa);
				stones[9, 2] = new Stone(Stone.Val.MySang);
				stones[9, 6] = new Stone(Stone.Val.MySang);
				stones[9, 7] = new Stone(Stone.Val.MyMa);
			}
			else if (myTable == Tables.Outer)
			{
				stones[9, 1] = new Stone(Stone.Val.MySang);
				stones[9, 2] = new Stone(Stone.Val.MyMa);
				stones[9, 6] = new Stone(Stone.Val.MyMa);
				stones[9, 7] = new Stone(Stone.Val.MySang);
			}
			else if (myTable == Tables.Left)
			{
				stones[9, 1] = new Stone(Stone.Val.MySang);
				stones[9, 2] = new Stone(Stone.Val.MyMa);
				stones[9, 6] = new Stone(Stone.Val.MySang);
				stones[9, 7] = new Stone(Stone.Val.MyMa);
			}
			else
			{
				stones[9, 1] = new Stone(Stone.Val.MyMa);
				stones[9, 2] = new Stone(Stone.Val.MySang);
				stones[9, 6] = new Stone(Stone.Val.MyMa);
				stones[9, 7] = new Stone(Stone.Val.MySang);
			}

			if (yoTable == Tables.Inner)
			{
				stones[0, 1] = new Stone(Stone.Val.YoMa);
				stones[0, 2] = new Stone(Stone.Val.YoSang);
				stones[0, 6] = new Stone(Stone.Val.YoSang);
				stones[0, 7] = new Stone(Stone.Val.YoMa);
			}
			else if (yoTable == Tables.Outer)
			{
				stones[0, 1] = new Stone(Stone.Val.YoSang);
				stones[0, 2] = new Stone(Stone.Val.YoMa);
				stones[0, 6] = new Stone(Stone.Val.YoMa);
				stones[0, 7] = new Stone(Stone.Val.YoSang);
			}
			else if (yoTable == Tables.Left)
			{
				stones[0, 1] = new Stone(Stone.Val.YoMa);
				stones[0, 2] = new Stone(Stone.Val.YoSang);
				stones[0, 6] = new Stone(Stone.Val.YoMa);
				stones[0, 7] = new Stone(Stone.Val.YoSang);
			}
			else
			{
				stones[0, 1] = new Stone(Stone.Val.YoSang);
				stones[0, 2] = new Stone(Stone.Val.YoMa);
				stones[0, 6] = new Stone(Stone.Val.YoSang);
				stones[0, 7] = new Stone(Stone.Val.YoMa);
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
			Stone[,] other = b.stones;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (stones[y, x].Equals(other[y, x]))
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
					nuBoard.stones[ny, nx] = stones[y, x].Opposite;
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
					if (stones[y, x].IsMy)
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
					if (stones[y, x].IsYo)
					{
						moves.AddRange(GetAllMoves(new Pos(x, y)));
					}
				}
			}

			moves.Add(Move.Rest);
			return moves;
		}

		public Stone this[Pos pos]
		{
			get => stones[pos.Y, pos.X];
		}

		public Stone this[int y, int x]
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

		public List<Move> GetAllMoves(Pos pos)
		{

			List<Move> moves = new List<Move>();

			int px = pos.X;
			int py = pos.Y;

			Stone stoneFrom = this[pos];
			Stone.Val valFrom = stoneFrom.Value;
			if (stoneFrom.IsEmpty)
			{
				return moves;
			}
			else if (stoneFrom.IsCha)
			{
				bool confirmAndAdd(int x, int y)
				{
					Stone stoneTo = stones[y, x];
					if (stones[y, x].IsEmpty)
					{
						moves.Add(new Move(px, py, x, y));
						return true;
					}
					else
					{
						if (!stoneTo.IsAlliesWith(stoneFrom))
						{
							moves.Add(new Move(px, py, x, y));
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
				if (px == 3 && (py == 2 || py == 9))
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
				if (px == 5 && (py == 0 || py == 7))
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
				if (px == 5 && (py == 2 || py == 9))
				{
					for (int x = px - 1, y = py - 1; x >= 3; x--, y--)
					{
						if (!confirmAndAdd(x, y))
						{
							break;
						}
					}
				}
			}
			else if (stoneFrom.IsPo)
			{
				bool dari = false;
				bool confirmAndAdd(int x, int y)
				{
					Stone stoneTo = stones[y, x];
					//다리가 없으면 다리를 발견한다.
					if (dari == false)
					{
						if (stoneTo.IsEmpty)
						{
							return true;
						}
						else if (!stoneTo.IsPo)
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
						if (stones[y, x].IsEmpty)
						{
							moves.Add(new Move(px, py, x, y));
							return true;
						}
						else
						{
							if (!stoneTo.IsAlliesWith(stoneFrom) && !stoneTo.IsPo)
							{
								moves.Add(new Move(px, py, x, y));
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
				if (px == 3 && (py == 2 || py == 9))
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
				if (px == 5 && (py == 0 || py == 7))
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
				if (px == 5 && (py == 2 || py == 9))
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
			}
			else if (stoneFrom.IsMa)
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
					if (!this[block].IsEmpty)
					{
						continue;
					}

					moves.Add(new Move(pos, nu));
				}
			}
			else if (stoneFrom.IsSang)
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
					if (!this[block1].IsEmpty)
					{
						continue;
					}

					Pos block2 = pos + wayAndBlockSang[i].Item3;
					if (!this[block2].IsEmpty)
					{
						continue;
					}

					moves.Add(new Move(pos, to));
				}
			}
			//궁/사
			else if (stoneFrom.IsGoong || stoneFrom.IsSa)
			{
				Pos origin;

				if (stoneFrom.IsMy)
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
					Stone stoneTo = this[to];
					if (stoneTo.IsEmpty || !stoneTo.IsAlliesWith(stoneFrom))
					{
						moves.Add(new Move(pos, to));
					}
				}
			}
			//졸
			else if (valFrom == Stone.Val.MyJol)
			{
				if (px - 1 >= 0 && !stoneFrom.IsAlliesWith(stones[py, px - 1]))
				{
					moves.Add(new Move(px, py, px - 1, py));
				}

				if (px + 1 < Width && !stoneFrom.IsAlliesWith(stones[py, px + 1]))
				{
					moves.Add(new Move(px, py, px + 1, py));
				}

				if (py - 1 >= 0 && !stoneFrom.IsAlliesWith(stones[py - 1, px]))
				{
					moves.Add(new Move(px, py, px, py - 1));
				}

				//우상으로 진출
				if (pos.Equals(3, 2) && !stoneFrom.IsAlliesWith(stones[1, 4]))
				{
					moves.Add(new Move(px, py, 4, 1));
				}
				else if (pos.Equals(4, 1) && !stoneFrom.IsAlliesWith(stones[0, 5]))
				{
					moves.Add(new Move(px, py, 5, 0));
				}
				//좌상으로 진출
				else if (pos.Equals(5, 2) && !stoneFrom.IsAlliesWith(stones[1, 4]))
				{
					moves.Add(new Move(px, py, 4, 1));
				}
				else if (pos.Equals(4, 1) && !stoneFrom.IsAlliesWith(stones[0, 3]))
				{
					moves.Add(new Move(px, py, 3, 0));
				}
			}
			else if (valFrom == Stone.Val.YoJol)
			{
				if (px - 1 >= 0 && !stoneFrom.IsAlliesWith(stones[py, px - 1]))
				{
					moves.Add(new Move(px, py, px - 1, py));
				}

				if (px + 1 < Width && !stoneFrom.IsAlliesWith(stones[py, px + 1]))
				{
					moves.Add(new Move(px, py, px + 1, py));
				}

				if (py + 1 < Height && !stoneFrom.IsAlliesWith(stones[py + 1, px]))
				{
					moves.Add(new Move(px, py, px, py + 1));
				}

				//우하로 진출
				if (pos.Equals(3, 7) && !stoneFrom.IsAlliesWith(stones[8, 4]))
				{
					moves.Add(new Move(px, py, 4, 8));
				}
				else if (pos.Equals(4, 8) && !stoneFrom.IsAlliesWith(stones[9, 5]))
				{
					moves.Add(new Move(px, py, 5, 9));
				}
				//좌하로 진출
				else if (pos.Equals(5, 7) && !stoneFrom.IsAlliesWith(stones[8, 4]))
				{
					moves.Add(new Move(px, py, 4, 8));
				}
				else if (pos.Equals(4, 8) && !stoneFrom.IsAlliesWith(stones[9, 3]))
				{
					moves.Add(new Move(px, py, 3, 9));
				}
			}
			else
			{
				throw new Exception("ERROR");
			}


			return moves;
		}

		public void MoveNext(Move move)
		{
			prevMove = move;
			if (!move.IsRest)
			{
				Point += this[move.To].Point;
				stones[move.To.Y, move.To.X] = stones[move.From.Y, move.From.X];
				stones[move.From.Y, move.From.X] = new Stone();
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
			return Point + this[move.To].Point;
		}

		//총 잡을 수 있는 기물
		public int CountTarget(List<Move> moves)
		{
			int sum = 0;
			foreach (var move in moves)
			{
				if (!move.IsRest)
				{
					if (!this[move.To].IsEmpty)
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
					Stone stone = this[y, x];

					if (prevMove.To.Equals(x, y))
					{
						Console.BackgroundColor = ConsoleColor.DarkYellow;
					}
					else
					{
						Console.BackgroundColor = ConsoleColor.Black;
					}
					if (stone.IsEmpty)
					{
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					else if (stone.IsMy ^ colorInverse)
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
					}
					Console.Write(letters[(int)this[y, x]]);
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
