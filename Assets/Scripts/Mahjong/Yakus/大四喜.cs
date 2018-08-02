using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 大四喜 : Yaku
    {
        public override string Name => "大四喜";
        public override int Value => YakuUtil.YakuManBasePoint + 1;
        public override bool IsYakuMan => true;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int kezi = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit != Suit.Z || mianzi.Index > 4) continue;
                if (mianzi.Type == MianziType.Kezi)
                    kezi |= 1 << (mianzi.Index - 1);
            }

            return kezi == 15;
        }
    }
}