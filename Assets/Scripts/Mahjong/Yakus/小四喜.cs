using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 小四喜 : Yaku
    {
        public override string Name => "小四喜";
        public override int Value => YakuUtil.YakuManBasePoint;
        public override bool IsYakuMan => true;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int kezi = 0, duizi = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit != Suit.Z || mianzi.Index > 4) continue;
                switch (mianzi.Type)
                {
                    case MianziType.Kezi:
                        kezi |= 1 << (mianzi.Index - 1);
                        break;
                    case MianziType.Jiang:
                        duizi |= 1 << (mianzi.Index - 1);
                        break;
                }
            }

            if (kezi == 0 || kezi == 15 || duizi == 0 || duizi == 15) return false;
            return (kezi ^ duizi) == 15;
        }
    }
}