using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mahjong
{
    [Serializable]
    public struct Tile : IComparable<Tile>
    {
        public Suit Suit { get; private set; }
        public int Index { get; private set; }
        public bool IsRed { get; private set; }

        public Tile(Suit suit, int index, bool isRed = false) : this()
        {
            if (index <= 0 || index > 9) throw new ArgumentException("Index must be within range of 1 and 9");
            Suit = suit;
            if (Suit == Suit.Z && Index > 7)
                throw new ArgumentException("Index of tiles in Suit of Zi must be within range of 1 and 9");
            Index = index;
            IsRed = isRed;
        }

        public bool IsYaojiu
        {
            get { return Suit == Suit.Z || Index == 1 || Index == 9; }
        }

        public bool IsLaotou
        {
            get { return Suit != Suit.Z && (Index == 1 || Index == 9); }
        }

        public Tile Next
        {
            get { return new Tile(Suit, Index + 1); }
        }

        public int CompareTo(Tile other)
        {
            if (Suit != other.Suit) return Suit - other.Suit;
            return Index - other.Index;
        }

        public override string ToString()
        {
            switch (Index)
            {
                case 1:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "一";
                        case Suit.P: return "①";
                        case Suit.S: return "１";
                        case Suit.Z: return "东";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 2:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "二";
                        case Suit.P: return "②";
                        case Suit.S: return "２";
                        case Suit.Z: return "南";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 3:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "三";
                        case Suit.P: return "③";
                        case Suit.S: return "３";
                        case Suit.Z: return "西";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 4:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "四";
                        case Suit.P: return "④";
                        case Suit.S: return "４";
                        case Suit.Z: return "北";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 5:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "五";
                        case Suit.P: return "⑤";
                        case Suit.S: return "５";
                        case Suit.Z: return "白";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 6:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "六";
                        case Suit.P: return "⑥";
                        case Suit.S: return "６";
                        case Suit.Z: return "发";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 7:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "七";
                        case Suit.P: return "⑦";
                        case Suit.S: return "７";
                        case Suit.Z: return "中";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 8:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "八";
                        case Suit.P: return "⑧";
                        case Suit.S: return "８";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                case 9:
                {
                    switch (Suit)
                    {
                        case Suit.M: return "九";
                        case Suit.P: return "⑨";
                        case Suit.S: return "９";
                        case Suit.Z: return "北";
                        default: throw new ArgumentException("Will not happen");
                    }
                }
                default: throw new ArgumentException("Will not happen");
            }
        }

        public string ToOriginalString()
        {
            return Index + Suit.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Tile)
            {
                var other = (Tile) obj;
                return Suit == other.Suit && Index == other.Index;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public enum Suit
    {
        M = 0,
        P = 1,
        S = 2,
        Z = 3
    }
}