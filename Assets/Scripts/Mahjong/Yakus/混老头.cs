using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 混老头 : Yaku
    {
        private static readonly Yaku qinglaotou = new 清老头();
        private static readonly Yaku ziyise = new 字一色();
        public override string Name => "混老头";

        public override int Value => 2;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            // 判定是否清老头
            if (qinglaotou.Test(hand, rong, status, options) || ziyise.Test(hand, rong, status, options)) return false;
            foreach (var mianzi in hand)
            {
                if (!mianzi.IsYaojiu) return false;
            }

            return true;
        }
    }
}