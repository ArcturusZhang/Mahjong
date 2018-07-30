namespace Mahjong.Yakus
{
    public class 国士无双 : Yaku
    {
        private static readonly string normal = "国士无双";
        private static readonly string shisan = "国士无双·十三面听";
        private string name = normal;
        private int value = 1;
        public override string Name => name;
        public override int Value => value;
        public override bool IsYakuMan => true;
        public override YakuType Type => YakuType.Menqian;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (hand.Count != 13) return false;
            bool isShisan = false;
            foreach (var mianzi in hand)
            {
                if (mianzi.Contains(rong) && mianzi.Type == MianziType.Jiang)
                {
                    isShisan = true;
                    break;
                }
            }

            if (isShisan)
            {
                name = shisan;
                value = 2;
            }
            return true;
        }
    }
}