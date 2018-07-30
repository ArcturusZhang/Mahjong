namespace Mahjong.Yakus
{
    public class 四暗刻 : Yaku
    {
        private string name;
        private int value;

        public override string Name => "四暗刻";

        public override int Value => 1;

        public override bool IsYakuMan => true;

        // todo -- 自摸和荣胡导致的暗刻差别
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