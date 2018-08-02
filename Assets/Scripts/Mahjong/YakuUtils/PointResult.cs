using System;

namespace Mahjong.YakuUtils
{
    public struct PointResult : IComparable<PointResult>
    {
        private static readonly int Mangan = 2000;
        private static readonly int Haneman = 3000;
        private static readonly int Baiman = 4000;
        private static readonly int Sanbaiman = 6000;
        private static readonly int Yakuman = 8000;
        public int Fu { get; }
        public int BasePoint { get; }
        public Level Level { get; }
        public YakuDetail YakuDetail { get; }

        public PointResult(int fu, YakuDetail detail)
        {
            Fu = fu;
            YakuDetail = detail;
            if (detail.Qingtianjing) // 青天井
            {
                int point = fu * (int) Math.Pow(2, detail.TotalValue + 2);
                if (point % 100 == 0) BasePoint = point;
                else BasePoint = (point / 100 + 1) * 100;

                Level = Level.Normal;
            }
            else // 普通规则
            {
                if (detail.IsYakuMan)
                {
                    BasePoint = Yakuman * detail.TotalValue;
                    Level = Level.Yakuman;
                }
                else
                {
                    if (detail.TotalValue >= 13)
                    {
                        BasePoint = Yakuman;
                        Level = Level.Yakuman;
                    }
                    else if (detail.TotalValue >= 11)
                    {
                        BasePoint = Sanbaiman;
                        Level = Level.Sanbaiman;
                    }
                    else if (detail.TotalValue >= 8)
                    {
                        BasePoint = Baiman;
                        Level = Level.Baiman;
                    }
                    else if (detail.TotalValue >= 6)
                    {
                        BasePoint = Haneman;
                        Level = Level.Haneman;
                    }
                    else if (detail.TotalValue >= 5)
                    {
                        BasePoint = Mangan;
                        Level = Level.Mangan;
                    }
                    else
                    {
                        int point = fu * (int) Math.Pow(2, detail.TotalValue + 2);
                        if (point >= Mangan)
                        {
                            BasePoint = Mangan;
                            Level = Level.Mangan;
                        }
                        else if (point % 100 == 0) BasePoint = point;
                        else BasePoint = (point / 100 + 1) * 100;

                        Level = Level.Normal;
                    }
                }
            }
        }

        public int CompareTo(PointResult other)
        {
            return BasePoint == other.BasePoint ? Fu.CompareTo(other.Fu) : BasePoint.CompareTo(other.BasePoint);
        }

        public static bool operator >(PointResult left, PointResult right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(PointResult left, PointResult right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator ==(PointResult left, PointResult right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(PointResult left, PointResult right)
        {
            return !(left == right);
        }
        
        private bool Equals(PointResult other)
        {
            return BasePoint == other.BasePoint && Fu == other.Fu;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PointResult && Equals((PointResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Fu;
                hashCode = (hashCode * 397) ^ BasePoint;
                hashCode = (hashCode * 397) ^ (int) Level;
                return hashCode;
            }
        }
    }

    public enum Level
    {
        Normal,
        Mangan,
        Haneman,
        Baiman,
        Sanbaiman,
        Yakuman
    }
}