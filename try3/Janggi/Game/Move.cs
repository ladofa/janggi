using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Game.Board;

namespace Janggi.Game
{
    public record Move(int from, int to)
    {
        public static readonly Move Rest = new Move(90, 90);
        

        public (int y, int x) From
        {
            get =>(from / W, from % W);
        }

        public (int y, int x) To
        {
            get => (to / W, to % W);
        }
        //public (int x, int y, int dx, int dy) Relative
        //{
        //    {
        //        int dx = to.x - from.x;
        //        int dy = to.y - from.y;
        //        return (from.x, from.y, dx, dy);
        //    }
        //}

        //public Move((int, int) from,  (int, int) to)
        //{
        //    this.from = from;
        //    this.to = to;
        //}

        public Move(int fy, int fx, int ty, int tx) : this(_(fy, fx), _(ty, tx))
        {

        }

        public Move((int, int) from, (int, int) to):this(_(from), _(to))
        {

        }


        //public Move((int x, int y, int dx, int dy) relative)
        //{
        //    from = (relative.x, relative.y);
        //    to = (relative.dx, relative.dy);
        //}

        public bool IsEmpty
        {
            get => from == 90;
        }

        public Move FlipXY()
        {
            //이 flip 공식은 잘 풀어보면 나옴....
            //원래 인덱스 i = y * W + x
            //플립된 인덱스 i' = (H - y - 1) * W + (W - x - 1)
            // = HW - yW - W + W - x - 1
            //                          여기서 y = i / W, x = i % w
            // = HW - (i/W)W - (i%W) - 1 (-W+W 삭제)
            //                          여기서 (i/W)W + (i%W) == i 이다!
            // = HW - i - 1

            return new (H*W-from-1, H*W-to - 1);
        }
    }
}
