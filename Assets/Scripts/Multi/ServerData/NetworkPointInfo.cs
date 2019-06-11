using System;
using Single;

namespace Multi.ServerData
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