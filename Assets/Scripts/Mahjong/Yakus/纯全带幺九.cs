using System.Linq;

namespace Mahjong.Yakus
{
    public class 纯全带幺九 : Yaku
    {
        private static readonly Yaku qinglaotou = new 清老头();
        private int value = 3;
        
        public override string Name => "纯全带幺九";

        public override int Value => value;

        public override YakuType Type => YakuType.Shixia;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) value = 2;
            // 判定非清老头
            if (qinglaotou.Test(hand, rong, status, options)) return false;
            foreach (var mianzi in hand)
            {
                if (!mianzi.HasLaotou) return false;
            }

            return true;
        }
    }
}