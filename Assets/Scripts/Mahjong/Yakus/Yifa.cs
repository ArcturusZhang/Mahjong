using System.Linq;

namespace Mahjong.Yakus
{
    public class Yifa : Yaku
    {
        public override string Name
        {
            get { return "一发"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool IsYakuMan
        {
            get { return false; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Menqing) && options.Contains(YakuOption.Lizhi) &&
                   options.Contains(YakuOption.Yifa);
        }
    }
}