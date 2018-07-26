namespace Mahjong.Yakus
{
    public class Sananke : Yaku
    {
        private static readonly Yaku sianke = new Sianke();

        public override string Name
        {
            get { return "三暗刻"; }
        }

        public override int Value
        {
            get { return 2; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (sianke.Test(hand, rong, status, options)) return false;
            int count = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && !mianzi.Open) count++;
            }

            return count >= 3;
        }
    }
}