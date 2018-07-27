namespace Mahjong.Yakus
{
    public class Xiaosanyuan : Yaku
    {
        public override string Name
        {
            get { return "小三元"; }
        }

        public override int Value
        {
            get { return 2; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int kezi = 0, duizi = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit != Suit.Z || mianzi.Index < 5) continue;
                switch (mianzi.Type)
                {
                    case MianziType.Kezi:
                        kezi |= 1 << (mianzi.Index - 5);
                        break;
                    case MianziType.Jiang:
                        duizi |= 1 << (mianzi.Index - 5);
                        break;
                }
            }

            if (kezi == 0 || kezi == 7 || duizi == 0 || duizi == 7) return false;

            return (kezi ^ duizi) == 7;
        }
    }
}