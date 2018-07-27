using System.Linq;

namespace Mahjong.Yakus
{
    public class Hunyise : Yaku
    {
        private int value = 3;

        public override string Name
        {
            get { return "混一色"; }
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

            return YakuUtils.Count1(flag) == 1 && hasZi;
        }
    }
}