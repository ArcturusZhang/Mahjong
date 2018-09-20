using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 四暗刻 : Yaku
    {
        private static readonly string normal = "四暗刻";
        private static readonly string single = "四暗刻·单骑听";
        private string name = normal;
        private int value = YakuUtil.YakuManBasePoint;

        public override string Name => name;

        public override int Value => value;

        public override bool IsYakuMan => true;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            int count = 0;
            bool isDanqi = false;
            foreach (var mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && !mianzi.Open) count++;
                if (mianzi.Type == MianziType.Jiang && mianzi.Contains(rong)) isDanqi = true;
            }

            if (isDanqi)
            {
                name = single;
                value = YakuUtil.YakuManBasePoint + 1;
            }
            else
            {
                name = normal;
                value = YakuUtil.YakuManBasePoint;
            }
            return count == 4;
        }
    }
}