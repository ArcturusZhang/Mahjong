using System.Linq;

namespace Mahjong.Yakus
{
    public class Haidi : Yaku
    {
        public override string Name
        {
            get { return "海底捞月"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Haidi) && options.Contains(YakuOption.Zimo);
        }
    }
}