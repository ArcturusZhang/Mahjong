using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 人和 : Yaku
    {
        public override string Name => "人和";

        public override int Value => YakuUtil.YakuManBasePoint;

        public override bool IsYakuMan => true;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            return options.HasFlag(YakuOptions.FirstRound) && !options.HasFlag(YakuOptions.Zimo);
        }
    }
}