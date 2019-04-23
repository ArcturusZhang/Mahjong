using System;
using Single;

namespace Multi.MahjongMessages
{
    [Serializable]
    public struct NetworkPointInfo
    {
        public int Fu;
        public YakuValue[] YakuValues;
        public int Dora;
        public int UraDora;
        public int RedDora;
        public bool IsQTJ;

        public override string ToString()
        {
            return $"Fu: {Fu}, YakuValues: {string.Join(",", YakuValues)}, "
                + $"Dora: {Dora}, UraDora: {UraDora}, RedDora: {RedDora}, IsQTJ: {IsQTJ}";
        }
    }
}