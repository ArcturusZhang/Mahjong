using System;
using Single.MahjongDataType;

namespace Single
{
    [Flags]
    public enum InTurnOperationType
    {
        Discard = 1 << 0,
        Richi = 1 << 1,
        Tsumo = 1 << 2,
        Kong = 1 << 3,
    }

    [Flags]
    public enum OutTurnOperationType
    {
        Skip = 1 << 0,
        Chow = 1 << 1,
        Pong = 1 << 2,
        Kong = 1 << 3,
        Rong = 1 << 4,
        RoundEnd = 1 << 5
    }

    [Serializable]
    public struct OutTurnOperation
    {
        public OutTurnOperationType Type;
        public Tile Tile;
        public Meld Meld;

        public override string ToString()
        {
            switch (Type)
            {
                case OutTurnOperationType.Skip:
                    return $"Type: {Type}";
                case OutTurnOperationType.Chow:
                case OutTurnOperationType.Pong:
                case OutTurnOperationType.Kong:
                    return $"Type: {Type}, Tile: {Tile}, Meld: {Meld}";
                case OutTurnOperationType.Rong:
                    return $"Type: {Type}, Tile: {Tile}";
                default:
                    throw new Exception("This will never happen");
            }
        }
    }
}