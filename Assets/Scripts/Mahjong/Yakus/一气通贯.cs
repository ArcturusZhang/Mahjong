using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 一气通贯 : Yaku
    {
        private int value = 2;

        public override string Name => "一气通贯";

        public override int Value => value;

        public override YakuType Type => YakuType.Shixia;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            value = options.HasFlag(YakuOptions.Menqing) ? 2 : 1;
            var indexFlag = new int[4];
            foreach (var mianzi in hand)
            {
                if (mianzi.Type == MianziType.Shunzi)
                    indexFlag[(int) mianzi.Suit] |= 1 << (mianzi.First.Index - 1); // binary
            }

            for (int i = 0; i < 3; i++)
            {
                if ((indexFlag[i] & 1) != 0 && (indexFlag[i] & (1 << 3)) != 0
                                            && (indexFlag[i] & (1 << 6)) != 0)
                    return true; // (binary) 1001001 = (decimal) 73
            }

            return false;
        }
    }
}