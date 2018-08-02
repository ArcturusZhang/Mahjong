using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 四杠子 : Yaku
    {
        public override string Name => "四杠子";
        public override int Value => YakuUtil.YakuManBasePoint;
        public override bool IsYakuMan => true;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            int count = 0;
            foreach (var mianzi in hand)
            {
                if (mianzi.IsGangzi) count++;
            }

            return count == 4;
        }
    }
}