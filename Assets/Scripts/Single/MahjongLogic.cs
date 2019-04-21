using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;


namespace Single
{
    public static class MahjongLogic
    {
        public readonly static IEnumerable<MethodInfo> YakuMethods;

        static MahjongLogic()
        {
            YakuMethods = typeof(YakuLogic).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(p =>
                p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new[]
                    {typeof(List<Meld>), typeof(Tile), typeof(HandStatus), typeof(RoundStatus), typeof(YakuSettings)}));
        }

        public static int CountFu(List<Meld> decompose, Tile winningTile, HandStatus handStatus,
            RoundStatus roundStatus, IList<YakuValue> yakus, YakuSettings yakuSettings)
        {
            if (decompose.Count == 7) return 25; // 7 pairs
            if (decompose.Count == 13) return 30; // 13 orphans
            int fu = 20; // base fu
            // Menqing and not tsumo
            if (handStatus.HasFlag(HandStatus.Menqing) && !handStatus.HasFlag(HandStatus.Tsumo)) fu += 10;
            // Tsumo
            if (handStatus.HasFlag(HandStatus.Tsumo) && !yakus.Any(yaku => yaku.Name == "平和" || yaku.Name == "岭上开花"))
                fu += 2;
            // pair
            var pair = decompose.First(meld => meld.Type == MeldType.Pair);
            if (pair.Suit == Suit.Z)
            {
                if (pair.First.Rank >= 5 && pair.First.Rank <= 7) fu += 2; // dragons
                var selfWind = roundStatus.SelfWind;
                var prevailingWind = roundStatus.PrevailingWind;
                if (pair.First.EqualsIgnoreColor(selfWind)) fu += 2;

                if (pair.First.EqualsIgnoreColor(prevailingWind))
                {
                    if (!prevailingWind.EqualsIgnoreColor(selfWind) ||
                        yakuSettings.连风对子额外加符) fu += 2;
                }
            }

            // sequences
            int flag = 0;
            foreach (var meld in decompose)
            {
                if (!meld.Tiles.Contains(winningTile)) continue;
                if (meld.Type == MeldType.Pair) flag++;
                if (meld.Type == MeldType.Sequence && !meld.Revealed && meld.IsTwoSideIgnoreColor(winningTile)) flag++;
            }

            if (flag != 0) fu += 2;
            // triplets
            var winningTileInOther = decompose.Any(meld => !meld.Revealed &&
                                                           (meld.Type == MeldType.Pair ||
                                                            meld.Type == MeldType.Sequence) &&
                                                           meld.ContainsIgnoreColor(winningTile));
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Triplet) continue;
                if (meld.Revealed)
                    fu += GetTripletFu(meld, true);
                else if (handStatus.HasFlag(HandStatus.Tsumo)) fu += GetTripletFu(meld, false);
                else if (winningTileInOther) fu += GetTripletFu(meld, false);
                else if (meld.ContainsIgnoreColor(winningTile)) fu += GetTripletFu(meld, true);
                else fu += GetTripletFu(meld, false);
            }

            return ToNextUnit(fu, 10);
        }

        public static int ToNextUnit(int value, int unit)
        {
            if (value % unit == 0) return value;
            return (value / unit + 1) * unit;
        }

        private static int GetTripletFu(Meld meld, bool revealed)
        {
            int triplet = revealed ? 2 : 4;
            if (meld.IsKong) triplet *= 4;
            if (meld.IsYaojiu) triplet *= 2;
            return triplet;
        }

        private static IList<YakuValue> CountYaku(List<Meld> decompose, Tile winningTile, HandStatus handStatus,
            RoundStatus roundStatus, YakuSettings yakuSettings)
        {
            var result = new List<YakuValue>();
            if (decompose == null || decompose.Count == 0) return result;
            foreach (var yakuMethod in YakuMethods)
            {
                var value = (YakuValue)yakuMethod.Invoke(yakuSettings,
                    new object[] { decompose, winningTile, handStatus, roundStatus, yakuSettings });
                if (value.Value != 0)
                {
                    result.Add(value);
                }
            }

            if (yakuSettings.青天井) return result;
            var hasYakuman = result.Any(yakuValue => yakuValue.Type == YakuType.Yakuman);
            return hasYakuman ? result.Where(yakuValue => yakuValue.Type == YakuType.Yakuman).ToList() : result;
        }

        public static PointInfo GetPointInfo(Tile[] handTiles, Meld[] openMelds, Tile winningTile,
            HandStatus handStatus, RoundStatus roundStatus, YakuSettings yakuSettings, Tile[] doraTiles = null,
            Tile[] uraDoraTiles = null)
        {
            var decomposes = Decompose(handTiles, openMelds, winningTile);
            if (decomposes.Count == 0) return new PointInfo();
            // count dora
            int dora = CountDora(handTiles, openMelds, winningTile, doraTiles);
            int uraDora = 0;
            if (handStatus.HasFlag(HandStatus.Richi) || handStatus.HasFlag(HandStatus.WRichi))
            {
                Assert.IsNotNull(uraDoraTiles, "There should be uraDoras after richi");
                uraDora = CountDora(handTiles, openMelds, winningTile, uraDoraTiles);
            }

            int redDora = CountRed(handTiles, openMelds, winningTile);
            return GetPointInfo(decomposes, winningTile, handStatus, roundStatus, yakuSettings, dora, uraDora, redDora);
        }

        private static int CountDora(Tile[] handTiles, Meld[] openMelds, Tile winningTile, Tile[] doraTiles)
        {
            if (doraTiles == null) return 0;
            int count = 0;
            foreach (var dora in doraTiles)
            {
                count += CountDora(handTiles, openMelds, winningTile, dora);
            }

            return count;
        }

        private static int CountDora(Tile[] handTiles, Meld[] openMelds, Tile winningTile, Tile dora)
        {
            int count = 0;
            foreach (var handTile in handTiles)
            {
                if (handTile.EqualsIgnoreColor(dora)) count++;
            }

            foreach (var meld in openMelds)
            {
                foreach (var tile in meld.Tiles)
                {
                    if (tile.EqualsIgnoreColor(dora)) count++;
                }
            }

            if (winningTile.EqualsIgnoreColor(dora)) count++;
            return count;
        }

        private static int CountRed(Tile[] handTiles, Meld[] openMelds, Tile winningTile)
        {
            int count = 0;
            count += handTiles.Count(tile => tile.IsRed);
            count += openMelds.Sum(meld => meld.Tiles.Count(tile => tile.IsRed));
            count += winningTile.IsRed ? 1 : 0;
            return count;
        }

        private static PointInfo GetPointInfo(ISet<List<Meld>> decomposes, Tile winningTile, HandStatus handStatus,
            RoundStatus roundStatus, YakuSettings yakuSettings, int dora = 0, int uraDora = 0, int redDora = 0)
        {
            Debug.Log($"GetPointInfo method, parameters: \ndecomposes: {DecompositionToString(decomposes)}, "
                      + $"winningTile: {winningTile}, handStatus: {handStatus}, roundStatus: {roundStatus}, "
                      + $"dora: {dora}, uraDora: {uraDora}, redDora: {redDora}");
            var infos = new List<PointInfo>();
            foreach (var decompose in decomposes)
            {
                var yakus = CountYaku(decompose, winningTile, handStatus, roundStatus, yakuSettings);
                var fu = CountFu(decompose, winningTile, handStatus, roundStatus, yakus, yakuSettings);
                if (yakus.Count == 0) continue;
                var info = new PointInfo(fu, yakus, yakuSettings.青天井, dora, uraDora, redDora);
                infos.Add(info);
                Debug.Log($"Decompose: {string.Join(", ", decompose)}, PointInfo: {info}");
            }

            if (infos.Count == 0) return new PointInfo();
            infos.Sort();
            Debug.Log($"CountPoint: {string.Join(", ", infos.Select(info => info.ToString()))}");
            return infos[infos.Count - 1];
        }

        public static int GetTotalPoint(PointInfo pointInfo, RoundStatus roundStatus)
        {
            return roundStatus.IsDealer
                ? pointInfo.BasePoint * roundStatus.TotalPlayer * 2
                : pointInfo.BasePoint * (roundStatus.TotalPlayer + 1);
        }

        private static ISet<List<Meld>> Decompose(IList<Tile> handTiles,
            IList<Meld> openMelds, Tile tile)
        {
            var decompose = new HashSet<List<Meld>>(new MeldListEqualityComparer());
            int count = handTiles.Count;
            if (count % 3 != 1) return decompose;
            var allTiles = new List<Tile>(handTiles) { tile };
            var hand = CountTiles(allTiles);
            Debug.Log($"Hand tile distribution: {string.Join(",", hand)}");
            AnalyzeNormal(hand, decompose);
            Analyze7Pairs(hand, decompose);
            Analyze13Orphans(hand, decompose);
            if (decompose.Count == 0) return decompose;
            var result = new HashSet<List<Meld>>(new MeldListEqualityComparer());
            foreach (var sub in decompose)
            {
                sub.AddRange(openMelds);
                sub.Sort();
                result.Add(sub);
            }

            return result;
        }

        public static bool HasWin(IList<Tile> handTiles, IList<Meld> openMelds, Tile tile)
        {
            return Decompose(handTiles, openMelds, tile).Count > 0;
        }

        public static IList<Tile> WinningTiles(IList<Tile> handTiles, IList<Meld> openMelds)
        {
            var list = new List<Tile>();
            for (int index = 0; index < MahjongConstants.TileKinds; index++)
            {
                var tile = Tile.GetTile(index);
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
            var suit = (Suit)suitOfTwo;
            int start = suitOfTwo * 9;
            int end = suit == Suit.Z ? start + 7 : start + 9;
            for (int rank = start; rank < end; rank++)
            {
                if (hand[rank] >= 2) // this tile can form a pair
                {
                    hand[rank] -= 2;
                    var decompose = new HashSet<List<Meld>>();
                    DecomposeCore(0, hand, new List<Meld>(), decompose);
                    var pairTile = Tile.GetTile(rank);
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
            var tile = Tile.GetTile(index);
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

        private static void Analyze7Pairs(int[] hand, HashSet<List<Meld>> result)
        {
            if (hand.Sum() != MahjongConstants.FullHandTilesCount) return;
            var sub = new List<Meld>();
            for (int index = 0; index < hand.Length; index++)
            {
                if (hand[index] != 0 && hand[index] != 2) return;
                if (hand[index] == 2)
                {
                    var tile = Tile.GetTile(index);
                    sub.Add(new Meld(false, tile, tile));
                }
            }
            sub.Sort();
            result.Add(sub);
        }

        private static void Analyze13Orphans(int[] hand, HashSet<List<Meld>> result)
        {
            if (hand.Sum() != MahjongConstants.FullHandTilesCount) return;
            var sub = new List<Meld>();
            int kinds = 0;
            for (int index = 0; index < hand.Length; index++)
            {
                var tile = Tile.GetTile(index);
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

        private struct MeldListEqualityComparer : IEqualityComparer<List<Meld>>
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

        public static int[] CountTiles(List<Meld> meldList)
        {
            var result = new int[MahjongConstants.TileKinds];
            foreach (var meld in meldList)
            {
                foreach (var tile in meld.Tiles)
                {
                    result[Tile.GetIndex(tile)]++;
                }
            }

            return result;
        }

        private static int[] CountTiles(List<Tile> tiles)
        {
            var array = new int[MahjongConstants.TileKinds];
            foreach (var tile in tiles)
            {
                array[Tile.GetIndex(tile)]++;
            }

            return array;
        }

        public static bool TestMenqing(IList<Meld> openMelds)
        {
            return openMelds.Count == 0 || openMelds.All(m => m.IsKong && !m.Revealed);
        }

        [System.Obsolete]
        public static ISet<Meld> GetChows(List<Tile> handTiles, Tile discardTile)
        {
            var result = new HashSet<Meld>();
            if (discardTile.Suit == Suit.Z) return result;
            var tilesP2 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank - 2);
            var tilesP1 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank - 1);
            var tilesN1 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank + 1);
            var tilesN2 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank + 2);
            foreach (var tileP2 in tilesP2)
                foreach (var tileP1 in tilesP1)
                    result.Add(new Meld(true, tileP2, tileP1, discardTile));

            foreach (var tileP1 in tilesP1)
                foreach (var tileN1 in tilesN1)
                    result.Add(new Meld(true, tileP1, discardTile, tileN1));

            foreach (var tileN1 in tilesN1)
                foreach (var tileN2 in tilesN2)
                    result.Add(new Meld(true, discardTile, tileN1, tileN2));
            return result;
        }

        [System.Obsolete]
        public static ISet<Meld> GetPongs(List<Tile> handTiles, Tile discardTile)
        {
            var result = new HashSet<Meld>();
            var tiles = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank);
            if (tiles.Count < 2) return result;
            if (tiles.Count == 2)
            {
                tiles.Add(discardTile);
                tiles.Sort();
                result.Add(new Meld(true, tiles.ToArray()));
            }

            // tiles.Count == 3
            Assert.IsTrue(tiles.Count == 3, "tiles.Count == 3");
            tiles.Sort();
            result.Add(new Meld(true, tiles[0], tiles[1], discardTile));
            result.Add(new Meld(true, tiles[0], tiles[2], discardTile));
            result.Add(new Meld(true, tiles[1], tiles[2], discardTile));
            return result;
        }

        [System.Obsolete]
        public static ISet<Meld> GetOutTurnKongs(List<Tile> handTiles, Tile discardTile)
        {
            var result = new HashSet<Meld>();
            var tiles = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank);
            if (tiles.Count < 3) return result;
            tiles.Add(discardTile);
            Assert.IsTrue(tiles.Count == 4, "tiles.Count == 4");
            result.Add(new Meld(true, tiles.ToArray()));
            return result;
        }

        [System.Obsolete]
        public static ISet<Meld> GetInTurnKongs(List<Tile> handTiles, List<Meld> openMelds, Tile lastDraw)
        {
            var result = new HashSet<Meld>();
            // find added kongs
            var addedKongIndex = openMelds.FindIndex(meld =>
                meld.Type == MeldType.Triplet && !meld.IsKong && meld.First.EqualsIgnoreColor(lastDraw));
            if (addedKongIndex >= 0)
            {
                var meld = openMelds[addedKongIndex];
                var tiles = new List<Tile>(meld.Tiles) { lastDraw };
                result.Add(new Meld(true, tiles.ToArray()));
            }

            // find concealed kongs
            var tilesCount = new Dictionary<Tile, List<Tile>>(Tile.TileIgnoreColorEqualityComparer);
            foreach (var handTile in handTiles)
            {
                if (tilesCount.ContainsKey(handTile)) tilesCount[handTile].Add(handTile);
                else tilesCount.Add(handTile, new List<Tile> { handTile });
            }

            if (tilesCount.ContainsKey(lastDraw) && tilesCount[lastDraw].Count >= 3)
            {
                tilesCount[lastDraw].Add(lastDraw);
                result.Add(new Meld(false, tilesCount[lastDraw].ToArray()));
            }

            foreach (var entry in tilesCount)
            {
                if (entry.Value.Count == 4) result.Add(new Meld(false, entry.Value.ToArray()));
            }

            return result;
        }

        [System.Obsolete]
        public static ISet<Meld> GetRichiKongs(List<Tile> handTiles, List<Meld> openMelds, Tile lastDraw)
        {
            var result = new HashSet<Meld>();
            return result;
        }

        private static string DecompositionToString(ISet<List<Meld>> decomposes)
        {
            var strings = decomposes.Select(list => $"[{string.Join(", ", list)}");
            return string.Join("; ", strings);
        }

        public static Tile GetDoraTile(Tile doraIndicator, IList<Tile> allTiles = null)
        {
            if (allTiles == null) allTiles = MahjongConstants.FullTiles;
            if (!allTiles.Contains(doraIndicator, Tile.TileIgnoreColorEqualityComparer))
            {
                Debug.LogError($"Full tile set does not contain tile {doraIndicator}, return itself");
                return doraIndicator;
            }
            int repeat;
            if (doraIndicator.Suit == Suit.Z)
            {
                if (doraIndicator.Rank <= 4) repeat = 4;
                else repeat = 3;
            }
            else repeat = 9;
            int rank = doraIndicator.Rank;
            Tile dora;
            do
            {
                rank++;
                if (rank > repeat) rank -= repeat;
                dora = new Tile(doraIndicator.Suit, rank);
            } while (!allTiles.Contains(dora, Tile.TileIgnoreColorEqualityComparer));
            return dora;
        }
    }
}