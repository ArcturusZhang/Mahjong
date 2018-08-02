using System.Linq;
using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 宝牌 : Yaku
    {
        private int value = 0;
        public override string Name => "宝牌";
        public override int Value => value;
        public override YakuType Type => YakuType.Optional;
        public override int SortIndex => int.MaxValue - 2;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int count = 0;
            foreach (var mianzi in hand)
            {
                foreach (var tile in mianzi.Tiles)
                {
                    if (status.Dora.Contains(tile)) count++;
                }
            }

            value = count;
            return count != 0;
        }
    }
}