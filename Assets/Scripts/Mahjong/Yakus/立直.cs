using System.Linq;

namespace Mahjong.Yakus
{
    public class 立直 : Yaku
    {
        public override string Name => "立直";

        public override int Value => 1;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Menqing) && options.Contains(YakuOption.Lizhi) &&
                   !options.Contains(YakuOption.FirstRound);
        }
    }
}