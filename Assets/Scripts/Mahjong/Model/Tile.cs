using System;
using System.Collections.Generic;
using UnityEngine;


namespace Mahjong.Model
{
    [Serializable]
    public struct Tile : IComparable<Tile>
    {
        public static readonly IEqualityComparer<Tile>
            TileConsiderColorEqualityComparer = new TileConsiderColorEqualityComparerImpl();
        public static readonly IEqualityComparer<Tile>
            TileIgnoreColorEqualityComparer = new TileIgnoreColorEqualityComparerImpl();
        public Suit Suit;
        public int Rank;
        public bool IsRed;

        public Tile(Suit suit, int rank, bool isRed = false) : this()
        {
            if (rank <= 0 || rank > 9) throw new ArgumentException("Index must be within range of 1 and 9");
            Suit = suit;
            if (Suit == Suit.Z && Rank > 7)
                throw new ArgumentException("Index of tiles in Suit of Zi must be within range of 1 and 7");
            Rank = rank;
            IsRed = isRed;
        }

        public bool IsYaojiu => Suit == Suit.Z || Rank == 1 || Rank == 9;

        public bool IsLaotou => Suit != Suit.Z && (Rank == 1 || Rank == 9);

        public Tile Previous => new Tile(Suit, Rank - 1);

        public Tile Next => new Tile(Suit, Rank + 1);

        public static bool TryTile(Suit suit, int rank, out Tile tile) {
            tile = default(Tile);
            switch (suit) {
                case Suit.M:
                case Suit.P:
                case Suit.S:
                    if (rank <= 0 || rank > 9) return false;
                    tile = new Tile(suit, rank);
                    return true;
                case Suit.Z:
                    if (rank <= 0 || rank > 7) return false;
                    tile = new Tile(suit, rank);
                    return true;
            }
            throw new Exception("This should never happen");
        }

        public int CompareTo(Tile other)
        {
            if (Suit != other.Suit) return Suit - other.Suit;
            if (Rank != other.Rank) return Rank - other.Rank;
            if (IsRed && !other.IsRed) return 1;
            if (!IsRed && other.IsRed) return -1;
            return 0;
        }

        public string ToStringIgnoreColor()
        {
            switch (Rank)
            {
                case 1:
                    {
                        switch (Suit)
                        {
                            case Suit.M: return "一";
                            case Suit.P: return "①";
                            case Suit.S: return "１";
                            case Suit.Z: return "东";
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
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
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
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
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
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
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
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
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
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
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
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
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
                        }
                    }
                case 8:
                    {
                        switch (Suit)
                        {
                            case Suit.M: return "八";
                            case Suit.P: return "⑧";
                            case Suit.S: return "８";
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
                        }
                    }
                case 9:
                    {
                        switch (Suit)
                        {
                            case Suit.M: return "九";
                            case Suit.P: return "⑨";
                            case Suit.S: return "９";
                            default: throw new ArgumentException($"Suit = {Suit} should not happen");
                        }
                    }
                default:
                    return $"Suit = {Suit}, Rank = {Rank}";
            }
        }

        public override string ToString()
        {
            return IsRed ? ToStringIgnoreColor() + "r" : ToStringIgnoreColor();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Tile)
            {
                var other = (Tile)obj;
                return Suit == other.Suit && Rank == other.Rank && IsRed == other.IsRed;
            }

            return false;
        }

        public bool EqualsIgnoreColor(Tile other)
        {
            return Suit == other.Suit && Rank == other.Rank;
        }

        public bool EqualsConsiderColor(Tile other)
        {
            return Suit == other.Suit && Rank == other.Rank && IsRed == other.IsRed;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        private struct TileConsiderColorEqualityComparerImpl : IEqualityComparer<Tile>
        {
            public bool Equals(Tile x, Tile y)
            {
                return x.EqualsConsiderColor(y);
            }

            public int GetHashCode(Tile obj)
            {
                int hash = (int)obj.Suit;
                hash = hash * 31 + obj.Rank;
                hash = hash * 31 + (obj.IsRed ? 1 : 0);
                return hash;
            }
        }

        private struct TileIgnoreColorEqualityComparerImpl : IEqualityComparer<Tile>
        {
            public bool Equals(Tile x, Tile y)
            {
                return x.EqualsIgnoreColor(y);
            }

            public int GetHashCode(Tile obj)
            {
                int hash = (int)obj.Suit;
                hash = hash * 31 + obj.Rank;
                return hash;
            }
        }

        public static Tile GetTile(int index)
        {
            var suit = (Suit)(index / 9);
            return new Tile(suit, index % 9 + 1);
        }

        public static int GetIndex(Tile tile)
        {
            return (int)tile.Suit * 9 + tile.Rank - 1;
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