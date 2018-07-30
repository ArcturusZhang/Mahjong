namespace Mahjong.Yakus
{
    public class 断幺九 : Yaku
    {
        public override string Name => "断幺九";

        public override int Value => 1;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.HasYaojiu) return false;
            }

            return true;
        }
    }
}