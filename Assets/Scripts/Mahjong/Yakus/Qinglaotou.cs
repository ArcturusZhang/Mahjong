namespace Mahjong.Yakus
{
    public class Qinglaotou : Yaku
    {
        public override string Name
        {
            get { return "清老头"; }
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
            foreach (var mianzi in hand)
            {
                if (!mianzi.IsLaotou) return false;
            }
            return true;
        }
    }
}