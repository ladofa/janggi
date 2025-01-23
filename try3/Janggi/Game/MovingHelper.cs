using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Game.Board;
using static Janggi.Game.Stones;

using static System.Math;


namespace Janggi.Game
{
    public static class MovingHelper
    {

        public static List<(int[] blocks, int target)>[][] moves_look_up;
        //all_moves[기물종류][위치] -> 

        public static List<Move> idx2move;
        public static Dictionary<Move, int> move2idx;
        public static readonly int total_moving_count;

        public static bool is_valid(int y, int x) => ((y is >= 0 and < H) && (x is >= 0 and < W));

        static MovingHelper() {

            moves_look_up = new List<(int[] blocks, int target)>[256][];
            idx2move = new ();
            move2idx = new ();

            //일단 모든 종류.. 없는 것 포함 다 만들고 시작.
            for (int stone = 0; stone < 256; stone++)
            {
                moves_look_up[stone] = new List<(int[] blocks, int target)>[H *W];
                for (int y = 0; y < H; y++) 
                {
                    for (int x = 0; x < W; x++)
                    {
                        moves_look_up[stone][_(y, x)] = new List<(int[] blocks, int target)>();
                    }
                }         
            }

            void add_func(Move move)
            {
                //if (move == new Move(5, 1))
                //{
                //    Console.WriteLine("??");
                //}
                idx2move.Add(move);
            }

            //search all (int, int)sible moves
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int from = _(y, x);
                    //CHA, PO, 일단은 같은 움직임
                    for (int dx = -W + 1; dx < W; dx++)
                    {
                        if (dx == 0) continue;

                        int ty = y;
                        int tx = x + dx;
                        if (!is_valid(ty, tx)) continue;
                        int to = _(ty, tx);

                        int[] blocks = Enumerable.Range(Min(x, tx) + 1, Abs(dx) - 1)
                            .Select(bx => _(y, bx)).ToArray<int>();

                        moves_look_up[CHA][from].Add((blocks, to));
                        moves_look_up[PO][from].Add((blocks, to));

                        //졸은 한 초 구분
                        if (dx == 1 || dx == -1)
                        {
                            moves_look_up[HAN_JOL][from].Add((blocks, to));
                            moves_look_up[CHO_JOL][from].Add((blocks, to));
                        }
                        add_func(new Move(from, to));
                    }

                    for (int dy = -H + 1; dy < H; dy++)
                    {
                        if (dy == 0) continue;

                        int ty = y + dy;
                        int tx = x;
                        if (!is_valid(ty, tx)) continue;
                        int to = _(ty, tx);

                        int[] blocks = Enumerable.Range(Min(y, ty) + 1, Abs(dy) - 1)
                            .Select(by => _(by, x)).ToArray<int>();

                        moves_look_up[CHA][from].Add((blocks, to));
                        moves_look_up[PO][from].Add((blocks, to));
                        if (dy == 1)
                            moves_look_up[HAN_JOL][from].Add((blocks, to));
                        else if (dy == -1)
                            moves_look_up[CHO_JOL][from].Add((blocks, to));
                        add_func(new Move(from, to));
                    }

                    //MA
                    foreach (var tuple in wayAndBlockMa)
                    {
                        int ty = y + tuple.Item1.y;
                        int tx = x + tuple.Item1.x;
                        int by = y + tuple.Item2.y;
                        int bx = x + tuple.Item2.x;
                        if (!is_valid(ty, tx)) continue;
                        int to = _(ty, tx);
                        int[] blocks = { _(by, bx) };
                        moves_look_up[MA][from].Add((blocks, to));
                        add_func(new Move(from, to));
                    }


                    //SANG
                    foreach (var tuple in wayAndBlockSang)
                    {
                        int ty = y + tuple.Item1.y;
                        int tx = x + tuple.Item1.x;
                        int by1 = y + tuple.Item2.y;
                        int bx1 = x + tuple.Item2.x;
                        int by2 = y + tuple.Item3.y;
                        int bx2 = x + tuple.Item3.x;
                        if (!is_valid(ty, tx)) continue;
                        int to = _(ty, tx);
                        int[] blocks = { _(by1, bx1), _(by2, bx2) };
                        moves_look_up[SANG][from].Add((blocks, to));
                        add_func(new Move(from, to));
                    }
                }

            }

            //궁 안에서 1칸 움직임
            foreach (int base_y in new []{ 0, 7})
            {
                int base_x = 3;

                for (int ry = 0; ry < 3; ry++)//relative y, x
                {
                    for (int rx = 0; rx < 3; rx++)
                    {
                        int y = base_y + ry;
                        int x = base_x + rx;
                        int from = _(y, x);

                        foreach (var way in wayInGoong[ry, rx])
                        {
                            int ty = base_y + way.y;
                            int tx = base_x + way.x;
                            int to = _(ty, tx);
                            if (!is_valid(ty, tx)) throw new Exception("????");

                            moves_look_up[KING][from].Add((new int[] { } , to));
                            moves_look_up[SA][from].Add((new int[] { }, to));
                            //대각선은 CHA, JOL에 추가
                            if (Abs(ty-y) == 1 && Abs(tx-x) == 1)
                            {
                                moves_look_up[CHA][from].Add((new int[] { }, to));

                                add_func(new Move(from, to));//flat을 위한 건 대각선만 추가하면 된다.
                                if (ty - y > 0)
                                {
                                    moves_look_up[HAN_JOL][from].Add((new int[] { }, to));
                                }
                                else
                                {
                                    moves_look_up[CHO_JOL][from].Add((new int[] { }, to));
                                }
                            }
                        }
                    }
                }
            }

            List<(int from, int block, int to)> diags =new List<(int, int, int)>()
            {
                (_(0, 3), _(1, 4), _(2, 5)),
                (_(2, 3), _(1, 4), _(0, 5)),
                (_(0, 5), _(1, 4), _(2, 3)),
                (_(2, 5), _(1, 4), _(0, 3)),

            };

            //궁 안에서 2칸 움직임
            foreach ((int from, int block, int to) in diags)
            { 
                foreach (int base_y in new[] { 0, 7 * W })
                {
                    moves_look_up[CHA][from + base_y].Add((new int[] {block + base_y }, to + base_y));
                    moves_look_up[PO][from + base_y].Add((new int[] { block + base_y }, to + base_y));
                    add_func(new Move(from + base_y, to + base_y));//flat을 위한 건 대각선만 추가하면 된다.
                }
            }

            moves_look_up[CHO_SANG] = moves_look_up[SANG];
            moves_look_up[HAN_SANG] = moves_look_up[SANG];

            moves_look_up[CHO_MA] = moves_look_up[MA];
            moves_look_up[HAN_MA] = moves_look_up[MA];

            moves_look_up[CHO_PO] = moves_look_up[PO];
            moves_look_up[HAN_PO] = moves_look_up[PO];

            moves_look_up[CHO_CHA] = moves_look_up[CHA];
            moves_look_up[HAN_CHA] = moves_look_up[CHA];

            moves_look_up[CHO_SA] = moves_look_up[SA];
            moves_look_up[HAN_SA] = moves_look_up[SA];

            moves_look_up[CHO_KING] = moves_look_up[KING];
            moves_look_up[HAN_KING] = moves_look_up[KING];


            idx2move.Add(Move.Rest);

            move2idx = new Dictionary<Move, int>();
            for (int i = 0; i < idx2move.Count; i++)
            {
                move2idx[idx2move[i]] = i;
            }

            total_moving_count = idx2move.Count;
        }


        //KING, SA
        static List<(int y, int x)>[,] wayInGoong = new List<(int y, int x)>[,]
            {
                { new List<(int y, int x)>(){ (1, 0), (1, 1), (0, 1) },
                    new List<(int y, int x)>(){ (0, 0), (1, 1), (0, 2) },
                    new List<(int y, int x)>(){ (1, 2), (1, 1), (0, 1) }
                },

                { new List<(int y, int x)>(){ (0, 0), (1, 1), (2, 0) },
                    new List<(int y, int x)>(){ (0, 1), (0, 0), (1, 0), (2, 0), (2, 1), (2, 2), (1, 2), (0, 2)},
                    new List<(int y, int x)>(){ (1, 1), (0, 2), (2, 2) }
                },

                { new List<(int y, int x)>(){ (1, 0), (1, 1), (2, 1) },
                    new List<(int y, int x)>(){ (2, 0), (1, 1), (2, 2) },
                    new List<(int y, int x)>(){ (1, 2), (1, 1), (2, 1) }
                }
            };

        //SANG
        static Tuple<(int y, int x), (int y, int x), (int y, int x)>[] wayAndBlockSang = new Tuple<(int y, int x), (int y, int x), (int y, int x)>[8]
                    {
                new Tuple<(int y, int x), (int y, int x), (int y, int x)>((-3, 2), (-2, 1), (-1, 0))
                ,new Tuple<(int y, int x), (int y, int x), (int y, int x)>((-3, -2), (-2, -1), (-1, 0))
                ,new Tuple<(int y, int x), (int y, int x), (int y, int x)>((-2, -3), (-1, -2), (0, -1))
                ,new Tuple<(int y, int x), (int y, int x), (int y, int x)>((2, -3), (1, -2), (0, -1))
                ,new Tuple<(int y, int x), (int y, int x), (int y, int x)>((3, -2), (2, -1), (1, 0))
                ,new Tuple<(int y, int x), (int y, int x), (int y, int x)>((3, 2), (2, 1), (1, 0))
                ,new Tuple<(int y, int x), (int y, int x), (int y, int x)>((2, 3), (1, 2), (0, 1))
                ,new Tuple<(int y, int x), (int y, int x), (int y, int x)>((-2, 3), (-1, 2), (0, 1))
                    };

        //MA
        static Tuple<(int y, int x), (int y, int x)>[] wayAndBlockMa = {
                    new Tuple<(int y, int x), (int y, int x)>((-2, 1), (-1, 0)),
                    new Tuple<(int y, int x), (int y, int x)>((-2, -1), (-1, 0)),
                    new Tuple<(int y, int x), (int y, int x)>((-1, -2), (0, -1)),
                    new Tuple<(int y, int x), (int y, int x)>((1, -2), (0, -1)),
                    new Tuple<(int y, int x), (int y, int x)>((2, -1), (1, 0)),
                    new Tuple<(int y, int x), (int y, int x)>((2, 1), (1, 0)),
                    new Tuple<(int y, int x), (int y, int x)>((1, 2), (0, 1)),
                    new Tuple<(int y, int x), (int y, int x)>((-1, 2), (0, 1))
                };

    }
}
