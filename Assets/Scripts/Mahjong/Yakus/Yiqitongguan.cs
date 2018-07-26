using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mahjong.Yakus
{
    public class Yiqitongguan : Yaku
    {
        private int value = 2;

        public override string Name
        {
            get { return "一气通贯"; }
        }

        public override int Value
        {
            get { return value; }
        }

        public override YakuType Type
        {
            get { return YakuType.Shixia; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) value = 1;
            var indexFlag = new int[4];
            foreach (var mianzi in hand)
            {
                indexFlag[(int) mianzi.Suit] |= 1 << (mianzi.First.Index - 1); // binary
            }
            
            for (int i = 0; i < 3; i++)
            {
                if ((indexFlag[i] & 1) != 0 && (indexFlag[i] & (1 << 3)) != 0
                    && (indexFlag[i] & (1 << 6)) != 0) return true; // (binary) 1001001 = (decimal) 73
            }

            return false;
        }
    }
}