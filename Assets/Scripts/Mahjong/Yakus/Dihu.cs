using System.Linq;

namespace Mahjong.Yakus
{
    public class Dihu : Yaku
    {
        public override string Name
        {
            get { return "地和"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool IsYakuMan
        {
            get { return true; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.FirstRound) && options.Contains(YakuOption.Zimo);
        }
    }
}