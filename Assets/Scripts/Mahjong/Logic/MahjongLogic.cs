using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;


namespace Mahjong.Logic
{
    public static class MahjongLogic
    {
        public readonly static IEnumerable<MethodInfo> YakuMethods;

        static MahjongLogic()
        {
            YakuMethods = typeof(YakuLogic).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(p =>
                p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new[]
                    {typeof(List<Meld>), typeof(Tile), typeof(HandStatus), typeof(RoundStatus), typeof(GameSetting)}));
        }

        public static int CountFu(List<Meld> decompose, Tile winningTile, HandStatus handStatus,
            RoundStatus roundStatus, IList<YakuValue> yakus, GameSetting settings)
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
                        settings.连风对子额外加符) fu += 2;
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
            RoundStatus roundStatus, GameSetting yakuSettings, bool isQTJ)
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

            if (isQTJ) return result;
            var hasYakuman = result.Any(yakuValue => yakuValue.Type == YakuType.Yakuman);
            return hasYakuman ? result.Where(yakuValue => yakuValue.Type == YakuType.Yakuman).ToList() : result;
        }

        public static PointInfo GetPointInfo(Tile[] handTiles, Meld[] openMelds, Tile winningTile,
            HandStatus handStatus, RoundStatus roundStatus, GameSetting settings, bool isQTJ,
            Tile[] doraTiles = null, Tile[] uraDoraTiles = null, int beiDora = 0)
        {
            var decomposes = Decompose(handTiles, openMelds, winningTile);
            if (decomposes.Count == 0) return new PointInfo();
            // count dora
            int dora = CountDora(handTiles, openMelds, winningTile, doraTiles);
            int uraDora = 0;
            if (handStatus.HasFlag(HandStatus.Richi) || handStatus.HasFlag(HandStatus.WRichi))
            {
                uraDora = CountDora(handTiles, openMelds, winningTile, uraDoraTiles);
            }

            int redDora = CountRed(handTiles, openMelds, winningTile);
            return GetPointInfo(decomposes, winningTile, handStatus, roundStatus, settings, isQTJ, dora, uraDora, redDora, beiDora);
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
            foreach (var tile in handTiles)
            {
                if (tile.EqualsIgnoreColor(dora)) count++;
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
            RoundStatus roundStatus, GameSetting settings, bool isQTJ, int dora, int uraDora, int redDora, int beiDora)
        {
            Debug.Log($"GetPointInfo method, parameters: \ndecomposes: {DecompositionToString(decomposes)}, "
                      + $"winningTile: {winningTile}, handStatus: {handStatus}, roundStatus: {roundStatus}, "
                      + $"dora: {dora}, uraDora: {uraDora}, redDora: {redDora}, beiDora: {beiDora}");
            var infos = new List<PointInfo>();
            foreach (var decompose in decomposes)
            {
                var yakus = CountYaku(decompose, winningTile, handStatus, roundStatus, settings, isQTJ);
                var fu = CountFu(decompose, winningTile, handStatus, roundStatus, yakus, settings);
                if (yakus.Count == 0) continue;
                var info = new PointInfo(fu, yakus, isQTJ, dora, uraDora, redDora, beiDora);
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

        private static ISet<List<Meld>> Decompose(IList<Tile> handTiles, IList<Meld> openMelds, Tile tile)
        {
            var decompose = new HashSet<List<Meld>>(new MeldListEqualityComparer());
            int count = handTiles.Count;
            if (count % 3 != 1) return decompose;
            var allTiles = new List<Tile>(handTiles) { tile };
            var hand = CountTiles(allTiles);
            AnalyzeHand(hand, decompose);
            if (decompose.Count == 0) return decompose;
            var result = new HashSet<List<Meld>>(new MeldListEqualityComparer());
            foreach (var sub in decompose)
            {
                if (openMelds != null) sub.AddRange(openMelds);
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

        public static IDictionary<Tile, IList<Tile>> DiscardForReady(IList<Tile> handTiles, Tile? lastDraw)
        {
            var list = new List<Tile>(handTiles);
            if (lastDraw != null) list.Add((Tile)lastDraw);
            Dictionary<Tile, IList<Tile>> result = null;
            for (int i = 0; i < list.Count; i++)
            {
                var first = list[0];
                list.RemoveAt(0);
                var waitingList = WinningTiles(list, null);
                if (waitingList.Count > 0)
                {
                    if (result == null) result = new Dictionary<Tile, IList<Tile>>(Tile.TileIgnoreColorEqualityComparer);
                    if (!result.ContainsKey(first)) result.Add(first, waitingList);
                }
                list.Add(first);
            }
            return result;
        }

        public static bool IsReady(IList<Tile> handTiles, IList<Meld> openMelds)
        {
            return WinningTiles(handTiles, openMelds).Count > 0;
        }

        public static bool Test9KindsOfOrphans(IList<Tile> handTiles, Tile lastDraw)
        {
            var set = new HashSet<Tile>(handTiles, Tile.TileIgnoreColorEqualityComparer);
            set.Add(lastDraw);
            int count = set.Count(tile => tile.IsYaojiu);
            return count >= 9;
        }

        private static void AnalyzeHand(int[] hand, HashSet<List<Meld>> decompose)
        {
            AnalyzeNormal(hand, decompose);
            Analyze7Pairs(hand, decompose);
            Analyze13Orphans(hand, decompose);
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

        public static int[] CountTiles(IList<Meld> meldList)
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

        private static int[] CountTiles(IList<Tile> tiles)
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

        public static bool TestRichi(IList<Tile> handTiles, IList<Meld> openMelds, Tile lastDraw,
            bool allowNotReady, out IList<Tile> availableTiles)
        {
            // test if menqing
            if (!TestMenqing(openMelds))
            {
                availableTiles = new List<Tile>();
                return false;
            }
            if (allowNotReady)
            {
                // return every hand tile as candidates
                availableTiles = new List<Tile>(handTiles);
                availableTiles.Add(lastDraw);
                return true;
            }
            var tiles = new List<Tile>(handTiles) { lastDraw };
            availableTiles = new List<Tile>();
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[0];
                tiles.RemoveAt(0);
                if (IsReady(tiles, openMelds)) availableTiles.Add(tile);
                tiles.Add(tile);
            }
            return availableTiles.Count > 0;
        }

        public static IEnumerable<OpenMeld> GetKongs(IList<Tile> handTiles, Tile discardTile, MeldSide side)
        {
            var result = new HashSet<Meld>(Meld.MeldConsiderColorEqualityComparer);
            var tileList = new List<Tile>(handTiles) { discardTile };
            var handCount = CountTiles(handTiles);
            int index = Tile.GetIndex(discardTile);
            if (handCount[index] == 3)
            {
                var tiles = tileList.FindAll(t => Tile.GetIndex(t) == index);
                result.Add(new Meld(true, tiles.ToArray()));
            }
            return result.Select(meld => new OpenMeld
            {
                Meld = meld,
                Tile = discardTile,
                Side = side
            });
        }

        public static IEnumerable<OpenMeld> GetSelfKongs(IList<Tile> handTiles, Tile lastDraw)
        {
            var result = new HashSet<Meld>(Meld.MeldConsiderColorEqualityComparer);
            var testTiles = new List<Tile>(handTiles) { lastDraw };
            var handCount = CountTiles(testTiles);
            for (int i = 0; i < handCount.Length; i++)
            {
                Assert.IsTrue(handCount[i] <= 4);
                if (handCount[i] == 4)
                {
                    var tiles = testTiles.FindAll(tile => Tile.GetIndex(tile) == i);
                    result.Add(new Meld(false, tiles.ToArray()));
                }
            }
            return result.Select(meld => new OpenMeld
            {
                Meld = meld,
                Side = MeldSide.Self
            });
        }

        public static IEnumerable<OpenMeld> GetAddKongs(IList<Tile> handTiles, IList<OpenMeld> openMelds, Tile lastDraw)
        {
            var result = new List<OpenMeld>();
            var testTiles = new List<Tile>(handTiles) { lastDraw };
            var pongs = openMelds.Where(meld => meld.Type == MeldType.Triplet && !meld.IsKong);
            foreach (var pong in pongs)
            {
                var extraIndex = testTiles.FindIndex(t => t.EqualsIgnoreColor(pong.First));
                if (extraIndex < 0) continue;
                var extraTile = testTiles[extraIndex];
                result.Add(pong.AddToKong(extraTile));
            }
            return result;
        }

        public static IEnumerable<OpenMeld> GetRichiKongs(IList<Tile> handTiles, Tile lastDraw)
        {
            var winningTiles = WinningTiles(handTiles, null);
            foreach (var winningTile in winningTiles)
            {
                var decomposes = Decompose(handTiles, null, winningTile);
                if (!decomposes.All(list => list.Exists(m => m.Type == MeldType.Triplet && m.First.EqualsIgnoreColor(lastDraw))))
                    return new List<OpenMeld>();
            }
            var tiles = new List<Tile>();
            for (int i = 0; i < handTiles.Count; i++)
            {
                if (handTiles[i].EqualsIgnoreColor(lastDraw)) tiles.Add(handTiles[i]);
            }
            tiles.Add(lastDraw);
            Assert.AreEqual(tiles.Count, 4);
            return new List<OpenMeld> {
                new OpenMeld {
                    Meld = new Meld(false, tiles.ToArray()),
                    Side = MeldSide.Self
                }
            };
        }

        public static IEnumerable<OpenMeld> GetPongs(IList<Tile> handTiles, Tile discardTile, MeldSide side)
        {
            var result = new HashSet<Meld>(Meld.MeldConsiderColorEqualityComparer);
            var handTileList = new List<Tile>(handTiles);
            var particularTiles = handTileList.FindAll(tile => tile.EqualsIgnoreColor(discardTile));
            var combination = Combination(particularTiles, 2);
            foreach (var item in combination)
            {
                item.Add(discardTile);
                result.Add(new Meld(true, item.ToArray()));
            }
            return result.Select(meld => new OpenMeld
            {
                Meld = meld,
                Tile = discardTile,
                Side = side
            });
        }

        public static IList<List<T>> Combination<T>(IList<T> list, int count)
        {
            var result = new List<List<T>>();
            if (count <= 0 || count > list.Count) return result;
            CombinationBackTrack(list, count, 0, new List<T>(), result);
            return result;
        }

        private static void CombinationBackTrack<T>(IList<T> list, int count, int start, IList<T> current, IList<List<T>> result)
        {
            // exits
            if (current.Count == count)
            {
                result.Add(new List<T>(current));
                return;
            }
            for (int i = start; i < list.Count; i++)
            {
                current.Add(list[i]);
                CombinationBackTrack(list, count, i + 1, current, result);
                // back track
                current.RemoveAt(current.Count - 1);
            }
        }

        public static IEnumerable<OpenMeld> GetChows(IList<Tile> handTiles, Tile discardTile, MeldSide side)
        {
            var result = new HashSet<Meld>(Meld.MeldConsiderColorEqualityComparer);
            if (discardTile.Suit == Suit.Z) return new List<OpenMeld>();
            var handTileList = new List<Tile>(handTiles);
            GetChows1(handTileList, discardTile, result);
            GetChows2(handTileList, discardTile, result);
            GetChows3(handTileList, discardTile, result);
            return result.Select(meld => new OpenMeld
            {
                Meld = meld,
                Tile = discardTile,
                Side = side
            });
        }

        private static void GetChows1(List<Tile> handTiles, Tile discardTile, HashSet<Meld> result)
        {
            Tile first, second;
            if (Tile.TryTile(discardTile.Suit, discardTile.Rank - 2, out first) && Tile.TryTile(discardTile.Suit, discardTile.Rank - 1, out second))
            {
                var firstTiles = handTiles.FindAll(tile => tile.EqualsIgnoreColor(first));
                if (firstTiles.Count == 0) return;
                var secondTiles = handTiles.FindAll(tile => tile.EqualsIgnoreColor(second));
                if (secondTiles.Count == 0) return;
                foreach (var pair in CartesianJoin(firstTiles, secondTiles))
                {
                    result.Add(new Meld(true, pair.Key, pair.Value, discardTile));
                }
            }
        }

        private static void GetChows2(List<Tile> handTiles, Tile discardTile, HashSet<Meld> result)
        {
            Tile first, second;
            if (Tile.TryTile(discardTile.Suit, discardTile.Rank - 1, out first) && Tile.TryTile(discardTile.Suit, discardTile.Rank + 1, out second))
            {
                var firstTiles = handTiles.FindAll(tile => tile.EqualsIgnoreColor(first));
                if (firstTiles.Count == 0) return;
                var secondTiles = handTiles.FindAll(tile => tile.EqualsIgnoreColor(second));
                if (secondTiles.Count == 0) return;
                foreach (var pair in CartesianJoin(firstTiles, secondTiles))
                {
                    result.Add(new Meld(true, pair.Key, pair.Value, discardTile));
                }
            }
        }

        private static void GetChows3(List<Tile> handTiles, Tile discardTile, HashSet<Meld> result)
        {
            Tile first, second;
            if (Tile.TryTile(discardTile.Suit, discardTile.Rank + 1, out first) && Tile.TryTile(discardTile.Suit, discardTile.Rank + 2, out second))
            {
                var firstTiles = handTiles.FindAll(tile => tile.EqualsIgnoreColor(first));
                if (firstTiles.Count == 0) return;
                var secondTiles = handTiles.FindAll(tile => tile.EqualsIgnoreColor(second));
                if (secondTiles.Count == 0) return;
                foreach (var pair in CartesianJoin(firstTiles, secondTiles))
                {
                    result.Add(new Meld(true, pair.Key, pair.Value, discardTile));
                }
            }
        }

        private static IEnumerable<KeyValuePair<T, T>> CartesianJoin<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            var result = first.SelectMany(x => second, (x, y) => new KeyValuePair<T, T>(x, y));
            return result;
        }

        private static string DecompositionToString(ISet<List<Meld>> decomposes)
        {
            var strings = decomposes.Select(list => $"[{string.Join(", ", list)}");
            return string.Join("; ", strings);
        }

        public static bool TestDiscardZhenting(IList<Tile> handTiles, List<RiverTile> riverTiles)
        {
            var winningTiles = WinningTiles(handTiles, null);
            foreach (var winningTile in winningTiles)
            {
                int index = riverTiles.FindIndex(riverTile => riverTile.Tile.EqualsIgnoreColor(winningTile));
                if (index >= 0) return true;
            }
            return false;
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

        public static IOrderedEnumerable<KeyValuePair<int, int>> SortPointsAndPlaces(IEnumerable<int> points)
        {
            return points.Select((p, i) => new KeyValuePair<int, int>(p, i))
                .OrderBy(key => key, new PointsComparer());
        }

        private struct PointsComparer : IComparer<KeyValuePair<int, int>>
        {
            public int Compare(KeyValuePair<int, int> point1, KeyValuePair<int, int> point2)
            {
                var pointsCmp = point1.Key.CompareTo(point2.Key);
                if (pointsCmp != 0) return -pointsCmp;
                return point1.Value.CompareTo(point2.Value);
            }
        }
    }
}