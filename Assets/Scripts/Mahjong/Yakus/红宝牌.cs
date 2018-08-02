using System;
using Mahjong.YakuUtils;

namespace Mahjong.Yakus
{
    public class 红宝牌 : Yaku
    {
        private int value = 0;
        public override string Name => "红宝牌";
        public override int Value => value;
        public override int SortIndex => Int32.MaxValue - 2;

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            // todo
            return false;
        }
    }
}