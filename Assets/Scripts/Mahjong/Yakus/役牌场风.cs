using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 役牌场风 : Yaku
    {
        public override string Name => "役牌-场风牌";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && mianzi.First.Equals(status.Changfeng))
                    return true;
            }

            return false;
        }
    }
}