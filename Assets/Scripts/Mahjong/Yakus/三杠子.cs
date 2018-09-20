using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 三杠子 : Yaku
    {
        public override string Name => "三杠子";

        public override int Value => 2;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            int count = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.IsGangzi) count++;
            }
            return count == 3;
        }
    }
}