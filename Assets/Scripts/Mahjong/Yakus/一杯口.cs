using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 一杯口 : Yaku
    {
        private static readonly Yaku erbeikou = new 二杯口();
        public override string Name => "一杯口";

        public override int Value => 1;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            if (!options.HasFlag(YakuOptions.Menqing)) return false;
            if (erbeikou.Test(hand, rong, status, options)) return false;
            hand.Sort();
            for (int i = 1; i < hand.MianziCount; i++)
            {
                if (hand[i].Type == MianziType.Shunzi && hand[i].Equals(hand[i - 1])) return true;
            }

            return false;
        }
    }
}