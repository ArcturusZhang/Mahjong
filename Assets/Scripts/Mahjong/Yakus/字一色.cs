namespace Mahjong.Yakus
{
    public class 字一色 : Yaku
    {
        public override string Name => "字一色";
        public override int Value => 1;
        public override bool IsYakuMan => true;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            foreach (var mianzi in hand)
            {
                if (mianzi.Suit != Suit.Z) return false;
            }

            return true;
        }
    }
}