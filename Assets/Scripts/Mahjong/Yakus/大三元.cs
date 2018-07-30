namespace Mahjong.Yakus
{
    public class 大三元 : Yaku
    {
        public override string Name => "大三元";

        public override int Value => 1;

        public override bool IsYakuMan => true;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int kezi = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit != Suit.Z || mianzi.Index < 5) continue;
                if (mianzi.Type == MianziType.Kezi)
                    kezi |= 1 << (mianzi.Index - 5);
            }
            
            return kezi == 7;
        }
    }
}