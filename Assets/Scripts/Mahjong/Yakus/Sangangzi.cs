namespace Mahjong.Yakus
{
    public class Sangangzi : Yaku
    {
        public override string Name
        {
            get { return "三杠子"; }
        }

        public override int Value
        {
            get { return 2; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int count = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.IsGangzi) count++;
            }
            return count == 3;
        }
    }
}