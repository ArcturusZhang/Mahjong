namespace Mahjong.Yakus
{
    public class YipaiBai : Yaku
    {
        public string Name
        {
            get { return "役牌：三元牌(白)"; }
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
                if (mianzi.Type == MianziType.Kezi && mianzi.First.Equals(new Tile(Suit.Z, 5)))
                    return true;
            }

            return false;
        }
    }
}