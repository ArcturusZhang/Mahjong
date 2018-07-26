using System.Linq;

namespace Mahjong.Yakus
{
    public class Lizhi : Yaku
    {
        public override string Name
        {
            get { return "立直"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Menqing) && options.Contains(YakuOption.Lizhi) &&
                   !options.Contains(YakuOption.FirstRound);
        }
    }
}