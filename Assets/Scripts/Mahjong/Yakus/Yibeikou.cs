using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Yakus
{
    public class Yibeikou : Yaku
    {
        private static readonly Yaku erbeikou = new Erbeikou();
        public override string Name
        {
            get { return "一杯口"; }
        }
        
        public override int Value
        {
            get { return 1; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) return false;
            if (erbeikou.Test(hand, rong, status, options)) return false;
            hand.Sort();
            for (int i = 1; i < hand.Count; i++)
            {
                if (hand[i].Type == MianziType.Shunzi && hand[i].Equals(hand[i - 1])) return true;
            }

            return false;
        }
    }
}