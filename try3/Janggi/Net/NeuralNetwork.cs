using Janggi.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Janggi.Game.Board;

namespace Janggi.Net
{
    public static class NeuralNetwork
    {
        public static float[] Flatten(Board board)
        {
            float[] data = new float[16 * (H * W)];  //16 by 90으로 생각한다.
            byte[] stones = board.stones;
            
            for (int i = 0; i < H * W; i++)
            {
                int ch = stones[i] & Stones._HAN; //EMPTY = 0, HAN기물은 1~7, CHO기물은 9~15, 8번 채널은 그냥 더미
                data[ch * (H * W) + i] = stones[i];
            }

            return data;
        }

        public static int Flatten(Move move)
        {
            return MovingHelper.move2idx[move];
        }
    }
}
