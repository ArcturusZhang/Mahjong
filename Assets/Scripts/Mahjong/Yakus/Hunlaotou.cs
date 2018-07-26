namespace Mahjong.Yakus
{
    public class Hunlaotou : Yaku
    {
        private static readonly Yaku qinglaotou = new Qinglaotou();
        public override string Name
        {
            get { return "混老头"; }
        }

        public override int Value
        {
            get { return 2; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            // 判定是否清老头
            if (qinglaotou.Test(hand, rong, status, options)) return false;
            foreach (var mianzi in hand)
            {
                if (!mianzi.IsYaojiu) return false;
            }

            return true;
        }
    }
}