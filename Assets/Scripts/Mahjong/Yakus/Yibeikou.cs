namespace Mahjong.Yakus
{
    public class Yibeikou : Yaku
    {
        public string Name
        {
            get { return "一杯口"; }
        }
        public int Value
        {
            get { return 1; }
        }
        public bool IsYakuMan
        {
            get { return false; }
        }

        public bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            throw new System.NotImplementedException();
        }
    }
}