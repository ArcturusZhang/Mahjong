using System;
using System.Collections.Generic;

namespace Mahjong
{
    [Serializable]
    public class Mianzi : IComparable<Mianzi>
    {
        public MianziType Type { get; }
        public Tile First { get; }
        public bool Open { get; set; }
        private bool isGangzi;

        public IEnumerable<Tile> Tiles { get; }

        public Mianzi(Tile first, MianziType type, bool open = false, bool gangzi = false)
        {
            First = first;
            Type = type;
            Open = open;
            isGangzi = gangzi;
            var tiles = new List<Tile>();
            switch (Type)
            {
                case MianziType.Single:
                    tiles.Add(first);
                    break;
                case MianziType.Jiang:
                    tiles.Add(first);
                    tiles.Add(first);
                    break;
                case MianziType.Kezi:
                    tiles.Add(first);
                    tiles.Add(first);
                    tiles.Add(first);
                    if (gangzi) tiles.Add(first);
                    break;
                case MianziType.Shunzi:
                    if (First.Suit == Suit.Z) throw new ArgumentException("Suit of Zi cannot compose a Shunzi");
                    tiles.Add(first);
                    tiles.Add(first.Next);
                    tiles.Add(first.Next.Next);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Tiles = tiles;
        }

        public Mianzi(bool open = false, params Tile[] tiles)
        {
            Open = open;
            var list = new List<Tile>();
            switch (tiles.Length)
            {
                case 1:
                    Type = MianziType.Single;
                    list.Add(tiles[0]);
                    break;
                case 2:
                    if (!tiles[0].Equals(tiles[1])) throw new ArgumentException("Invalid mianzi composition");
                    Type = MianziType.Jiang;
                    list.Add(tiles[0]);
                    list.Add(tiles[1]);
                    break;
                case 3:
                    if (tiles[0].Equals(tiles[2])) Type = MianziType.Kezi;
                    else Type = MianziType.Shunzi;
                    if (Type == MianziType.Kezi)
                    {
                        if (!tiles[0].Equals(tiles[1])) throw new ArgumentException("Invalid mianzi composition");
                    }
                    else if (Type == MianziType.Shunzi)
                    {
                        Array.Sort(tiles);
                        if (tiles[0].Suit == Suit.Z) throw new ArgumentException("Suit of Zi cannot compose a Shunzi");
                        if (tiles[0].Suit != tiles[1].Suit || tiles[0].Suit != tiles[2].Suit)
                            throw new ArgumentException("Invalid mianzi composition");
                        if (tiles[1].Index != tiles[0].Index + 1 || tiles[2].Index != tiles[0].Index + 2)
                            throw new ArgumentException("Invalid mianzi composition");
                    }
                    else
                    {
                        throw new ArgumentException("Should not happen");
                    }
                    list.Add(tiles[0]);
                    list.Add(tiles[1]);
                    list.Add(tiles[2]);
                    break;
                case 4:
                    for (int i = 1; i < 4; i++)
                    {
                        if (!tiles[i].Equals(tiles[i - 1])) throw new ArgumentException("Invalid mianzi composition");
                    }

                    Type = MianziType.Kezi;
                    isGangzi = true;
                    list.Add(tiles[0]);
                    list.Add(tiles[1]);
                    list.Add(tiles[2]);
                    list.Add(tiles[3]);
                    break;
                default:
                    throw new ArgumentException("Invalid tile count");
            }
            First = tiles[0];
            Tiles = list;
        }

        public Tile Last
        {
            get
            {
                switch (Type)
                {
                    case MianziType.Kezi:
                    case MianziType.Jiang:
                    case MianziType.Single:
                        return First;
                    case MianziType.Shunzi:
                        return First.Next.Next;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool IsGangzi
        {
            get
            {
                if (Type != MianziType.Kezi) return false;
                return isGangzi;
            }
        }

        public bool HasYaojiu => Type != MianziType.Single && (First.IsYaojiu || Last.IsYaojiu);

        public bool IsYaojiu => Type != MianziType.Single && First.IsYaojiu && Last.IsYaojiu;

        public bool HasLaotou => Type != MianziType.Single && (First.IsLaotou || Last.IsLaotou);

        public bool IsLaotou => Type != MianziType.Single && First.IsLaotou && Last.IsLaotou;

        public Suit Suit => First.Suit;

        public int Index => First.Index;

        public int CompareTo(Mianzi other)
        {
            if (!First.Equals(other.First)) return First.CompareTo(other.First); // 先按花色排序，然后按序数排序
            return Type - other.Type; // 第一张牌相同时，按刻子、顺子、将的顺序排列
        }

        public bool Contains(Tile tile)
        {
            if (tile.Suit != Suit) return false;
            return tile.Index >= First.Index && tile.Index <= Last.Index;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MianziType.Kezi:
                    return $"{First}{First}{First}";
                case MianziType.Shunzi:
                    return $"{First}{First.Next}{First.Next.Next}";
                case MianziType.Jiang:
                    return $"{First}{First}";
                case MianziType.Single:
                    return First.ToString();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Mianzi)
            {
                var other = (Mianzi) obj;
                return Type == other.Type && First.Equals(other.First);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    public enum MianziType
    {
        Kezi = 0,
        Shunzi = 1,
        Jiang = 2,
        Single = 3
    }
}