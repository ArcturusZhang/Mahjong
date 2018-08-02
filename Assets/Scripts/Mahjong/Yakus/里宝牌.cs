using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 里宝牌 : Yaku
    {
        private int value = 0;
        public override string Name => "里宝牌";
        public override int Value => value;
        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            // todo
            return false;
        }
    }
}