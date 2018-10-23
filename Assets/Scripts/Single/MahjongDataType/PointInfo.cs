using System;
using System.Collections.Generic;
using System.Linq;

namespace Single.MahjongDataType
{
    [Serializable]
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
                    Fan += yaku.Type == YakuType.Yakuman ? yaku.Value * YakuSettings.YakumanBaseFan : yaku.Value;
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
                BasePoint = MahjongLogic.ToNextUnit(point, 100);
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
                    point = MahjongLogic.ToNextUnit(point, 100);
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