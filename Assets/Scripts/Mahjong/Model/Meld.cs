using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mahjong.Model
{
    [Serializable]
    public struct Meld : IComparable<Meld>
    {
        public static IEqualityComparer<Meld> MeldConsiderColorEqualityComparer =
            new MeldConsiderColorEqualityComparerImpl();

        public static IEqualityComparer<Meld> MeldIgnoreColorEqualityComparer =
            new MeldIgnoreColorEqualityComparerImpl();

        public MeldType Type;
        public Tile[] Tiles;
        public bool Revealed;
        public bool IsKong;

        public Meld(bool revealed, params Tile[] tiles)
        {
            Revealed = revealed;
            Tiles = new Tile[tiles.Length];
            Array.Copy(tiles, Tiles, tiles.Length);
            Array.Sort(Tiles);
            IsKong = false;
            switch (Tiles.Length)
            {
                case 1:
                    Type = MeldType.Single;
                    break;
                case 2:
                    if (!Tiles[0].EqualsIgnoreColor(Tiles[1])) throw new ArgumentException("Invalid meld composition");
                    Type = MeldType.Pair;
                    break;
                case 3:
                    Type = Tiles[0].EqualsIgnoreColor(Tiles[2]) ? MeldType.Triplet : MeldType.Sequence;
                    if (Type == MeldType.Triplet)
                    {
                        if (!Tiles[0].EqualsIgnoreColor(Tiles[1]))
                            throw new ArgumentException("Invalid meld composition");
                    }
                    else if (Type == MeldType.Sequence)
                    {
                        if (Tiles[0].Suit == Suit.Z) throw new ArgumentException("Suit of Z cannot form sequences");
                        if (Tiles[0].Suit != Tiles[1].Suit || Tiles[0].Suit != Tiles[2].Suit)
                            throw new ArgumentException("Invalid meld composition");
                        if (Tiles[1].Rank != Tiles[0].Rank + 1 || Tiles[2].Rank != Tiles[0].Rank + 2)
                            throw new ArgumentException("Invalid meld composition");
                    }
                    else throw new ArgumentException("Will not happen");
                    break;
                case 4:
                    for (int i = 1; i < 4; i++)
                        if (!Tiles[i].EqualsIgnoreColor(Tiles[i - 1]))
                            throw new ArgumentException("Invalid meld composition");
                    Type = MeldType.Triplet;
                    IsKong = true;
                    break;
                default:
                    throw new ArgumentException("Invalid tile count");
            }
        }

        public Tile First => Tiles[0];
        public Tile Last => Tiles[Tiles.Length - 1];
        public Suit Suit => First.Suit;
        public int EffectiveTileCount => Math.Min(Tiles.Length, 3);
        public int TileCount => Tiles.Length;

        public bool HasYaojiu => Type != MeldType.Single && (First.IsYaojiu || Last.IsYaojiu);
        public bool IsYaojiu => Type != MeldType.Single && First.IsYaojiu && Last.IsYaojiu;
        public bool HasLaotou => Type != MeldType.Single && (First.IsLaotou || Last.IsLaotou);
        public bool IsLaotou => Type != MeldType.Single && First.IsLaotou && Last.IsLaotou;

        public int CompareTo(Meld other)
        {
            if (!First.EqualsConsiderColor(other.First)) return First.CompareTo(other.First);
            if (Type != other.Type) return Type - other.Type;
            var hasRed = Tiles.Any(tile => tile.IsRed);
            var otherHasRed = other.Tiles.Any(tile => tile.IsRed);
            if (hasRed && !otherHasRed) return 1;
            if (!hasRed && otherHasRed) return -1;
            return 0;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var tile in Tiles)
            {
                builder.Append(tile);
            }

            if (Revealed) builder.Append("(revealed)");

            return builder.ToString();
        }

        public string ToStringIgnoreColor()
        {
            var builder = new StringBuilder();
            foreach (var tile in Tiles)
            {
                builder.Append(tile.ToStringIgnoreColor());
            }

            if (Revealed) builder.Append("(revealed)");

            return builder.ToString();
        }

        public bool ContainsIgnoreColor(Tile tile)
        {
            if (Suit != tile.Suit) return false;
            return tile.Rank >= First.Rank && tile.Rank <= Last.Rank;
        }

        public bool ContainsConsiderColor(Tile tile)
        {
            return Tiles.Contains(tile, Tile.TileConsiderColorEqualityComparer);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Meld)) return false;
            var other = (Meld)obj;
            if (Type != other.Type || Revealed != other.Revealed) return false;
            if (Tiles.Length != other.Tiles.Length) return false;
            for (int i = 0; i < Tiles.Length; i++)
            {
                if (!Tiles[i].EqualsConsiderColor(other.Tiles[i])) return false;
            }

            return true;
        }

        public bool IdenticalTo(MeldType type, Tile first)
        {
            return Type == type && First.EqualsIgnoreColor(first);
        }

        public Meld AddToKong(Tile extra)
        {
            if (Type != MeldType.Triplet || IsKong || !First.EqualsIgnoreColor(extra))
                throw new InvalidOperationException();
            var tiles = new List<Tile>(Tiles) { extra };
            return new Meld(Revealed, tiles.ToArray());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool IsTwoSideIgnoreColor(Tile tile)
        {
            if (Type != MeldType.Sequence) return false;
            if (!First.EqualsIgnoreColor(tile) && !Last.EqualsIgnoreColor(tile)) return false;
            if (tile.Rank != 3 && tile.Rank != 7) return true;
            if (tile.Rank == 3 && First.Rank == 1) return false;
            if (tile.Rank == 7 && Last.Rank == 9) return false;
            return true;
        }

        public bool IsTwoSideConsiderColor(Tile tile)
        {
            if (Type != MeldType.Sequence) return false;
            if (!First.EqualsConsiderColor(tile) && !Last.EqualsConsiderColor(tile)) return false;
            if (tile.Rank != 3 && tile.Rank != 7) return true;
            if (tile.Rank == 3 && First.Rank == 1) return false;
            if (tile.Rank == 7 && Last.Rank == 9) return false;
            return true;
        }

        public int IndexOfIgnoreColor(Tile tile)
        {
            return Array.FindIndex(Tiles, t => t.EqualsIgnoreColor(tile));
        }

        public Tile[] GetForbiddenTiles(Tile tile)
        {
            switch (Type)
            {
                case MeldType.Triplet:
                    return new Tile[] { tile };
                case MeldType.Sequence:
                    return GetForbiddenTilesForSequence(tile);
                default:
                    Debug.LogError($"Type of {Type} should not call this method");
                    return null;
            }
        }

        private Tile[] GetForbiddenTilesForSequence(Tile tile)
        {
            var list = new List<Tile>() { tile };
            int index = IndexOfIgnoreColor(tile);
            if (index == 1) return list.ToArray();
            int other;
            if (index == 0) other = tile.Rank + 3;
            else other = tile.Rank - 3;
            if (other <= 0 || other > 9) return list.ToArray();
            list.Add(new Tile(tile.Suit, other));
            return list.ToArray();
        }

        /// <summary>
        /// This comparer is identical to the default Equals method
        /// </summary>
        private struct MeldConsiderColorEqualityComparerImpl : IEqualityComparer<Meld>
        {
            public bool Equals(Meld x, Meld y)
            {
                if (x.Type != y.Type || x.Revealed != y.Revealed) return false;
                if (x.Tiles.Length != y.Tiles.Length) return false;
                for (int i = 0; i < x.Tiles.Length; i++)
                {
                    if (!x.Tiles[i].EqualsConsiderColor(y.Tiles[i])) return false;
                }

                return true;
            }

            public int GetHashCode(Meld obj)
            {
                return obj.GetHashCode();
            }
        }

        private struct MeldIgnoreColorEqualityComparerImpl : IEqualityComparer<Meld>
        {
            public bool Equals(Meld x, Meld y)
            {
                if (x.Type != y.Type || x.Revealed != y.Revealed) return false;
                if (x.Tiles.Length != y.Tiles.Length) return false;
                for (int i = 0; i < x.Tiles.Length; i++)
                {
                    if (!x.Tiles[i].EqualsIgnoreColor(y.Tiles[i])) return false;
                }

                return true;
            }

            public int GetHashCode(Meld obj)
            {
                return obj.ToStringIgnoreColor().GetHashCode();
            }
        }
    }

    public enum MeldType
    {
        Triplet = 0,
        Sequence = 1,
        Pair = 2,
        Single = 3
    }
}