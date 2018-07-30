using System.Linq;

namespace Mahjong.Yakus
{
    public class 地和 : Yaku
    {
        public override string Name => "地和";

        public override int Value => 1;

        public override bool IsYakuMan => true;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.FirstRound) && options.Contains(YakuOption.Zimo);
        }
    }
}