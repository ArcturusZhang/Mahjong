using System;

namespace Mahjong.Yakus
{
    public class YipaiChangfeng : Yaku
    {
        public string Name
        {
            get { return "役牌：场风牌"; }
        }

        public int Value
        {
            get { return 1; }
        }

        public bool IsYakuMan
        {
            get { return false; }
        }

        public bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type == MianziType.Kezi && mianzi.First.Equals(status.Changfeng))
                    return true;
            }

            return false;
        }
    }
}