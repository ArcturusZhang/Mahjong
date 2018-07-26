using System.Linq;

namespace Mahjong.Yakus
{
    public class Hedi : Yaku
    {
        private string name;
        private int value;
        private bool isYakuMan;

        public override string Name
        {
            get { return "河底捞鱼"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Haidi) && !options.Contains(YakuOption.Zimo);
        }
    }
}