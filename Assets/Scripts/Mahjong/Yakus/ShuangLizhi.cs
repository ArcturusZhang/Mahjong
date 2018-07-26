using System.Linq;

namespace Mahjong.Yakus
{
    public class ShuangLizhi : Yaku
    {
        public override string Name
        {
            get { return "双立直"; }
        }

        public override int Value
        {
            get { return 2; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Menqing) && options.Contains(YakuOption.Lizhi) &&
                   options.Contains(YakuOption.FirstRound);
        }
    }
}