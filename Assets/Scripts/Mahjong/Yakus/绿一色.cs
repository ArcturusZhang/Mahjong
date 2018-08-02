using System.Linq;
using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 绿一色 : Yaku
    {
        private static readonly string normal = "绿一色";
        private static readonly string pure = "纯绿一色";
        private static readonly int[] Greens = {19, 20, 21, 23, 25, 32};
        private string name = normal;
        private int value = YakuUtil.YakuManBasePoint;
        public override string Name => name;
        public override int Value => value;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            var counts = hand.TileDistribution;
            for (int i = 0; i < counts.Length; i++)
            {
                if (Greens.Contains(i)) continue;
                if (counts[i] != 0) return false;
            }

            if (counts[32] == 0)
            {
                name = pure;
                value = YakuUtil.YakuManBasePoint + 1;
            }
            else
            {
                name = normal;
                value = YakuUtil.YakuManBasePoint;
            }

            return true;
        }
    }
}