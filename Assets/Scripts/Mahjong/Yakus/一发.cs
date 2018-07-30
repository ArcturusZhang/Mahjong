using System.Linq;

namespace Mahjong.Yakus
{
    public class 一发 : Yaku
    {
        public override string Name => "一发";

        public override int Value => 1;

        public override bool IsYakuMan => false;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Menqing) && options.Contains(YakuOption.Lizhi) &&
                   options.Contains(YakuOption.Yifa);
        }
    }
}