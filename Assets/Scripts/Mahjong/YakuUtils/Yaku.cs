using System;
using System.Linq;

namespace Mahjong.YakuUtils
{
    public abstract class Yaku : IComparable<Yaku>
    {
        public abstract string Name { get; }
        public abstract int Value { get; }
        public virtual bool IsYakuMan => false;
        public virtual int SortIndex => -1;

        public virtual YakuType Type => YakuType.Normal;

        public abstract bool Test(MianziSet hand, Tile rong, GameStatus status, YakuOptions options);

        public int CompareTo(Yaku other)
        {
            if (SortIndex > other.SortIndex) return 1;
            if (SortIndex < other.SortIndex) return -1;
            if (Value > other.Value) return 1;
            if (Value < other.Value) return -1;
            return string.CompareOrdinal(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Yaku;
            return other != null && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static void PreTest(MianziSet hand, Tile rong, params YakuOptions[] options)
        {
            if (options.Contains(YakuOptions.Zimo)) // 自摸的情形，将含有胡牌的刻字设为暗刻
            {
                for (int i = 0; i < hand.MianziCount; i++)
                {
                    var mianzi = hand[i];
                    if (mianzi.Type == MianziType.Kezi && mianzi.Contains(rong)) // 含有胡牌的刻子
                        mianzi.Open = false;
                }
            }
            else // 荣和的情形，将含有胡牌的刻字设为明刻
            {
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
        Menqian,
        Optional
    }
}