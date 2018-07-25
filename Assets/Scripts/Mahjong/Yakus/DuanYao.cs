namespace Mahjong.Yakus
{
    public class DuanYao : Yaku
    {
        public string Name
        {
            get { return "断幺"; }
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
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.HasYaojiu) return false;
            }

            return true;
        }
    }
}