using System.Linq;
using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 岭上开花 : Yaku
    {
        public override string Name => "岭上开花";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Lingshang);
        }
    }
}