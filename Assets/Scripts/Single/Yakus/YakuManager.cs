using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Single.MahjongDataType;
using UnityEngine;

namespace Single.Yakus
{
    public class YakuManager : MonoBehaviour
    {
        public static YakuManager Instance;

        public YakuData YakuData;

        private IEnumerable<MethodInfo> yakuMethods;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            yakuMethods = YakuData.GetType().GetMethods().Where(p =>
                p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new[]
                    {typeof(List<Meld>), typeof(Tile), typeof(HandStatus), typeof(RoundStatus)}));
        }

        public int CountFu(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus,
            IList<YakuValue> yakus)
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
                    if (!prevailingWind.EqualsIgnoreColor(selfWind) || YakuData.连风对子额外加符) fu += 2;
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

        private IList<YakuValue> CountYaku(List<Meld> decompose, Tile winningTile, HandStatus handStatus,
            RoundStatus roundStatus)
        {
            var result = new List<YakuValue>();
            if (decompose == null || decompose.Count == 0) return result;
            foreach (var yakuMethod in yakuMethods)
            {
                var value = (YakuValue) yakuMethod.Invoke(YakuData,
                    new object[] {decompose, winningTile, handStatus, roundStatus});
                if (value.Value != 0)
                {
                    result.Add(value);
                }
            }

            if (YakuData.青天井) return result;
            var hasYakuman = result.Any(yakuValue => yakuValue.Type == YakuType.Yakuman);
            return hasYakuman ? result.Where(yakuValue => yakuValue.Type == YakuType.Yakuman).ToList() : result;
        }

        public PointInfo GetPointInfo(List<Tile> handTiles, List<Meld> openMelds, Tile winningTile, 
            HandStatus handStatus, RoundStatus roundStatus, int dora = 0, int uraDora = 0, int redDora = 0)
        {
            var decomposes = MahjongLogic.Decompose(handTiles, openMelds, winningTile);
            return GetPointInfo(decomposes, winningTile, handStatus, roundStatus, dora, uraDora, redDora);
        }

        public PointInfo GetPointInfo(ISet<List<Meld>> decomposes, Tile winningTile, HandStatus handStatus,
            RoundStatus roundStatus, int dora = 0, int uraDora = 0, int redDora = 0)
        {
            var infos = new List<PointInfo>();
            foreach (var decompose in decomposes)
            {
                var yakus = CountYaku(decompose, winningTile, handStatus, roundStatus);
                var fu = CountFu(decompose, winningTile, handStatus, roundStatus, yakus);
                infos.Add(new PointInfo(fu, yakus, YakuData.青天井, dora, uraDora, redDora));
            }

            if (infos.Count == 0) return new PointInfo();
            infos.Sort();
            Debug.Log($"CountPoint: {string.Join(", ", infos.Select(info => info.ToString()))}");
            return infos[infos.Count - 1];
        }

        private void FixedUpdate()
        {
            // for test
            if (Input.GetKeyDown(KeyCode.A))
            {
                var handTiles = new List<Tile>
                {
                    new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 1),
                    new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 1), new Tile(Suit.M, 2),
                    new Tile(Suit.M, 1), new Tile(Suit.M, 6), new Tile(Suit.M, 6), new Tile(Suit.M, 2),
                    new Tile(Suit.M, 3),
                };
                var winningTile = new Tile(Suit.M, 3);
                var decomposes = MahjongLogic.Decompose(handTiles, new List<Meld>(), winningTile);
                var info = GetPointInfo(decomposes, winningTile, HandStatus.Menqing,
                    new RoundStatus {PlayerIndex = 2, RoundCount = 1, FieldCount = 1, TotalPlayer = 3});
                Debug.Log(info);
            }
        }
    }

    public struct PointInfo : IComparable<PointInfo>
    {
        private static readonly int Mangan = 2000;
        private static readonly int Haneman = 3000;
        private static readonly int Baiman = 4000;
        private static readonly int Sanbaiman = 6000;
        private static readonly int Yakuman = 8000;
        public int Fu;
        public int Fan;
        public YakuValue[] Yakus;
        public bool IsYakuman;
        public bool Is青天井;
        public int Dora;
        public int UraDora;
        public int RedDora;

        public PointInfo(int fu, IList<YakuValue> yakuValues, bool 青天井, int dora = 0, int uraDora = 0, int redDora = 0)
        {
            Fu = fu;
            Yakus = yakuValues.ToArray();
            Fan = 0;
            Is青天井 = 青天井;
            Dora = dora;
            UraDora = uraDora;
            RedDora = redDora;
            IsYakuman = false;
            if (青天井)
            {
                foreach (var yaku in yakuValues)
                {
                    Fan += yaku.Type == YakuType.Yakuman ? yaku.Value * YakuData.YakumanBaseFan : yaku.Value;
                    if (yaku.Type == YakuType.Yakuman) IsYakuman = true;
                }
            }
            else
            {
                foreach (var yaku in yakuValues)
                {
                    Fan += yaku.Value;
                    if (yaku.Type == YakuType.Yakuman) IsYakuman = true;
                }
            }

            if (yakuValues.Count == 0)
            {
                BasePoint = 0;
                TotalFan = 0;
                return;
            }

            if (青天井)
            {
                TotalFan = Fan + dora + uraDora + redDora;
                int point = Fu * (int) Math.Pow(2, TotalFan + 2);
                BasePoint = YakuManager.ToNextUnit(point, 100);
            }
            else if (IsYakuman)
            {
                BasePoint = Fan * Yakuman;
                TotalFan = Fan;
            }
            else
            {
                TotalFan = Fan + dora + uraDora + redDora;
                if (TotalFan >= 13) BasePoint = Yakuman;
                else if (TotalFan >= 11) BasePoint = Sanbaiman;
                else if (TotalFan >= 8) BasePoint = Baiman;
                else if (TotalFan >= 6) BasePoint = Haneman;
                else if (TotalFan >= 5) BasePoint = Mangan;
                else
                {
                    int point = Fu * (int) Math.Pow(2, TotalFan + 2);
                    point = YakuManager.ToNextUnit(point, 100);
                    BasePoint = Math.Min(Mangan, point);
                }
            }
        }

        public int BasePoint { get; }
        public int TotalFan { get; }

        public override string ToString()
        {
            var yakus = Yakus == null ? "" : string.Join(", ", Yakus.Select(yaku => yaku.ToString()));
            return
                $"Fu = {Fu}, Fan = {Fan}, Yakus = [{yakus}], BasePoint = {BasePoint}";
        }

        public int CompareTo(PointInfo other)
        {
            var basePointComparison = BasePoint.CompareTo(other.BasePoint);
            if (basePointComparison != 0) return basePointComparison;
            var fanComparison = TotalFan.CompareTo(other.TotalFan);
            if (fanComparison != 0) return fanComparison;
            return Fu.CompareTo(other.Fu);
        }
    }
}