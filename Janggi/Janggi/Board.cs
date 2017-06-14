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

		public static int Width = 9;
		public static int Height = 10;

		public Board()
		{
			SetUp();
		}

		public enum Tables
		{
			Inner,
			Outer,
			Left,
			Right
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
			}
		}

		public void SetUp(Tables myTable, Tables yoTable)
		{
			SetUp();
			stones[0][0] = new Stone(Stone.Val.YoCha);
			stones[0][3] = new Stone(Stone.Val.YoSa);
			stones[0][5] = new Stone(Stone.Val.YoSa);
			stones[0][8] = new Stone(Stone.Val.YoCha);
			stones[1][4] = new Stone(Stone.Val.YoGoong);
			stones[2][1] = new Stone(Stone.Val.YoPo);
			stones[2][7] = new Stone(Stone.Val.YoPo);
			stones[3][0] = new Stone(Stone.Val.YoJol);
			stones[3][2] = new Stone(Stone.Val.YoJol);
			stones[3][4] = new Stone(Stone.Val.YoJol);
			stones[3][6] = new Stone(Stone.Val.YoJol);
			stones[3][8] = new Stone(Stone.Val.YoJol);

			stones[6][0] = new Stone(Stone.Val.MyJol);
			stones[6][2] = new Stone(Stone.Val.MyJol);
			stones[6][4] = new Stone(Stone.Val.MyJol);
			stones[6][6] = new Stone(Stone.Val.MyJol);
			stones[6][8] = new Stone(Stone.Val.MyJol);
			stones[7][1] = new Stone(Stone.Val.MyPo);
			stones[7][7] = new Stone(Stone.Val.MyPo);
			stones[8][4] = new Stone(Stone.Val.MyGoong);
			stones[9][0] = new Stone(Stone.Val.MyCha);
			stones[9][3] = new Stone(Stone.Val.MySa);
			stones[9][5] = new Stone(Stone.Val.MySa);
			stones[9][8] = new Stone(Stone.Val.MyCha);

			if (myTable == Tables.Inner)
			{
				stones[9][1] = new Stone(Stone.Val.MyMa);
				stones[9][2] = new Stone(Stone.Val.MySang);
				stones[9][6] = new Stone(Stone.Val.MySang);
				stones[9][7] = new Stone(Stone.Val.MyMa);
			}
			else if (myTable == Tables.Outer)
			{
				stones[9][1] = new Stone(Stone.Val.MySang);
				stones[9][2] = new Stone(Stone.Val.MyMa);
				stones[9][6] = new Stone(Stone.Val.MyMa);
				stones[9][7] = new Stone(Stone.Val.MySang);
			}
			else if (myTable == Tables.Left)
			{
				stones[9][1] = new Stone(Stone.Val.MySang);
				stones[9][2] = new Stone(Stone.Val.MyMa);
				stones[9][6] = new Stone(Stone.Val.MySang);
				stones[9][7] = new Stone(Stone.Val.MyMa);
			}
			else
			{
				stones[9][1] = new Stone(Stone.Val.MyMa);
				stones[9][2] = new Stone(Stone.Val.MySang);
				stones[9][6] = new Stone(Stone.Val.MyMa);
				stones[9][7] = new Stone(Stone.Val.MySang);
			}

			if (yoTable == Tables.Inner)
			{
				stones[0][1] = new Stone(Stone.Val.YoMa);
				stones[0][2] = new Stone(Stone.Val.YoSang);
				stones[0][6] = new Stone(Stone.Val.YoSang);
				stones[0][7] = new Stone(Stone.Val.YoMa);
			}
			else if (yoTable == Tables.Outer)
			{
				stones[0][1] = new Stone(Stone.Val.YoSang);
				stones[0][2] = new Stone(Stone.Val.YoMa);
				stones[0][6] = new Stone(Stone.Val.YoMa);
				stones[0][7] = new Stone(Stone.Val.YoSang);
			}
			else if (yoTable == Tables.Left)
			{
				stones[0][1] = new Stone(Stone.Val.YoMa);
				stones[0][2] = new Stone(Stone.Val.YoSang);
				stones[0][6] = new Stone(Stone.Val.YoMa);
				stones[0][7] = new Stone(Stone.Val.YoSang);
			}
			else
			{
				stones[0][1] = new Stone(Stone.Val.YoSang);
				stones[0][2] = new Stone(Stone.Val.YoMa);
				stones[0][6] = new Stone(Stone.Val.YoSang);
				stones[0][7] = new Stone(Stone.Val.YoMa);
			}
		}

		//상대방 입장에서 보도록 회전시킨다.
		public void GetOpposite()
		{
			//이전 포석을 보관하고
			Stone[,] oldStones = stones;
			//새 포석을 만든다.
			SetUp();

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					//회전된 새로운 위치
					int nx = Width - x - 1;
					int ny = Height - y - 1;

					//편을 바꿔서 넣는다.
					stones[ny, nx] = stones[y, x].Opposite;
				}
			}
		}

		public List<Move> GetAllMyMoves()
		{
			List<Move> moves = new List<Move>();

			return moves;
		}

		public Stone this[Pos pos]
		{
			get => stones[pos.Y, pos.X];
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



		static List<Pos>[,] wayInSung = new List<Pos>[,]
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

				for (int x = px - 1; px >= 0; px--)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}

				for (int x = px + 1; px < Width; x++)
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
						else if (stoneTo.IsAlliesWith(stoneFrom))
						{
							dari = true;
							return true;
						}
						else
						{
							return false;
						}
					}
					//다리를 발견한 뒤로는 차와 같다.
					else
					{
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
				for (int x = px - 1; px >= 0; px--)
				{
					if (!confirmAndAdd(x, py))
					{
						break;
					}
				}

				dari = false;
				for (int x = px + 1; px < Width; x++)
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

					Pos block2 = pos + wayAndBlockSang[i].Item2;
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
				foreach (var e in wayInSung[relPos.Y, relPos.X])
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

			}


			return moves;
		}
	}
}
