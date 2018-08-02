using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 清老头 : Yaku
    {
        public override string Name => "清老头";

        public override int Value => 1;

        public override bool IsYakuMan => true;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            foreach (var mianzi in hand)
            {
                if (!mianzi.IsLaotou) return false;
            }
            return true;
        }
    }
}