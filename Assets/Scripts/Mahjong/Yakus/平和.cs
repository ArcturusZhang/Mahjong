using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 平和 : Yaku
    {
        public override string Name => "平和";

        public override int Value => 1;

        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            if (!options.HasFlag(YakuOptions.Menqing)) return false;
            int shunziCount = 0;
            bool twoSide = false;
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type == MianziType.Shunzi)
                {
                    shunziCount++;
                    if (mianzi.First.Equals(rong) || mianzi.Last.Equals(rong)) twoSide = true;
                }
            }

            return shunziCount == 4 && twoSide;
        }
    }
}