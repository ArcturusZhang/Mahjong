namespace Mahjong.Yakus
{
    public class Qiduizi : Yaku
    {
        public override string Name
        {
            get { return "七对子"; }
        }

        public override int Value
        {
            get { return 2; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int count = 0;
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type != MianziType.Jiang) return false;
                count++;
            }

            return count == 7;
        }
    }
}