using System;
using Mahjong.Logic;

namespace GamePlay.Server.Model
{
    [Serializable]
    public struct NetworkPointInfo
    {
        public int Fu;
        public YakuValue[] YakuValues;
        public int Dora;
        public int UraDora;
        public int RedDora;
        public int BeiDora;
        public bool IsQTJ;

        public override string ToString()
        {
            return $"Fu: {Fu}, YakuValues: {string.Join(",", YakuValues)}, "
                + $"Dora: {Dora}, UraDora: {UraDora}, RedDora: {RedDora}, BeiDora: {BeiDora}, IsQTJ: {IsQTJ}";
        }
    }
}