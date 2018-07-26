namespace Mahjong.Yakus
{
    public class YipaiZifeng : Yaku
    {
        public override string Name
        {
            get { return "役牌-自风牌"; }
        }

        public override int Value
        {
            get { return 1; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && mianzi.First.Equals(status.Zifeng))
                    return true;
            }

            return false;
        }
    }
}