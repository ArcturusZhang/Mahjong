using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 抢杠 : Yaku
    {
        public override string Name => "抢杠";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            return options.HasFlag(YakuOptions.Qianggang);
        }
    }
}