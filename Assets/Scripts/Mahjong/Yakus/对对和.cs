using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 对对和 : Yaku
    {
        private static readonly Yaku dasixi = new 大四喜();
        public override string Name => "对对和";

        public override int Value => 2;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (dasixi.Test(hand, rong, status, options)) return false;
            int count = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Type == MianziType.Shunzi || mianzi.Type == MianziType.Single) return false;
                if (mianzi.Type == MianziType.Kezi) count++;
            }

            return count == 4;
        }
    }
}