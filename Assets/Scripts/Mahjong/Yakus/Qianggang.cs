using System.Linq;

namespace Mahjong.Yakus
{
    public class Qianggang : Yaku
    {
        public override string Name
        {
            get { return "抢杠"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            return options.Contains(YakuOption.Qianggang);
        }
    }
}