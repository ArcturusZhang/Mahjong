using System.Linq;

namespace Mahjong.Yakus
{
    public class Lingshang : Yaku
    {
        public override string Name
        {
            get { return "岭上开花"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Lingshang);
        }
    }
}