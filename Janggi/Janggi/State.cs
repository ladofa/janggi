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

		public static List<Move> GetAllMoves(Stones[][] board, Pos pos)
		{
			List<Move> moves = new List<Move>();
			Stones stone = board[pos.Y][pos.X];
			if (stone == Stones.Empty)
			{
				
			}

			return moves;
		}
	}
}
