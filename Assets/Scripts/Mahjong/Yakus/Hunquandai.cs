using System.Linq;

namespace Mahjong.Yakus
{
    public class Hunquandai : Yaku
    {
        private static readonly Yaku qinglaotou = new Qinglaotou();
        private static readonly Yaku hunlaotou = new Hunlaotou();
        private static readonly Yaku chunquandai = new Chunquandai();
        private int value = 2;

        public override string Name
        {
            get { return "混全带幺九"; }
        }

        public override int Value
        {
            get { return value; }
        }

        public override YakuType Type
        {
            get { return YakuType.Shixia; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) value = 1;
            // 判定非清老头、混老头、纯全带
            if (qinglaotou.Test(hand, rong, status, options) || hunlaotou.Test(hand, rong, status, options)
                                                             || chunquandai.Test(hand, rong, status, options))
                return false;
            foreach (var mianzi in hand)
            {
                if (!mianzi.HasYaojiu) return false;
            }

            return true;
        }
    }
}