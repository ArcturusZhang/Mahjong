using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 自摸 : Yaku
    {
        public override string Name => "门前清自摸和";

        public override int Value => 1;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            return options.HasFlag(YakuOptions.Menqing) && options.HasFlag(YakuOptions.Zimo);
        }
    }
}