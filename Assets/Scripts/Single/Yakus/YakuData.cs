using System;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;

namespace Single.Yakus
{
    [CreateAssetMenu(menuName = "Mahjong/Yakus")]
    public class YakuData : ScriptableObject
    {
        public bool allowShiduan = true;

        public YakuValue 立直(List<Meld> decompose, Tile winningTile, HandStatus handStatus, PlayerStatus playerStatus)
        {
            var none = new YakuValue {Name = "立直"};
            if (handStatus.HasFlag(HandStatus.Menqing) && handStatus.HasFlag(HandStatus.Richi)) return none.SetValue(1);
            if (handStatus.HasFlag(HandStatus.Menqing) && handStatus.HasFlag(HandStatus.WRichi))
                return new YakuValue {Name = "双立直", Value = 2};
            return none;
        }

        public YakuValue 门前清自摸和(List<Meld> decompose, Tile winningTile, HandStatus handStatus,
            PlayerStatus playerStatus)
        {
            var none = new YakuValue {Name = "门前清自摸和"};
            if (!handStatus.HasFlag(HandStatus.Menqing) || !handStatus.HasFlag(HandStatus.Tsumo)) return none;
            return none.SetValue(1);
        }

        public YakuValue 断幺(List<Meld> decompose, Tile winningTile, HandStatus handStatus, PlayerStatus playerStatus)
        {
            var none = new YakuValue {Name = "断幺"};
            if (!allowShiduan && !handStatus.HasFlag(HandStatus.Menqing)) return none;
            foreach (var meld in decompose)
            {
                if (meld.HasYaojiu) return none;
            }

            return none.SetValue(1);
        }

        public YakuValue 平和(List<Meld> decompose, Tile winningTile, HandStatus handStatus, PlayerStatus playerStatus)
        {
            var none = new YakuValue {Name = "平和"};
            if (!handStatus.HasFlag(HandStatus.Menqing)) return none;
            int countOfSequence = 0;
            bool twoSide = false;
            foreach (var meld in decompose)
            {
                if (meld.Type != MeldType.Pair && meld.Type != MeldType.Sequence) return none;
                if (meld.Type == MeldType.Sequence)
                {
                    countOfSequence++;
                    if (meld.First.EqualsIgnoreColor(winningTile) || meld.First.EqualsIgnoreColor(winningTile))
                        twoSide = true;
                }
            }

            if (countOfSequence == 4 && twoSide) return none.SetValue(1);
            return none;
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

    public struct PlayerStatus
    {
        public int StartIndex;
        public int RoundCount;
        public int FieldCount;
        public bool RichiRound;
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
    }

    public enum YakuType
    {
        Normal = 0,
        Yakuman = 1
    }
}