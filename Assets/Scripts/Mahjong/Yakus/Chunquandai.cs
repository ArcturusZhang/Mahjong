using System.Linq;

namespace Mahjong.Yakus
{
    public class Chunquandai : Yaku
    {
        private static readonly Yaku qinglaotou = new Qinglaotou();
        private int value = 3;
        
        public override string Name
        {
            get { return "纯全带幺九"; }
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