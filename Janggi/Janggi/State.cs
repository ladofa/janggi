using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi
{
	public static class State
	{
		public static int VectorLength = 15;

		public static int BoardWidth = 9;

		public static int BoardHeight = 10;

		public static Stones[][] GetEmptyBoard()
		{
			Stones[][] board = new Stones[BoardHeight][];
			for (int i = 0; i < BoardHeight; i++)
			{
				board[i] = new Stones[BoardWidth];
			}

			return board;
		}

		public static float[] Board2Vector(Stones[][] board)
		{
			throw new NotImplementedException();
		}

		public static Stones[][] Vector2Board(float[] vector)
		{
			throw new NotImplementedException();
		}

		public static List<Move> GetAllMoves(Stones[][] board)
		{
			List<Move> moves = new List<Move>();
			for (int y = 0; y < BoardHeight; y++)
			{
				for (int x = 0; x < BoardWidth; x++)
				{
					Stones stone = board[y][x];
					//졸이라면
					if (stone == Stones.MyJol)
					{
						//왼쪽
					}

					//차라면
					if (stone == Stones.MyCha)
					{
						//위로
						//아래로
						//좌로
						//우로
					}
				}
			}
		}

		public static bool IsMy(Stones stone)
		{
			return ((byte)stone & 128) > 0;
		}

		public static bool IsYo(Stones stone)
		{
			return ((byte)stone & 64) > 0;
		}

		public static bool AreAllies(Stones stone1, Stones stone2)
		{
			if (stone1 == Stones.Empty || stone2 == Stones.Empty)
			{
				return false;
			}

			return ((byte)stone1 & 64) == ((byte)stone2 & 64);
		}

		public static Stones GetStone(Stones[][] board, Pos pos)
		{
			return board[pos.Y][pos.X];
		}

		public static List<Move> GetAllMoves(Stones[][] board, Pos pos)
		{
			List<Move> moves = new List<Move>();
			Stones stone = GetStone(board, pos);
			int px = pos.X;
			int py = pos.Y;


			//비어있으면, 비어있지 않고 적이면 추가하고
			//아군 멱이면 그만
			bool confirmAndAdd(int x, int y)
			{
				if (board[y][x] == Stones.Empty)
				{
					moves.Add(new Move(px, py, x, y));
					return true;
				}
				else
				{
					if (!AreAllies(board[y][x], stone))
					{
						moves.Add(new Move(px, py, x, y));
					}
					return false;
				}
			}

			if (stone == Stones.Empty)
			{

			}
			else if (stone == Stones.MyCha || stone == Stones.YoCha)
			{
				for (int y = py - 1; y >= 0; y--)
				{
					if (!confirmAndAdd(px, y))
					{
						break;
					}
				}

				for (int y = py + 1; y < BoardHeight; y++)
				{
					if (!confirmAndAdd(px, y))
					{
						break;
					}
				}

				for (int x = px - 1; px >= 0; px--)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}

				for (int x = px + 1; px < BoardWidth; x++)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}
			}
			else if (stone == Stones.MyPo || stone == Stones.YoPo)
			{
				bool 
				for (int y = py - 1; y >= 0; y--)
				{
					if (!confirmAndAdd(px, y))
					{
						break;
					}
				}
			}

			return moves;
		}


		
		
	}
}
