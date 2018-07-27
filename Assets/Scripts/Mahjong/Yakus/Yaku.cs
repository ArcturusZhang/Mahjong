using System;

namespace Mahjong.Yakus
{
    public abstract class Yaku : IComparable<Yaku>
    {
        public abstract string Name { get; }
        public abstract int Value { get; }
        public virtual bool IsYakuMan
        {
            get { return false; }
        }

        public virtual YakuType Type
        {
            get { return YakuType.Normal; }
        }
        
        public abstract bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options);

        public int CompareTo(Yaku other)
        {
            if (Value != other.Value) return Value - other.Value;
            return string.CompareOrdinal(Name, other.Name);
        }
    }

    public enum YakuType
    {
        Normal,
        Shixia,
        Menqian
    }
}