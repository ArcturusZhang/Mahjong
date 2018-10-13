using System;
using System.Collections.Generic;
using System.Linq;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;

namespace Single.Yakus
{
    [CreateAssetMenu(menuName = "Mahjong/Yakus")]
    public class YakuData : ScriptableObject
    {
        public static int YakumanBaseFan = 13;
        public bool 允许食断 = true;
        public bool 连风对子额外加符 = true;
        [Range(1, 2)] public int 四暗刻单骑 = 2;
        [Range(1, 2)] public int 国士无双十三面 = 2;
        [Range(1, 2)] public int 纯正九连宝灯 = 2;
        [Range(1, 2)] public int 纯绿一色 = 2;
        public bool 青天井 = false;
        public int InitialPoints = 25000;

        private static readonly int[] Greens = {19, 20, 21, 23, 25, 32};

        public YakuValue 立直(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (handStatus.HasFlag(HandStatus.Menqing) && handStatus.HasFlag(HandStatus.Richi))
                return new YakuValue {Name = "立直", Value = 1};
            if (handStatus.HasFlag(HandStatus.Menqing) && handStatus.HasFlag(HandStatus.WRichi))
                return new YakuValue {Name = "双立直", Value = 2};
            return new YakuValue();
        }

        public YakuValue 一发(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (handStatus.HasFlag(HandStatus.Menqing) &&
                (handStatus.HasFlag(HandStatus.Richi) || handStatus.HasFlag(HandStatus.WRichi)) &&
                handStatus.HasFlag(HandStatus.OneShot)) return new YakuValue {Name = "一发", Value = 1};
            return new YakuValue();
        }

        public YakuValue 自摸(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!handStatus.HasFlag(HandStatus.Menqing) || !handStatus.HasFlag(HandStatus.Tsumo))
                return new YakuValue();
            return new YakuValue {Name = "门前清自摸和", Value = 1};
        }

        public YakuValue 平和(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!handStatus.HasFlag(HandStatus.Menqing)) return new YakuValue();
            int countOfSequence = 0;
            bool twoSide = false;
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Pair && meld.Type != MeldType.Sequence) return new YakuValue();
                if (meld.Type == MeldType.Sequence)
                {
                    countOfSequence++;
                    twoSide = meld.IsTwoSideIgnoreColor(winningTile);
                }
            }

            if (countOfSequence == 4 && twoSide) return new YakuValue {Name = "平和", Value = 1};
            return new YakuValue();
        }

        public YakuValue 役牌自风(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            var tile = roundStatus.SelfWind;
            foreach (var meld in decompose)
            {
                if (meld.IdenticalTo(MeldType.Triplet, tile))
                    return new YakuValue {Name = $"自风{tile}", Value = 1};
            }

            return new YakuValue();
        }

        public YakuValue 役牌场风(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            var tile = roundStatus.PrevailingWind;
            foreach (var meld in decompose)
            {
                if (meld.IdenticalTo(MeldType.Triplet, tile))
                    return new YakuValue {Name = $"场风{tile}", Value = 1};
            }

            return new YakuValue();
        }

        public YakuValue 役牌白(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            foreach (var meld in decompose)
            {
                if (meld.IdenticalTo(MeldType.Triplet, new Tile(Suit.Z, 5)))
                    return new YakuValue {Name = "役牌白", Value = 1};
            }

            return new YakuValue();
        }

        public YakuValue 役牌发(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            foreach (var meld in decompose)
            {
                if (meld.IdenticalTo(MeldType.Triplet, new Tile(Suit.Z, 6)))
                    return new YakuValue {Name = "役牌发", Value = 1};
            }

            return new YakuValue();
        }

        public YakuValue 役牌中(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            foreach (var meld in decompose)
            {
                if (meld.IdenticalTo(MeldType.Triplet, new Tile(Suit.Z, 7)))
                    return new YakuValue {Name = "役牌中", Value = 1};
            }

            return new YakuValue();
        }

        public YakuValue 断幺九(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!允许食断 && !handStatus.HasFlag(HandStatus.Menqing)) return new YakuValue();
            foreach (var meld in decompose)
            {
                if (meld.HasYaojiu) return new YakuValue();
            }

            return new YakuValue {Name = "断幺九", Value = 1};
        }

        public YakuValue 岭上(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            return handStatus.HasFlag(HandStatus.Lingshang)
                ? new YakuValue {Name = "岭上开花", Value = 1}
                : new YakuValue();
        }

        public YakuValue 海底(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!handStatus.HasFlag(HandStatus.LastDraw)) return new YakuValue();
            return handStatus.HasFlag(HandStatus.Tsumo)
                ? new YakuValue {Name = "海底捞月", Value = 1}
                : new YakuValue {Name = "河底摸鱼", Value = 1};
        }

        public YakuValue 抢杠(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            return handStatus.HasFlag(HandStatus.RobbKong) ? new YakuValue {Name = "抢杠", Value = 1} : new YakuValue();
        }

        public YakuValue 七对子(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!handStatus.HasFlag(HandStatus.Menqing)) return new YakuValue();
            if (decompose.Count != 7) return new YakuValue();
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Pair) return new YakuValue();
            }

            return new YakuValue {Name = "七对子", Value = 2};
        }

        public YakuValue 一气(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            const int flag = 73; // binary : 1001001
            int handFlag = 0;
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Sequence) continue;
                handFlag |= 1 << Tile.GetIndex(meld.First);
            }

            Assert.IsTrue(handFlag >= 0, "Only 27 flag bits, this number should not be less than 0");

            while (handFlag > 0)
            {
                if ((handFlag & flag) == flag)
                    return new YakuValue {Name = "一气通贯", Value = handStatus.HasFlag(HandStatus.Menqing) ? 2 : 1};
                handFlag >>= 9;
            }

            return new YakuValue();
        }

        public YakuValue 三色同顺(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            const int flag = 1 + (1 << 9) + (1 << 18); // binary : 1000000001000000001
            int handFlag = 0;
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Sequence) continue;
                handFlag |= 1 << Tile.GetIndex(meld.First);
            }

            Assert.IsTrue(handFlag >= 0, "Only 27 flag bits, this number should not be less than 0");
            for (int i = 0; i < 9; i++)
            {
                if ((handFlag & flag) == flag)
                    return new YakuValue {Name = "三色同顺", Value = handStatus.HasFlag(HandStatus.Menqing) ? 2 : 1};
                handFlag >>= 1;
            }

            return new YakuValue();
        }

        public YakuValue 三色同刻(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            const int flag = 1 + (1 << 9) + (1 << 18); // binary : 1000000001000000001
            int handFlag = 0;
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Triplet) continue;
                handFlag |= 1 << Tile.GetIndex(meld.First);
            }

            Assert.IsTrue(handFlag >= 0, "Only 27 flag bits, this number should not be less than 0");
            for (int i = 0; i < 9; i++)
            {
                if ((handFlag & flag) == flag)
                    return new YakuValue {Name = "三色同刻", Value = 2};
                handFlag >>= 1;
            }

            return new YakuValue();
        }

        // todo -- this may contains bugs
        public YakuValue 全带系(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!decompose.All(meld => meld.HasYaojiu)) return new YakuValue();
            if (decompose.All(meld => meld.Suit == Suit.Z))
                return new YakuValue {Name = "字一色", Value = 1, Type = YakuType.Yakuman};
            var all = decompose.All(meld => meld.IsYaojiu);
            var any = decompose.Any(meld => meld.Suit == Suit.Z);
            if (all)
                return any
                    ? new YakuValue {Name = "混老头", Value = 2}
                    : new YakuValue {Name = "清老头", Value = 1, Type = YakuType.Yakuman};
            return any
                ? new YakuValue {Name = "混全带幺九", Value = handStatus.HasFlag(HandStatus.Menqing) ? 2 : 1}
                : new YakuValue {Name = "纯全带幺九", Value = handStatus.HasFlag(HandStatus.Menqing) ? 3 : 2};
        }

        public YakuValue 杯口系(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!handStatus.HasFlag(HandStatus.Menqing)) return new YakuValue();
            int handFlag = 0;
            int count = 0;
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Sequence) continue;
                int tileFlag = 1 << Tile.GetIndex(meld.First);
//                if ((handFlag & tileFlag) != 0) return new YakuValue {Name = "一杯口", Value = 1};
                if ((handFlag & tileFlag) != 0)
                {
                    count++;
                    handFlag ^= tileFlag; // toggle that bit, aka make it 0 again
                }
                else handFlag |= tileFlag;
            }

            Assert.IsTrue(handFlag >= 0, "Only 27 flag bits, this number should not be less than 0");
            if (count == 2) return new YakuValue {Name = "二杯口", Value = 3};
            if (count == 1) return new YakuValue {Name = "一杯口", Value = 1};

            return new YakuValue();
        }

        public YakuValue 对对和(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            int countPairs = decompose.Count(meld => meld.Type == MeldType.Pair);
            if (countPairs != 1) return new YakuValue();
            if (decompose.All(meld => meld.Type == MeldType.Pair || meld.Type == MeldType.Triplet))
                return new YakuValue {Name = "对对和", Value = 2};
            return new YakuValue();
        }

        public YakuValue 暗刻系(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            var count = decompose.Count(meld => meld.Type == MeldType.Triplet && !meld.Revealed);
            if (count < 3) return new YakuValue();
            Assert.IsTrue(count >= 3 && count <= 4, "There could not be more than 4 triplets in a complete hand");
            var winningTileInOther = decompose.Any(meld => !meld.Revealed &&
                (meld.Type == MeldType.Pair || meld.Type == MeldType.Sequence) &&
                meld.ContainsIgnoreColor(winningTile));
            if (handStatus.HasFlag(HandStatus.Tsumo))
            {
                if (count == 3) return new YakuValue {Name = "三暗刻", Value = 2};
                // count == 4
                return winningTileInOther
                    ? new YakuValue {Name = "四暗刻·单骑听", Value = 四暗刻单骑, Type = YakuType.Yakuman}
                    : new YakuValue {Name = "四暗刻", Value = 1, Type = YakuType.Yakuman};
            }

            if (count == 3 && !winningTileInOther) return new YakuValue();
            if (count == 3 && winningTileInOther) return new YakuValue {Name = "三暗刻", Value = 2};
            // count == 4
            return winningTileInOther
                ? new YakuValue {Name = "四暗刻·单骑听", Value = 四暗刻单骑, Type = YakuType.Yakuman}
                : new YakuValue {Name = "三暗刻", Value = 2};
        }

        public YakuValue 一色系(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
//            var allZ = decompose.All(meld => meld.Suit == Suit.Z);
//            if (allZ) return new YakuValue {Name = "字一色", Value = 1, Type = YakuType.Yakuman};
            // this has already been handled in 全带系 
            var allM = decompose.All(meld => meld.Suit == Suit.M || meld.Suit == Suit.Z);
            var allS = decompose.All(meld => meld.Suit == Suit.S || meld.Suit == Suit.Z);
            var allP = decompose.All(meld => meld.Suit == Suit.P || meld.Suit == Suit.Z);
            var single = allM || allS || allP;
            if (!single) return new YakuValue();
            var anyZ = decompose.Any(meld => meld.Suit == Suit.Z);
            return anyZ
                ? new YakuValue {Name = "混一色", Value = handStatus.HasFlag(HandStatus.Menqing) ? 3 : 2}
                : new YakuValue {Name = "清一色", Value = handStatus.HasFlag(HandStatus.Menqing) ? 6 : 5};
        }

        public YakuValue 杠子系(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            int count = decompose.Count(meld => meld.IsKong);
            if (count < 3) return new YakuValue();
            if (count == 3) return new YakuValue {Name = "三杠子", Value = 2};
            Assert.AreEqual(count, 4);
            return new YakuValue {Name = "四杠子", Value = 1, Type = YakuType.Yakuman};
        }

        public YakuValue 三元系(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            const int flag = 7;
            int tripletFlag = 0;
            int pairFlag = 0;
            foreach (var meld in decompose)
            {
                if (meld.Suit != Suit.Z || meld.First.Rank <= 4) continue;
                if (meld.Type == MeldType.Triplet)
                    tripletFlag |= 1 << (meld.First.Rank - 5);
                if (meld.Type == MeldType.Pair)
                    pairFlag |= 1 << (meld.First.Rank - 5);
            }

            if (tripletFlag == flag) return new YakuValue {Name = "大三元", Value = 1, Type = YakuType.Yakuman};
            if ((tripletFlag & pairFlag) == flag && pairFlag != flag) return new YakuValue {Name = "小三元", Value = 2};
            return new YakuValue();
        }

        public YakuValue 天地和(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!handStatus.HasFlag(HandStatus.Tsumo) || !handStatus.HasFlag(HandStatus.Menqing) ||
                !handStatus.HasFlag(HandStatus.FirstRound))
                return new YakuValue();
            return roundStatus.PlayerIndex == roundStatus.RoundCount - 1
                ? new YakuValue {Name = "天和", Value = 1, Type = YakuType.Yakuman}
                : new YakuValue {Name = "地和", Value = 1, Type = YakuType.Yakuman};
        }

        public YakuValue 国士(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (decompose.Count != 13) return new YakuValue();
            var pair = decompose.First(meld => meld.Type == MeldType.Pair);
            return pair.ContainsIgnoreColor(winningTile)
                ? new YakuValue {Name = "国士无双十三面", Value = 国士无双十三面, Type = YakuType.Yakuman}
                : new YakuValue {Name = "国士无双", Value = 1, Type = YakuType.Yakuman};
        }

        public YakuValue 九莲(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            if (!handStatus.HasFlag(HandStatus.Menqing)) return new YakuValue();
            var first = decompose[0];
            var all = decompose.All(meld => meld.Suit == first.Suit);
            if (!all) return new YakuValue();
            var counts = new int[9];
            foreach (var meld in decompose)
            {
                foreach (var tile in meld.Tiles)
                {
                    counts[tile.Rank - 1]++;
                }
            }

            if (counts[0] < 3 || counts[8] < 3) return new YakuValue();

            for (int i = 1; i < 8; i++)
            {
                if (counts[i] < 1) return new YakuValue();
            }

            return counts[winningTile.Rank - 1] == 2 || counts[winningTile.Rank - 1] == 4
                ? new YakuValue {Name = "纯正九连宝灯", Value = 纯正九连宝灯, Type = YakuType.Yakuman}
                : new YakuValue {Name = "九连宝灯", Value = 1, Type = YakuType.Yakuman};
        }

        public YakuValue 四喜系(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            const int flag = 15;
            int tripletFlag = 0;
            int pairFlag = 0;
            foreach (var meld in decompose)
            {
                if (meld.Suit != Suit.Z || meld.First.Rank > 4) continue;
                if (meld.Type == MeldType.Triplet)
                    tripletFlag |= 1 << (meld.First.Rank - 1);
                if (meld.Type == MeldType.Pair)
                    pairFlag |= 1 << (meld.First.Rank - 1);
            }

            if (tripletFlag == flag) return new YakuValue {Name = "大四喜", Value = 2, Type = YakuType.Yakuman};
            if ((tripletFlag & pairFlag) == flag && pairFlag != flag)
                return new YakuValue {Name = "小四喜", Value = 1, Type = YakuType.Yakuman};
            return new YakuValue();
        }

        public YakuValue 绿一色(List<Meld> decompose, Tile winningTile, HandStatus handStatus, RoundStatus roundStatus)
        {
            var counts = MahjongLogic.CountTiles(decompose);
            for (int i = 0; i < counts.Length; i++)
            {
                if (Greens.Contains(i)) continue;
                if (counts[i] > 0) return new YakuValue();
            }

            return counts[Greens[Greens.Length - 1]] == 0
                ? new YakuValue {Name = "纯绿一色", Value = 纯绿一色, Type = YakuType.Yakuman}
                : new YakuValue {Name = "绿一色", Value = 1, Type = YakuType.Yakuman};
        }

        // todo -- more to add
    }

    public struct YakuValue
    {
        public string Name;
        public int Value;
        public YakuType Type;

        public YakuValue SetValue(int value)
        {
            return new YakuValue
            {
                Name = Name, Value = value, Type = Type
            };
        }

        public override string ToString()
        {
            return $"Name = {Name}, Value = {Value}, Type = {Type}";
        }
    }

    public struct RoundStatus
    {
        public int PlayerIndex;
        public int RoundCount;
        public int FieldCount;
        public int TotalPlayer;

        public Tile SelfWind
        {
            get
            {
                int index = PlayerIndex - (RoundCount - 1) + 1;
                if (index <= 0) index += TotalPlayer;
                Assert.IsTrue(index > 0 && index <= 4, "Self wind should be one of E, S, W, N");
                return new Tile(Suit.Z, index);
            }
        }

        public Tile PrevailingWind => new Tile(Suit.Z, FieldCount);
    }

    [Flags]
    public enum HandStatus
    {
        Nothing = 1 << 0,
        Menqing = 1 << 1,
        Tsumo = 1 << 2,
        Richi = 1 << 3,
        WRichi = 1 << 4,
        FirstRound = 1 << 5,
        Lingshang = 1 << 6,
        LastDraw = 1 << 7,
        RobbKong = 1 << 8,
        OneShot = 1 << 9
    }

    public enum YakuType
    {
        Normal = 0,
        Yakuman = 1
    }
}