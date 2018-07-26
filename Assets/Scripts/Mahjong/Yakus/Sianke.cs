namespace Mahjong.Yakus
{
    public class Sianke : Yaku
    {
        private string name;
        private int value;

        public override string Name
        {
            get { return "四暗刻"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool IsYakuMan
        {
            get { return true; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int count = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && !mianzi.Open) count++;
            }
            return count == 4;
        }
    }
}