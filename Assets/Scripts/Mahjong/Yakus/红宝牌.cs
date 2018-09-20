using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 红宝牌 : Yaku
    {
        private int value = 0;
        public override string Name => "红宝牌";
        public override int Value => value;
        public override YakuType Type => YakuType.Optional;
        public override int SortIndex => int.MaxValue - 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options)
        {
            int count = 0;
            foreach (var mianzi in hand)
            {
                foreach (var tile in mianzi.Tiles)
                {
                    if (tile.IsRed) count++;
                }
            }

            value = count;
            return count != 0;
        }
    }
}