using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 海底捞月 : Yaku
    {
        public override string Name => "海底捞月";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            return options.HasFlag(YakuOptions.Haidi) && options.HasFlag(YakuOptions.Zimo);
        }
    }
}