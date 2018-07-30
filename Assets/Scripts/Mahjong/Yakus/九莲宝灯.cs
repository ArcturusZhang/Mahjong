using System.Linq;

namespace Mahjong.Yakus
{
    public class 九莲宝灯 : Yaku
    {
        private static readonly string normal = "九莲宝灯";
        private static readonly string pure = "纯正九莲宝灯";
        private string name = normal;
        private int value = 1;
        public override string Name => name;
        public override int Value => value;
        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) return false;
            if (hand.MianziCount != 5 || hand.TileCount != 14) return false;
            var suit = hand[0].Suit;
            if (suit == Suit.Z) return false;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit != suit) return false;
            }

            int index = MahjongHand.GetIndex(new Tile(suit, 1));
            var counts = hand.TileDistribution;
            for (int i = index; i < index + 9; i++)
                if (counts[i] == 0)
                    return false;
            if (counts[index] < 3 || counts[index + 8] < 3) return false;
            // 至此已经确定为九莲宝灯，下面检查是否为纯正九莲宝灯
            bool isPure = false;
            index = MahjongHand.GetIndex(rong);
            if (rong.Index == 1 || rong.Index == 9)
                isPure = counts[index] == 4;
            else
                isPure = counts[index] == 2;
            if (isPure)
            {
                name = pure;
                value = 2;
            }
            return true;
        }
    }
}