using System.Linq;
using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 清一色 : Yaku
    {
        private int value = 6;

        public override string Name => "清一色";

        public override int Value => value;

        public override YakuType Type => YakuType.Shixia;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) value = 5;
            int flag = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit == Suit.Z) return false;
                flag |= 1 << (int) mianzi.Suit;
            }

            return YakuUtils.YakuUtil.Count1(flag) == 1;
        }
    }
}