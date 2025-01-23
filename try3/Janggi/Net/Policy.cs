using Janggi.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janggi.Net
{
    public static class Policy
    {
        static Dictionary<Move, int> move2idx;
        static List<Move> idx2move;


        static Policy()
        {
            //move2idx = new Dictionary<Move, int>();
            idx2move = new List<Move>();

            void write(int x1, int y1, int x2, int y2)
            {
                Move move = new Move(x1, y1, x2, y2);
                idx2move.Add(move);
                //writer.WriteLine($"moveSet.Add(new Move({x1}, {y1}, {x2}, {y2}));");
                var p1 = y1 * 9 + x1;
                var p2 = y2 * 9 + x2;
#if DEBUG
                if (x1 is < 0 or >= 9 || y1 is < 0 or >= 10)
                {
                    throw new Exception("???");
                }

                if (x2 is < 0 or >= 9 || y2 is < 0 or >= 10)
                {
                    throw new Exception("???");
                }
#endif
            }

            int[,] maWays = new int[,]{
                {1, 2 }, {1, -2 }, {-1, 2 }, {-1, -2 }, {2, 1 }, {2, -1 }, {-2, 1 }, {-2, -1 }
            };

            int[,] sangWays = new int[,]{
                {3, 2 }, {3, -2 }, {-3, 2 }, {-3, -2 }, {2, 3 }, {2, -3 }, {-2, 3 }, {-2, -3 }
            };

            const int width = 9;
            const int height = 10;
            for (int y1 = 0; y1 < height; y1++)
            {
                for (int x1 = 0; x1 < width; x1++)
                {
                    //-- 차길
                    for (int x2 = 0; x2 < width; x2++)
                    {
                        int y2 = y1;
                        if (x2 == x1) continue;
                        write(x1, y1, x2, y2);
                    }

                    for (int y2 = 0; y2 < height; y2++)
                    {
                        int x2 = x1;
                        if (y2 == y1) continue;
                        write(x1, y1, x2, y2);
                    }

                    //-- 마길
                    //-- 상길
                    for (int i = 0; i < 8; i++)
                    {
                        int x2 = x1 + maWays[i, 0];
                        int y2 = y1 + maWays[i, 1];
                        if (x2 >= 0 && y2 >= 0 && x2 < width && y2 < height)
                        {
                            write(x1, y1, x2, y2);
                        }

                        x2 = x1 + sangWays[i, 0];
                        y2 = y1 + sangWays[i, 1];

                        if (x2 >= 0 && y2 >= 0 && x2 < width && y2 < height)
                        {
                            write(x1, y1, x2, y2);
                        }
                    }

                    //-- 궁길
                    if (x1 == 3 && y1 == 0)
                    {
                        write(x1, y1, x1 + 1, y1 + 1);
                        write(x1, y1, x1 + 2, y1 + 2);
                    }
                    else if (x1 == 3 && y1 == 2)
                    {
                        write(x1, y1, x1 + 1, y1 - 1);
                        write(x1, y1, x1 + 2, y1 - 2);
                    }
                    else if (x1 == 5 && y1 == 0)
                    {
                        write(x1, y1, x1 - 1, y1 + 1);
                        write(x1, y1, x1 - 2, y1 + 2);
                    }
                    else if (x1 == 5 && y1 == 2)
                    {
                        write(x1, y1, x1 - 1, y1 - 1);
                        write(x1, y1, x1 - 2, y1 - 2);
                    }
                    else if (x1 == 4 && y1 == 1)
                    {
                        write(x1, y1, x1 - 1, y1 - 1);
                        write(x1, y1, x1 - 1, y1 + 1);
                        write(x1, y1, x1 + 1, y1 - 1);
                        write(x1, y1, x1 + 1, y1 + 1);
                    }
                    else if (x1 == 3 && y1 == 7)
                    {
                        write(x1, y1, x1 + 1, y1 + 1);
                        write(x1, y1, x1 + 2, y1 + 2);
                    }
                    else if (x1 == 3 && y1 == 9)
                    {
                        write(x1, y1, x1 + 1, y1 - 1);
                        write(x1, y1, x1 + 2, y1 - 2);
                    }
                    else if (x1 == 5 && y1 == 7)
                    {
                        write(x1, y1, x1 - 1, y1 + 1);
                        write(x1, y1, x1 - 2, y1 + 2);
                    }
                    else if (x1 == 5 && y1 == 9)
                    {
                        write(x1, y1, x1 - 1, y1 - 1);
                        write(x1, y1, x1 - 2, y1 - 2);
                    }
                    else if (x1 == 4 && y1 == 8)
                    {
                        write(x1, y1, x1 - 1, y1 - 1);
                        write(x1, y1, x1 - 1, y1 + 1);
                        write(x1, y1, x1 + 1, y1 - 1);
                        write(x1, y1, x1 + 1, y1 + 1);
                    }
                }
            }

            //맨 마지막으로 제자리 움직임.
            idx2move.Add(Move.Rest);

            move2idx = idx2move.Select((element, index) => new { element, index })
                .ToDictionary(x => x.element, x => x.index);
        }
    }
}
