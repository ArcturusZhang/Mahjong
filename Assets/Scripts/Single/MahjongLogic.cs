using System.Collections.Generic;
using Multi;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Single
{
    public static class MahjongLogic
    {
        public static ISet<List<Meld>> Decompose(List<Tile> handTiles, List<Meld> openMelds, Tile tile)
        {
            var decompose = new HashSet<List<Meld>>(new ListEqualityComparer());
            int count = handTiles.Count;
            if (count % 3 != 1) return decompose;
            var allTiles = new List<Tile>(handTiles) {tile};
            var hand = TilesToArray(allTiles);
            AnalyzeNormal(hand, decompose);
            Analyze7Pairs(count, hand, decompose);
            Analyze13Orphans(count, hand, decompose);
            if (decompose.Count == 0) return decompose;
            var result = new HashSet<List<Meld>>(new ListEqualityComparer());
            foreach (var sub in decompose)
            {
                sub.AddRange(openMelds);
                sub.Sort();
                result.Add(sub);
            }

            return result;
        }

        public static bool HasWin(List<Tile> handTiles, List<Meld> openMelds, Tile tile)
        {
            return Decompose(handTiles, openMelds, tile).Count > 0;
        }

        public static IList<Tile> WinningTiles(List<Tile> handTiles, List<Meld> openMelds)
        {
            var list = new List<Tile>();
            for (int index = 0; index < MahjongConstants.TileKinds; index++)
            {
                var tile = GetTile(index);
                if (HasWin(handTiles, openMelds, tile))
                    list.Add(tile);
            }

            return list;
        }

        public static bool IsReady(List<Tile> handTiles, List<Meld> openMelds)
        {
            return WinningTiles(handTiles, openMelds).Count > 0;
        }

        private static void AnalyzeNormal(int[] hand, HashSet<List<Meld>> result)
        {
            var suitCount = new int[MahjongConstants.SuitCount];
            for (int i = 0; i < MahjongConstants.TileKinds; i++)
                suitCount[i / 9] += hand[i];
            // remainder of each suit must be 0 or 2
            int remainderOfZero = 0;
            int suitOfTwo = -1;
            for (int i = 0; i < suitCount.Length; i++)
            {
                if (suitCount[i] % 3 == 1) return;
                if (suitCount[i] % 3 == 0) remainderOfZero++;
                else if (suitCount[i] % 3 == 2) suitOfTwo = i;
            }

            if (remainderOfZero != 3) return;
            FindCompleteForm(suitOfTwo, hand, result);
        }

        private static void FindCompleteForm(int suitOfTwo, int[] hand, HashSet<List<Meld>> result)
        {
            var suit = (Suit) suitOfTwo;
            int start = suitOfTwo * 9;
            int end = suit == Suit.Z ? start + 7 : start + 9;
            for (int rank = start; rank < end; rank++)
            {
                if (hand[rank] >= 2) // this tile can form a pair
                {
                    hand[rank] -= 2;
                    var decompose = new HashSet<List<Meld>>();
                    DecomposeCore(0, hand, new List<Meld>(), decompose);
                    var pairTile = GetTile(rank);
                    foreach (var sub in decompose)
                    {
                        sub.Add(new Meld(false, pairTile, pairTile));
                        sub.Sort();
                        result.Add(sub);
                    }

                    hand[rank] += 2; // backtrack
                }
            }
        }

        private static void DecomposeCore(int index, int[] hand, List<Meld> current, HashSet<List<Meld>> result)
        {
            if (index == hand.Length) // outlet
            {
                var newList = new List<Meld>(current);
                newList.Sort();
                result.Add(newList);
                return;
            }

            if (hand[index] == 0) // no tile of this rank exists in hand
                DecomposeCore(index + 1, hand, current, result);
            var tile = GetTile(index);
            if (hand[index] >= 3) // find triplet
            {
                hand[index] -= 3;
                current.Add(new Meld(false, tile, tile, tile));
                DecomposeCore(index, hand, current, result);
                current.RemoveLast();
                hand[index] += 3; // backtrack
            }

            // find sequence
            if (tile.Suit != Suit.Z && tile.Rank <= 7 && hand[index + 1] > 0 && hand[index + 2] > 0)
            {
                hand[index]--;
                hand[index + 1]--;
                hand[index + 2]--;
                current.Add(new Meld(false, tile, tile.Next, tile.Next.Next));
                DecomposeCore(index, hand, current, result);
                current.RemoveLast();
                hand[index]++;
                hand[index + 1]++;
                hand[index + 2]++;
            }
        }

        private static void Analyze7Pairs(int total, int[] hand, HashSet<List<Meld>> result)
        {
            if (total != MahjongConstants.FullHandTilesCount) return;
            var sub = new List<Meld>();
            for (int index = 0; index < hand.Length; index++)
            {
                if (hand[index] != 0 && hand[index] != 2) return;
                if (hand[index] == 2)
                {
                    var tile = GetTile(index);
                    sub.Add(new Meld(false, tile, tile));
                }
            }

            sub.Sort();

            result.Add(sub);
        }

        private static void Analyze13Orphans(int total, int[] hand, HashSet<List<Meld>> result)
        {
            if (total != MahjongConstants.FullHandTilesCount) return;
            var sub = new List<Meld>();
            int kinds = 0;
            for (int index = 0; index < hand.Length; index++)
            {
                var tile = GetTile(index);
                if (!tile.IsYaojiu && hand[index] != 0) return;
                if (tile.IsYaojiu && (hand[index] == 0 || hand[index] > 2)) return;
                if (tile.IsYaojiu) // 1 or 2
                {
                    kinds++;
                    if (hand[index] == 1) sub.Add(new Meld(false, tile));
                    if (hand[index] == 2) sub.Add(new Meld(false, tile, tile));
                }
            }

            Assert.AreEqual(kinds, 13, "Something wrong happened");
            sub.Sort();
            result.Add(sub);
        }

        private static int[] TilesToArray(List<Tile> tiles)
        {
            var array = new int[MahjongConstants.TileKinds];
            foreach (var tile in tiles)
            {
                array[GetIndex(tile)]++;
            }

            return array;
        }

        private static Tile GetTile(int index)
        {
            var suit = (Suit) (index / 9);
            return new Tile(suit, index % 9 + 1);
        }

        private static int GetIndex(Tile tile)
        {
            return (int) tile.Suit * 9 + tile.Rank - 1;
        }

        public static InTurnOperation GetInTurnOperation(List<Tile> handTiles, List<Meld> openMelds, Tile tile)
        {
            var operation = InTurnOperation.Discard;
            if (openMelds.Count == 0) operation |= InTurnOperation.Richi;
            int count = 0;
            foreach (var handTile in handTiles)
            {
                if (handTile.EqualsIgnoreColor(tile)) count++;
            }

            // todo -- test for tsumo
            if (count == 3) operation |= InTurnOperation.ConcealedKong;
            if (count > 3) Debug.LogError("More than four identical tiles exists, this should not happen!");
            foreach (var meld in openMelds)
            {
                if (meld.Type == MeldType.Triplet && meld.First.EqualsIgnoreColor(tile))
                    operation |= InTurnOperation.AddedKong;
            }

            return operation;
        }

        private struct ListEqualityComparer : IEqualityComparer<List<Meld>>
        {
            public bool Equals(List<Meld> x, List<Meld> y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                if (x.Count != y.Count) return false;
                for (int i = 0; i < x.Count; i++)
                {
                    if (!x[i].Equals(y[i])) return false;
                }

                return true;
            }

            public int GetHashCode(List<Meld> obj)
            {
                int hash = 0;
                foreach (var meld in obj)
                {
                    hash = hash * 31 + meld.GetHashCode();
                }

                return hash;
            }
        }
    }
}