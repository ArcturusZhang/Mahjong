using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 一发 : Yaku
    {
        public override string Name => "一发";

        public override int Value => 1;

        public override bool IsYakuMan => false;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            return options.HasFlag(YakuOptions.Menqing) && options.HasFlag(YakuOptions.Lizhi) &&
                   options.HasFlag(YakuOptions.Yifa);
        }
    }
}