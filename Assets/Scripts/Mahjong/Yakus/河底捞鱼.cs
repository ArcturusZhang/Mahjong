using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 河底捞鱼 : Yaku
    {
        private string name;
        private int value;
        private bool isYakuMan;

        public override string Name => "河底捞鱼";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            return options.HasFlag(YakuOptions.Haidi) && !options.HasFlag(YakuOptions.Zimo);
        }
    }
}