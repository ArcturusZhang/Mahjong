using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mahjong
{
    [Serializable]
    public class MahjongHand
    {
        private const int kinds = 4;
        private const int fullHandCount = 14;
        private const int tileKinds = 34;
        private string originalHandString;
        private List<Tile> mTiles;
        private int[] mHand;
        private HashSet<MianziSet> decompose;
        private bool hasWin;
        private List<Tile> tingList;
        private bool hasTing;
        public MahjongHand(string handString)
        {
            InitializeFromString(handString);
        }

        public MahjongHand(IEnumerable<Tile> tiles)
        {
            InitializeFromTiles(tiles);
        }

        public MahjongHand Add(string tile)
        {
            return new MahjongHand(originalHandString + tile);
        }

        public MahjongHand Add(Tile tile)
        {
            return new MahjongHand(originalHandString + tile);
        }

        private bool AnalyzeNormal(int[] hand)
        {
            var tilesCount = new int[kinds];
            int total = HandTilesCount(hand);
            for (int i = 0; i < tileKinds; i++)
                tilesCount[i / 9] += hand[i];
            if (total % 3 != 2) return false;
            // 计算每种花色牌除以3的余数
            int remainderOfZero = 0;
            int suitOfTwo = -1;
            for (int i = 0; i < kinds; i++)
            {
                if (tilesCount[i] % 3 == 1) return false;
                if (tilesCount[i] % 3 == 0) remainderOfZero++;
                if (tilesCount[i] % 3 == 2) suitOfTwo = i;
            }
            // 必须有3组余数为0才可能组成胡牌牌型
            if (remainderOfZero != 3) return false;
            var normalDecompose = new HashSet<MianziSet>();
            FindCompleteForm(suitOfTwo, hand, normalDecompose);
            foreach (var d in normalDecompose)
            {
                decompose.Add(d);
            }
            return normalDecompose.Count != 0;
        }

        private bool AnalyzeQiduizi(int[] hand)
        {
            int total = HandTilesCount(hand);
            if (total != fullHandCount) return false;
            var subList = new MianziSet();
            for (int index = 0; index < tileKinds; index++)
            {
                if (hand[index] != 0 && hand[index] != 2) return false;
                if (hand[index] == 2)
                {
                    var tile = GetTile(index);
                    subList.Add(new Mianzi(tile, MianziType.Jiang));
                }
            }
            decompose.Add(subList);
            return true;
        }

        private bool AnalyzeGuoshiWushuang(int[] hand)
        {
            int total = HandTilesCount(hand);
            if (total != fullHandCount) return false;
            var subList = new MianziSet();
            for (int index = 0; index < tileKinds; index++)
            {
                var tile = GetTile(index);
                if (!tile.IsYaojiu && hand[index] != 0) return false;
                if (tile.IsYaojiu && (hand[index] == 0 || hand[index] > 2)) return false;
                if (tile.IsYaojiu)
                {
                    var type = hand[index] == 1 ? MianziType.Single : MianziType.Jiang;
                    subList.Add(new Mianzi(tile, type));
                }
            }
            decompose.Add(subList);
            return true;
        }

        private int HandTilesCount(int[] hand)
        {
            int total = 0;
            for (int i = 0; i < tileKinds; i++) total += hand[i];
            return total;
        }

        private int[] TilesToArray(List<Tile> tiles)
        {
            var array = new int[tileKinds];
            foreach (var tile in tiles)
            {
                array[GetIndex(tile)]++;
            }
            return array;
        }

        private void FindCompleteForm(int suitOfTwo, int[] hand, HashSet<MianziSet> result)
        {
            Suit suit = (Suit)suitOfTwo;
            int start = suitOfTwo * 9;
            int end = suit == Suit.Z ? start + 7 : start + 9;
            for (int index = start; index < end; index++)
            {
                if (hand[index] >= 2) // 这张牌可以构成将牌
                {
                    hand[index] -= 2;
                    var decomposeForThisPair = new HashSet<MianziSet>();
                    Decompose(0, hand, new MianziSet(), decomposeForThisPair);
                    foreach (var com in decomposeForThisPair)
                    {
                        com.Add(new Mianzi(new Tile(suit, index - start + 1), MianziType.Jiang));
                        result.Add(com);
                    }
                    // 撤销这次更改，以便再测试下一组将牌
                    hand[index] += 2;
                }
            }
        }

        private void Decompose(int index, int[] hand, MianziSet current, HashSet<MianziSet> result)
        {
            if (index == hand.Length) // 出口
            {
                var newList = new MianziSet(current);
                newList.Sort();
                result.Add(newList);
                return;
            }
            if (hand[index] == 0) Decompose(index + 1, hand, current, result);
            var tile = GetTile(index);
            if (hand[index] >= 3) // 寻找刻子
            {
                hand[index] -= 3;
                current.Add(new Mianzi(tile, MianziType.Kezi));
                Decompose(index, hand, current, result);
                current.RemoveAt(current.Count - 1);
                hand[index] += 3;
            }
            // 寻找顺子
            if (tile.Suit != Suit.Z && tile.Index <= 7 && hand[index + 1] > 0 && hand[index + 2] > 0)
            {
                hand[index]--;
                hand[index + 1]--;
                hand[index + 2]--;
                current.Add(new Mianzi(tile, MianziType.Shunzi));
                Decompose(index, hand, current, result);
                current.RemoveAt(current.Count - 1);
                hand[index]++;
                hand[index + 1]++;
                hand[index + 2]++;
            }
        }

        private Tile GetTile(int index)
        {
            Suit suit = (Suit)(index / 9);
            return new Tile(suit, index % 9 + 1);
        }

        private int GetIndex(Tile tile)
        {
            return (int)tile.Suit * 9 + tile.Index - 1;
        }

        public IEnumerable<Tile> Tiles
        {
            get
            {
                return new List<Tile>(mTiles);
            }
        }

        public IEnumerable<MianziSet> Decomposition
        {
            get
            {
                if (!HasWin) return new HashSet<MianziSet>();
                var backup = new HashSet<MianziSet>();
                foreach (var subList in decompose)
                    backup.Add(new MianziSet(subList));
                return backup;
            }
        }

        public bool HasWin
        {
            get
            {
                if (decompose != null) return hasWin;
                decompose = new HashSet<MianziSet>();
                var normal = AnalyzeNormal(mHand);
                var qiduizi = AnalyzeQiduizi(mHand);
                var guoshi = AnalyzeGuoshiWushuang(mHand);
                hasWin = normal || qiduizi || guoshi;
                return hasWin;
            }
        }

        public bool HasTing
        {
            get
            {
                if (HasWin) return false;
                if (tingList == null) tingList = FindAllTingTiles(mHand);
                return tingList.Count != 0;
            }
        }

        public List<Tile> TingList
        {
            get
            {
                if (!HasTing) return new List<Tile>();
                return new List<Tile>(tingList);
            }
        }

        private List<Tile> FindAllTingTiles(int[] hand)
        {
            var result = new List<Tile>();
            int total = HandTilesCount(hand);
            if (total % 3 != 1) return result; // no ting
            for (int index = 0; index < tileKinds; index++)
            {
                if (hand[index] == 4) continue;
                hand[index]++;
                bool win = AnalyzeNormal(hand) || AnalyzeQiduizi(hand) || AnalyzeGuoshiWushuang(hand);
                if (win) result.Add(GetTile(index));
                hand[index]--;
            }
            decompose = null;
            return result;
        }

        private void InitializeFromString(string hand)
        {
            mTiles = new List<Tile>();
            originalHandString = hand;
            Resolve(hand, @"\d+[MmWw]", Suit.M, mTiles);
            Resolve(hand, @"\d+[PpBb]", Suit.P, mTiles);
            Resolve(hand, @"\d+[SsTt]", Suit.S, mTiles);
            Resolve(hand, @"\d+[Zz]", Suit.Z, mTiles);
            mTiles.Sort();
            mHand = TilesToArray(mTiles);
        }

        private void InitializeFromTiles(IEnumerable<Tile> tiles)
        {
            mTiles = new List<Tile>();
            foreach (var tile in tiles)
            {
                mTiles.Add(tile);
            }
            mTiles.Sort();
            var builder = new StringBuilder();
            foreach (var tile in mTiles)
                builder.Append(tile.ToString());
            originalHandString = builder.ToString();
            mHand = TilesToArray(mTiles);
        }

        private void Resolve(string hand, string pattern, Suit suit, List<Tile> result)
        {
            foreach (Match match in Regex.Matches(hand, pattern))
            {
                var mpsz = match.Value.Substring(0, match.Value.Length - 1);
                foreach (var c in mpsz)
                {
                    result.Add(new Tile(suit, c - '0'));
                }
            }
        }
    }

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
            return Index + Suit.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Tile)
            {
                var other = (Tile)obj;
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
        M = 0, P = 1, S = 2, Z = 3
    }

    [Serializable]
    public struct Mianzi : IComparable<Mianzi>
    {
        public MianziType Type { get; private set; }
        public Tile First { get; private set; }
        public bool Open { get; set; }
        public Mianzi(Tile first, MianziType type, bool open = false) : this()
        {
            First = first;
            Type = type;
            Open = open;
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
                        throw new ArgumentException("Invalid MianziType");
                }
            }
        }

        public bool HasYaojiu
        {
            get { return First.IsYaojiu || Last.IsYaojiu; }
        }

        public bool IsYaojiu
        {
            get { return First.IsYaojiu || Last.IsYaojiu; }
        }

        public Suit Suit
        {
            get { return First.Suit; }
        }

        public int CompareTo(Mianzi other)
        {
            if (!First.Equals(other.First)) return First.CompareTo(other.First); // 先按花色排序，然后按序数排序
            return Type - other.Type; // 第一张牌相同时，按刻子、顺子、将的顺序排列
        }

        public override string ToString()
        {
            switch (Type)
            {
                case MianziType.Kezi:
                    return string.Format("{0}{1}{2}", First.Index, First.Index, First.ToString());
                case MianziType.Shunzi:
                    return string.Format("{0}{1}{2}", First.Index, First.Next.Index, First.Next.Next.ToString());
                case MianziType.Jiang:
                    return string.Format("{0}{1}", First.Index, First.ToString());
                case MianziType.Single:
                    return First.ToString();
                default:
                    throw new ArgumentException("Will not happen");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Mianzi)
            {
                var other = (Mianzi)obj;
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
        Kezi = 0, Shunzi = 1, Jiang = 2, Single = 3
    }

    [Serializable]
    public class MianziSet : IEnumerable
    {
        private List<Mianzi> list;
        public MianziSet()
        {
            list = new List<Mianzi>();
        }

        public MianziSet(MianziSet copy) : this()
        {
            foreach (Mianzi mianzi in copy)
                list.Add(mianzi);
        }

        public void Add(Mianzi item)
        {
            list.Add(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public void Sort()
        {
            list.Sort();
        }

        public Mianzi this[int index]
        {
            get
            {
                return list[index];
            }
        }

        public int Count
        {
            get { return list.Count; }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var mianzi in list)
            {
                builder.Append(mianzi).Append(" ");
            }
            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is MianziSet)
            {
                var set = (MianziSet)obj;
                if (Count != set.Count) return false;
                for (int i = 0; i < Count; i++)
                {
                    if (!this[i].Equals(set[i])) return false;
                }
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}