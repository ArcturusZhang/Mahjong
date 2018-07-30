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
            var counts = new int[9];
            var suit = hand[0].Suit;
            if (suit == Suit.Z) return false;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit != suit) return false;
                
            }
            // todo
            return false;
        }
    }
}