using System.Linq;
using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 混全带幺九 : Yaku
    {
        private static readonly Yaku qinglaotou = new 清老头();
        private static readonly Yaku hunlaotou = new 混老头();
        private static readonly Yaku chunquandai = new 纯全带幺九();
        private static readonly Yaku ziyise = new 字一色();
        private int value = 2;

        public override string Name => "混全带幺九";

        public override int Value => value;

        public override YakuType Type => YakuType.Shixia;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) value = 1;
            // 判定非清老头、字一色、混老头、纯全带
            if (qinglaotou.Test(hand, rong, status, options) || ziyise.Test(hand, rong, status, options) ||
                hunlaotou.Test(hand, rong, status, options) || chunquandai.Test(hand, rong, status, options))
                return false;
            foreach (var mianzi in hand)
            {
                if (!mianzi.HasYaojiu) return false;
            }

            return true;
        }
    }
}