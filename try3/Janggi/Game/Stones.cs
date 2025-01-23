using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Janggi.Game
{
    public static class Stones
    {
        public const byte EMPTY = 0;

        public const byte JOL = 0x01;
        public const byte SANG = 0x02;
        public const byte MA = 0x03;
        public const byte PO = 0x04;
        public const byte CHA = 0x05;
        public const byte SA = 0x06;
        public const byte KING = 0x07;

        public const byte KIND = 0x07;

        public const byte CHO = 0x08;
        public const byte HAN = 0x10;
        public const byte SIDE = 0x18;

        public const byte _HAN = 0x_EF;


        public const byte CHO_JOL = JOL + CHO;
        public const byte CHO_SANG = SANG + CHO;
        public const byte CHO_MA = MA + CHO;
        public const byte CHO_PO = PO + CHO;
        public const byte CHO_CHA = CHA + CHO;
        public const byte CHO_SA = SA + CHO;
        public const byte CHO_KING = KING + CHO;

        public const byte HAN_JOL = JOL + HAN;
        public const byte HAN_SANG = SANG + HAN;
        public const byte HAN_MA = MA + HAN;
        public const byte HAN_PO = PO + HAN;
        public const byte HAN_CHA = CHA + HAN;
        public const byte HAN_SA = SA + HAN;
        public const byte HAN_KING = KING + HAN;


        public static bool IS_KIND(byte stone, byte kind) => (stone & KIND) == kind;
        public static bool IS_SIDE(byte stone, byte side) => (stone & SIDE) == side;

        public static int SIDE_VALUE(byte side) => ((side & CHO) >> 3) + ((side & HAN) >> 4) * (-1);




    }
    //public static class Stones
    //{
    //    public const int Empty = 0;
    //    public const int pJol = 1;
    //    public const int pSang = 2;
    //    public const int pMa = 3;
    //    public const int pPo = 4;
    //    public const int pCha = 5;
    //    public const int pSa = 6;
    //    public const int pKing = 7;
    //    public const int nJol = -1;
    //    public const int nSang = -2;
    //    public const int nMa = -3;
    //    public const int nPo = -4;
    //    public const int nCha = -5;
    //    public const int nSa = -6;
    //    public const int nKing = -7;
    //}
}
