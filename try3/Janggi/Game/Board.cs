using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Game.Stones;
using static Janggi.Game.MovingHelper;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Serialization;



namespace Janggi.Game
{


    public class Board
    {
        public static (int y, int x) _(int p) => (p / W, p % W);

        public static int _((int y, int x) p) => p.y * W + p.x;
        public static int _(int y, int x) => y * W + x;

        public const int W = 9;
        public const int H = 10;

        public byte[] stones;
        public byte this[int key] => stones[key];
        public float dum = 1.5f;
        public float score = 0f;

        public byte player = CHO;

        public Move prevMove = Move.Rest;
        Board? prevBoard = null;

        public int winner = 0;
        public List<Move> moves = null!;
        public List<Move> Moves
        {
            get
            {
                if (moves == null)
                {
                    CalcMoves();
                }
                return moves;
            }
        }
        public int turn = 0;

        public Board(byte[] stones)
        {
            this.stones = stones;

            score = 0;
            for (int p = 0; p < H * W; p++)
            {
                score += scores[stones[p]];
            }

            score = score + dum;
        }

        //public Board(Board src)
        //{
        //    stones = new byte[H * W];
        //    prevBoard = src;
        //    dum = src.dum;
        //    moves = new List<Move>();

        //    src.stones.CopyTo(stones, 0);
        //    score = src.score;
        //}

        public Board(Board src, Move move = null!)
        {
            stones = new byte[H * W];
            prevBoard = src;
            prevMove = move;
            dum = src.dum;

            src.stones.CopyTo(stones, 0);

            //기물 이동 및 점수 계산
            if (move != null && move != Move.Rest)
            {
                byte delta_stone = stones[move.to]; //없어진 기물만큼 점수 빠진다.
                stones[move.to] = stones[move.from];
                stones[move.from] = EMPTY;

                //TODO : 빅장의 경우???
                if (delta_stone == HAN_KING)
                {
                    winner = 1;
                }
                else if (delta_stone == CHO_KING)
                {
                    winner = -1;
                }

                score += scores[delta_stone];
            }
            score += dum;

            player = (byte)(src.player ^ SIDE);//toggle
            turn = src.turn + 1;
        }

        public Board CreateTranspose()
        {
            Board nu = new Board(this);
            nu.dum = -dum;
            nu.player = (byte)(player ^ SIDE);//toggle
            nu.winner = -winner;
            nu.score = -score;
            //prevmove는 어떻게 하지?

            //moves를 다 filp하느니 그냥 null 처리.. 일단
            moves = null!;

            for (int p = 0; p < H * W; p++)
            {
                //플립공식에 의해 .. (move.cs참조)
                nu.stones[H * W - p - 1] = stones[p];
            }

            return nu;
        }

        public static Board RandomBoard()
        {
            Startings p = (Startings)(new Random().Next(4));
            Startings n = (Startings)(new Random().Next(4));
            return new Board(p, n);
        }

        public Board(Startings pStarting, Startings nStarting)
        {
            stones = new byte[H * W];
            stones[_(0, 0)] = HAN_CHA;
            //stones[yx(0, 1)] = HAN_CHA;
            //stones[yx(0, 2)] = HAN_CHA;
            stones[_(0, 3)] = HAN_SA;
            stones[_(0, 5)] = HAN_SA;
            //stones[yx(0, 6)] = HAN_CHA;
            //stones[yx(0, 7)] = HAN_CHA;
            stones[_(0, 8)] = HAN_CHA;
            stones[_(1, 4)] = HAN_KING;
            stones[_(2, 1)] = HAN_PO;
            stones[_(2, 7)] = HAN_PO;
            stones[_(3, 0)] = HAN_JOL;
            stones[_(3, 2)] = HAN_JOL;
            stones[_(3, 4)] = HAN_JOL;
            stones[_(3, 6)] = HAN_JOL;
            stones[_(3, 8)] = HAN_JOL;

            stones[_(9, 0)] = CHO_CHA;
            stones[_(9, 3)] = CHO_SA;
            stones[_(9, 5)] = CHO_SA;
            stones[_(9, 8)] = CHO_CHA;
            stones[_(8, 4)] = CHO_KING;
            stones[_(7, 1)] = CHO_PO;
            stones[_(7, 7)] = CHO_PO;
            stones[_(6, 0)] = CHO_JOL;
            stones[_(6, 2)] = CHO_JOL;
            stones[_(6, 4)] = CHO_JOL;
            stones[_(6, 6)] = CHO_JOL;
            stones[_(6, 8)] = CHO_JOL;

            if (pStarting == Startings.MSSM)
            {
                stones[_(9, 1)] = CHO_MA;
                stones[_(9, 2)] = CHO_SANG;
                stones[_(9, 6)] = CHO_SANG;
                stones[_(9, 7)] = CHO_MA;
            }
            else if (pStarting == Startings.SMMS)
            {
                stones[_(9, 1)] = CHO_SANG;
                stones[_(9, 2)] = CHO_MA;
                stones[_(9, 6)] = CHO_MA;
                stones[_(9, 7)] = CHO_SANG;
            }
            else if (pStarting == Startings.MSMS)
            {
                stones[_(9, 1)] = CHO_MA;
                stones[_(9, 2)] = CHO_SANG;
                stones[_(9, 6)] = CHO_MA;
                stones[_(9, 7)] = CHO_SANG;
            }
            else if (pStarting == Startings.SMSM)
            {
                stones[_(9, 1)] = CHO_SANG;
                stones[_(9, 2)] = CHO_MA;
                stones[_(9, 6)] = CHO_SANG;
                stones[_(9, 7)] = CHO_MA;
            }

            if (nStarting == Startings.MSSM)
            {
                stones[_(0, 1)] = HAN_MA;
                stones[_(0, 2)] = HAN_SANG;
                stones[_(0, 6)] = HAN_SANG;
                stones[_(0, 7)] = HAN_MA;
            }
            else if (nStarting == Startings.SMMS)
            {
                stones[_(0, 1)] = HAN_SANG;
                stones[_(0, 2)] = HAN_MA;
                stones[_(0, 6)] = HAN_MA;
                stones[_(0, 7)] = HAN_SANG;
            }
            else if (nStarting == Startings.MSMS) //오른상차림
            {
                stones[_(0, 1)] = HAN_SANG;
                stones[_(0, 2)] = HAN_MA;
                stones[_(0, 6)] = HAN_SANG;
                stones[_(0, 7)] = HAN_MA;
            }
            else if (nStarting == Startings.SMSM) //왼상차림
            {
                stones[_(0, 1)] = HAN_MA;
                stones[_(0, 2)] = HAN_SANG;
                stones[_(0, 6)] = HAN_MA;
                stones[_(0, 7)] = HAN_SANG;
            }

            prevMove = Move.Rest;

            score = 0;
            for (int p = 0; p < H * W; p++)
            {
                score += scores[stones[p]];
            }

            score = score + dum;
        }



        /// <summary>
        /// 가능한 움직임을 계산 (한수 쉼 제외)
        /// </summary>
        /// <returns></returns>
        /// 


        public void CalcMoves()
        {
            //TODO : 차 포 졸 계산 더 빠르게
            moves = new List<Move>();

            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int from = _(y, x);
                    byte stone = stones[from];
                    if (stone == EMPTY) continue;
                    else if (!IS_SIDE(stone, player))
                    {
                        continue;
                    }


                    if (IS_KIND(stone, PO))
                    {
                        foreach ((int[] blocks, int to) in moves_look_up[stone][from])
                        {
                            bool pass = false;
                            int count = 0;
                            foreach (int b in blocks)
                            {
                                byte bs = stones[b];
                                if (IS_KIND(bs, PO))
                                {
                                    pass = true; break;
                                }
                                else if (bs != EMPTY)
                                {
                                    count++;
                                }
                            }

                            if (pass) continue;
                            if (count != 1) continue;

                            moves.Append(new Move(from, to));
                        }
                    }
                    else
                    {
                        foreach ((int[] blocks, int to) in moves_look_up[stone][from])
                        {
                            if (!blocks.All(b => stones[b] == EMPTY)) continue;
                            if (IS_SIDE(stones[to], player)) continue;
                            moves.Add(new Move(from, to));
                        }
                    }
                }
            }

            moves.Append(Move.Rest);

            //반복수 금지 p는 내차례 q는 상대차례
            // y-> q3->x
            // x-> p2->w
            // w-> q2->v
            // v-> p1->u
            // u-> q1->t
            // t-> p0->s

            //한 수 쉼을 서로 하는 경우
            //po == empty , q1 == empty
            //같은 상태에서 같은 행마를 두 번 하는 경우



            return;
        }



        static Board()
        {
            scores = new float[256];
            scores[EMPTY] = 0;
            scores[CHO_JOL] = 2;
            scores[CHO_SANG] = 3;
            scores[CHO_MA] = 5;
            scores[CHO_PO] = 7;
            scores[CHO_CHA] = 13;
            scores[CHO_SA] = 3;
            scores[CHO_KING] = 100;

            scores[HAN_JOL] = -2;
            scores[HAN_SANG] = -3;
            scores[HAN_MA] = -5;
            scores[HAN_PO] = -7;
            scores[HAN_CHA] = -13;
            scores[HAN_SA] = -3;
            scores[HAN_KING] = -100;

        }


        static float[] scores;


        #region 텍스트 출력

        static Dictionary<byte, string> letters = new Dictionary<byte, string>() {
            {EMPTY,  "＋" },
            { CHO_JOL, "졸" },
            {CHO_SANG,"상"},
            {CHO_MA,        "마"},
            {CHO_PO,        "포"},
            {CHO_CHA,        "차"},
            {CHO_SA,        "사"},
            {CHO_KING,        "초" },
        {HAN_JOL, "兵"},
        {HAN_SANG, "象"},
        {HAN_MA, "馬"},
        {HAN_PO, "包"},
        {HAN_CHA, "車"},
        {HAN_SA, "士"},
        {HAN_KING, "漢" },
        };

        public static string GetLetter(byte stone)
        {
            return letters[stone];
        }


        public string ToStringStones()
        {
            string result = "";
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    result += (letters[stones[_(y, x)]] + " ");
                }
                result += '\n';
            }

            return result;
        }

        public void PrintStones()
        {
            string result = "";
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    byte stone = stones[_(y, x)];

                    if (prevMove.To == (y, x) || prevMove.From == (y, x))
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
                    else if (IS_SIDE(stone, CHO))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }

                    Console.Write( letters[stone] );

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        public override string ToString()
        {
            string result = "";
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    byte stone = stones[_(y, x)];

                    result += letters[stone];
                    result += " ";
                }
                result += "||";
            }

            return result;
        }



        #endregion

        public List<Move> GetAllMovesFrom((int y, int x) pos)
        {
            List<Move> res = new List<Move>();
            foreach (Move move in Moves)
            {
                if (move.From == pos)
                {
                    res.Add(move);
                }
            }

            return res;
        }
    }


}
