using System;
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
        private readonly List<Tile> mTiles = new List<Tile>();
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
            var list = Tiles;
            list.Add(tile);
            return new MahjongHand(list);
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
            Suit suit = (Suit) suitOfTwo;
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

        private static Tile GetTile(int index)
        {
            Suit suit = (Suit) (index / 9);
            return new Tile(suit, index % 9 + 1);
        }

        private static int GetIndex(Tile tile)
        {
            return (int) tile.Suit * 9 + tile.Index - 1;
        }

        public IList<Tile> Tiles
        {
            get { return new List<Tile>(mTiles); }
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

        public IEnumerable<Tile> TingList
        {
            get
            {
                return !HasTing ? new List<Tile>() : new List<Tile>(tingList);
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

        private string stringForm = null;

        public override string ToString()
        {
            if (stringForm == null)
            {
                var builder = new StringBuilder();
                foreach (var tile in mTiles)
                {
                    builder.Append(tile);
                }

                stringForm = builder.ToString();
            }

            return stringForm;
        }
    }
}