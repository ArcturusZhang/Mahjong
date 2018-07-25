namespace Mahjong.Yakus
{
    public class YipaiZhong : Yaku
    {
        public string Name
        {
            get { return "役牌：三元牌(中)"; }
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
                if (mianzi.Type == MianziType.Kezi && mianzi.First.Equals(new Tile(Suit.Z, 7)))
                    return true;
            }

            return false;
        }
    }
}