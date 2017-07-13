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
		public uint[,] stones;
		public uint[,] targets;
		public uint[,] blocks;

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


			for (int i = 0; i < 32; i++)
			{
				uint stone = (uint)1 << i;
				for (int y = 0; y < Height; y++)
				{
					for (int x = 0; x < Width; x++)
					{
						if (stones[y, x] == stone)
						{
							positions[i + 1] = new Pos(x, y);
						}
					}
				}
			}


			for (int i = 0; i < 32; i++)
			{
				uint stone = (uint)1 << i;

				Pos p = GetPos(i + 1);
				setTargets(p);
			}
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
					if (IsMine(targets[y, x]) && !IsMine(stones[y, x]))
					{
						for (int i = 0; i < 16; i++)
						{
							uint stone = (uint)1 << i;
							if ((targets[y, x] & stone) > 0)
							{
								moves.Add(new Move(GetPos(i + 1), new Pos(x, y)));
							}
						}
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
					if (IsYours(targets[y, x]) && !IsYours(stones[y, x]))
					{
						for (int i = 0; i < 16; i++)
						{
							uint stone = (uint)0x0001_0000 << i;
							if ((targets[y, x] & stone) > 0)
							{
								moves.Add(new Move(GetPos(i + 17), new Pos(x, y)));
							}
						}
					}
				}
			}

			moves.Add(Move.Rest);
			return moves;
		}

		public ref uint this[Pos pos]
		{
			get =>  ref stones[pos.Y, pos.X];
		}

		public ref uint this[int y, int x]
		{
			get => ref stones[y, x];
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

		private void setTargets(Pos pos)
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
					targets[y, x] |= stoneFrom;
					if (stones[y, x] == 0)
					{
						return true;
					}
					else
					{
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
							if (IsPo(stones[y, x]))
							{
								blocks[y, x] |= stoneFrom;
							}
							else
							{
								targets[y, x] |= stoneFrom;
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
					blocks[block.Y, block.X] |= stoneFrom;
					if (this[block] != 0)
					{
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

					Pos block2 = pos + wayAndBlockSang[i].Item3;
					blocks[block2.Y, block2.X] |= stoneFrom;
					if (this[block2] != 0)
					{
						continue;
					}

					Pos block1 = pos + wayAndBlockSang[i].Item2;
					blocks[block1.Y, block1.X] |= stoneFrom;
					
					if (this[block1] != 0)
					{
						continue;
					}

					targets[to.Y, to.X] |= stoneFrom;
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
					
					targets[to.Y, to.X] |= stoneFrom;
				}
			}
			//졸
			else if (IsJol(stoneFrom) && IsMine(stoneFrom))
			{
				//TODO : else를 만들어야 하는데...
				if (px - 1 >= 0)
				{
					targets[py, px - 1] |= stoneFrom;
				}

				if (px + 1 < Width)
				{
					
					targets[py, px + 1] |= stoneFrom;
				}

				if (py - 1 >= 0)
				{					
					targets[py - 1, px] |= stoneFrom;
				}

				//우상으로 진출
				if (pos.Equals(3, 2) )
				{
					targets[1, 4] |= stoneFrom;
				}
				else if (pos.Equals(4, 1))
				{
					targets[0, 5] |= stoneFrom;
				}
				//좌상으로 진출
				else if (pos.Equals(5, 2))
				{
					targets[1, 4] |= stoneFrom;
				}
				else if (pos.Equals(4, 1))
				{
					targets[0, 3] |= stoneFrom;
				}
			}
			else if (IsJol(stoneFrom) && IsYours(stoneFrom))
			{
				if (px - 1 >= 0)
				{
					targets[py , px - 1] |= stoneFrom;
				}

				if (px + 1 < Width)
				{
					targets[py, px + 1] |= stoneFrom;
				}

				if (py + 1 < Height)
				{
					targets[py + 1, px] |= stoneFrom;
				}

				//우하로 진출
				if (pos.Equals(3, 7))
				{
					targets[8, 4] |= stoneFrom;
				}
				else if (pos.Equals(4, 8))
				{
					targets[9, 5] |= stoneFrom;
				}
				//좌하로 진출
				else if (pos.Equals(5, 7))
				{
					targets[8, 4] |= stoneFrom;
				}
				else if (pos.Equals(4, 8))
				{
					targets[9, 3] |= stoneFrom;
				}
			}
			else
			{
				throw new Exception("ERROR");
			}
		}

		private void removeTargets(uint stone)
		{
			uint _stone = ~stone;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					targets[y, x] &= _stone;
					blocks[y, x] &= _stone;
				}
			}
		}

		private void recalcTargets(Pos pos)
		{
			uint target = targets[pos.Y, pos.X];
			uint block = blocks[pos.Y, pos.X];
			
			
			for (int i = 0; i < 32; i++)
			{
				uint stone = (uint)1 << i;
				if ((target & stone) > 0 || (block & stone) > 0)
				{
					removeTargets(stone);
					Pos from = GetPos(i + 1);
					if (from.X != -1)
					{
						setTargets(from);
					}
				}
			}
		}

		public void MoveNext(Move move)
		{
			prevMove = move;
			if (!move.IsRest)
			{
				uint stone = this[move.From];
				//도착 위치에 물체가 있으면
				uint stoneTo = this[move.To];
				if (stoneTo != 0)
				{
					//타겟을 지워준다.
					removeTargets(stoneTo);
					Point += GetPoint(stoneTo);
					positions[Stone2Index(stoneTo)] = new Pos(-1, -1);
				}

				//기물을 제거
				this[move.From] = 0;
				//움직이려는 기물에 대한 target, block을 지워줌
				removeTargets(stone);
				//도착 위치에 세워놓고,
				this[move.To] = stone;

				//기물이 있던 자리에 대해서 계산을 다시 해줌
				recalcTargets(move.From);
				//도착 위치에 대해서도 계산을 다시 해줌
				recalcTargets(move.To);
				setTargets(move.To);

				positions[Stone2Index(stone)] = move.To;
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

		public bool IsFinished
		{
			get => IsMyWin || IsYoWin;
		}

		#region 기물의 위치를 찾는 것 관련

		//원래는 positions를 구현해야 하지만 지금은 그냥 놔둠

		public Pos GetPos(uint stone)
		{
			return positions[Stone2Index(stone)];
		}

		public Pos GetPos(int index)
		{
			return positions[index];
		}

		private void movePos(Move move, uint stone)
		{
			//아무것도 안 함 일단은.
		}

		#endregion




		#region 정책망 관련

		public int ExpectedPoint(Move move)
		{
			return Point + GetPoint(this[move.To]);
		}

		public int Judge()
		{
			//내 점수를 더한다.
			int p1 = Point;

			//잡을 수 있는 점수
			int p2 = CountTake();

			//잡힐 점수
			int p3 = CountTaken();

			//잡을 수 있는 가장 비싼 것
			//int p4 = MaxTake();

			//잡힐 수 있는 가장 비싼 것
			//int p5 = MinTaken();

			int p6 = 0;

			const int addPo = 3;
			//포 타깃이 있는지 살펴봄
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					uint target = targets[y, x];
					if ((target & (uint)Stones.MyPo1) > 0)
					{
						p6 += addPo;
						break;
					}
				}
			}

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					uint target = targets[y, x];
					if ((target & (uint)Stones.MyPo2) > 0)
					{
						p6 += addPo;
						break;
					}
				}
			}

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					uint target = targets[y, x];
					if ((target & (uint)Stones.YoPo1) > 0)
					{
						p6 -= addPo;
						break;
					}
				}
			}

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					uint target = targets[y, x];
					if ((target & (uint)Stones.YoPo2) > 0)
					{
						p6 -= addPo;
						break;
					}
				}
			}

			//차길은 무조건 +2
			int p7 = 0;

			const int addCha = 1;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					uint target = targets[y, x];
					if (IsMyCha(target))
					{
						p7 += addCha;
					}
					else if (IsYoCha(target))
					{
						p7 -= addCha;
					}
				}
			}

			//졸끼리 붙어있으면 +2
			int p8 = 0;
			const int addJol = 2;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width - 1; x++)
				{
					uint target = targets[y, x];
					if (IsMyJol(target))
					{
						if (IsMyJol(targets[y, x + 1]))
						{
							p8 += addJol;
						}
					}
					if (IsYoJol(target))
					{
						if (IsYoJol(targets[y, x + 1]))
						{
							p8 -= addJol;
						}
					}
				}
			}
			
			if (IsMyTurn)
			{
				p2 /= 3;
				p3 /= 5;
			}
			else
			{
				p2 /= 5;
				p3 /= 3;
			}

			return p1 + p2 + p3 + p6 + p7;
		}

		//총 잡을 수 있는 기물
		public int CountTake()
		{
			int sum = 0;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (IsYours(stones[y, x]))
					{
						if (IsMine(targets[y, x]))
						{
							if (IsKing(stones[y, x]))
							{
								sum += 200;
							}
							else
							{
								sum += GetPoint(stones[y, x]);
							}
							
						}
					}
				}
			}
			return sum;
		}

		//잡을 수 있는 기물 중 가장 비싼 것
		public int MaxTake()
		{
			int max = 0;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (IsYours(stones[y, x]))
					{
						if (targets[y, x] > 0)
						{
							if (IsKing(stones[y, x]))
							{
								return 200;
								
							}
							else
							{
								int p =  GetPoint(stones[y, x]);
								if (p > max)
								{
									max = p;
								}
							}

						}
					}
				}
			}
			return max;
		}

		//총 잡힐 기물
		public int CountTaken()
		{
			int sum = 0;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (IsMine(stones[y, x]))
					{
						if (IsYours(targets[y, x]))
						{
							if (IsKing(stones[y, x]))
							{
								sum -= 200;
							}
							else
							{
								sum += GetPoint(stones[y, x]);
							}
						}
					}
				}
			}
			return sum;
		}

		//잡힐 수 있는 기물 중 가장 비싼 것
		public int MinTaken()
		{
			//마이너스라서..
			int min = 0;
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (IsMine(stones[y, x]))
					{
						if (targets[y, x] > 0)
						{
							if (IsKing(stones[y, x]))
							{
								return -200;
							}
							else
							{
								int p = GetPoint(stones[y, x]);
								if (p < min)
								{
									min = p;
								}
							}
						}
					}
				}
			}
			return min;
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

					if (prevMove.To.Equals(x, y) || prevMove.From.Equals(x, y))
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
