using System;
using System.Linq;

namespace Mahjong.Yakus
{
    public abstract class Yaku : IComparable<Yaku>
    {
        public abstract string Name { get; }
        public abstract int Value { get; }
        public virtual bool IsYakuMan => false;

        public virtual YakuType Type => YakuType.Normal;

        public abstract bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options);

        public int CompareTo(Yaku other)
        {
            if (Value != other.Value) return Value - other.Value;
            return string.CompareOrdinal(Name, other.Name);
        }

        public static void PreTest(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (options.Contains(YakuOption.Zimo)) // 自摸的情形，将含有胡牌的刻字设为暗刻
            {
                for (int i = 0; i < hand.MianziCount; i++)
                {
                    var mianzi = hand[i];
                    if (mianzi.Type == MianziType.Kezi && mianzi.Contains(rong)) // 含有胡牌的刻子
                        mianzi.Open = false;
                }
            }
            else // 荣和的情形，将含有胡牌的刻字设为明刻
            {// todo : may not work
                for (int i = 0; i < hand.MianziCount; i++)
                {
                    var mianzi = hand[i];
                    if (mianzi.Type == MianziType.Kezi && mianzi.Contains(rong)) // 含有胡牌的刻子
                        mianzi.Open = true;
                }
            }
        }
    }

    public enum YakuType
    {
        Normal,
        Shixia,
        Menqian
    }
}