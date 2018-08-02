using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 三暗刻 : Yaku
    {
        private static readonly Yaku sianke = new 四暗刻();

        public override string Name => "三暗刻";

        public override int Value => 2;

        // todo -- 自摸和荣胡导致的暗刻差别
        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (sianke.Test(hand, rong, status, options)) return false;
            int count = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && !mianzi.Open) count++;
            }

            return count >= 3;
        }
    }
}