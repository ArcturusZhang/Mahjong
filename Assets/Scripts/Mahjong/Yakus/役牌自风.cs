using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 役牌自风 : Yaku
    {
        public override string Name => "役牌-自风牌";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && mianzi.First.Equals(status.Zifeng))
                    return true;
            }

            return false;
        }
    }
}