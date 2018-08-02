using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 役牌中 : Yaku
    {
        public override string Name => "役牌-中";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && mianzi.First.Equals(new Tile(Suit.Z, 7)))
                    return true;
            }

            return false;
        }
    }
}