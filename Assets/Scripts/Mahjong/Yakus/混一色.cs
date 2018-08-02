using System.Linq;
using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 混一色 : Yaku
    {
        private int value = 3;

        public override string Name => "混一色";

        public override int Value => value;

        public override YakuType Type => YakuType.Shixia;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) value = 2;
            int flag = 0;
            bool hasZi = false;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit == Suit.Z)
                    hasZi = true;
                else
                    flag |= 1 << (int) mianzi.Suit;
            }

            return YakuUtils.YakuUtil.Count1(flag) == 1 && hasZi;
        }
    }
}