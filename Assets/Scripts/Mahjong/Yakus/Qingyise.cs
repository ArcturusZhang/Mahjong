using System.Linq;

namespace Mahjong.Yakus
{
    public class Qingyise : Yaku
    {
        private int _value = 6;

        public override string Name
        {
            get { return "清一色"; }
        }

        public override int Value
        {
            get { return _value; }
        }

        public override YakuType Type
        {
            get { return YakuType.Shixia; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) _value = 5;
            int flag = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit == Suit.Z) return false;
                flag |= 1 << (int) mianzi.Suit;
            }

            return YakuUtils.Count1(flag) == 1;
        }
    }
}