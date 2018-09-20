using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 双立直 : Yaku
    {
        public override string Name => "双立直";

        public override int Value => 2;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            return options.HasFlag(YakuOptions.Menqing) && options.HasFlag(YakuOptions.Lizhi) &&
                   options.HasFlag(YakuOptions.FirstRound);
        }
    }
}