using System.Linq;

namespace Mahjong.Yakus
{
    public class Sansetongshun : Yaku
    {
        private int value = 2;

        public override string Name
        {
            get { return "三色同顺"; }
        }

        public override int Value
        {
            get { return value; }
        }

        public override YakuType Type
        {
            get { return YakuType.Shixia; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) value = 1;
            var suitFlag = new int[9];
            foreach (var mianzi in hand)
            {
                if (mianzi.Type == MianziType.Shunzi)
                    suitFlag[mianzi.First.Index - 1] |= 1 << (int) mianzi.Suit; // binary: M = 1, P = 10, S = 100
            }

            for (int i = 0; i < 9; i++)
            {
                if (suitFlag[i] == 7) return true; // (binary) 111 = (decimal) 7
            }

            return false;
        }
    }
}