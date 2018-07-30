namespace Mahjong.Yakus
{
    public class 七对子 : Yaku
    {
        public override string Name => "七对子";

        public override int Value => 2;

        public override YakuType Type => YakuType.Menqian;

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