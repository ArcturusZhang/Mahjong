using System;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public enum InTurnOperationType
    {
        Discard, Richi, Tsumo, Kong,
    }

    public enum OutTurnOperationType
    {
        Skip, Chow, Pong, Kong, Rong, RoundDraw,
    }

    [Serializable]
    public struct OutTurnOperation
    {
        // public int PlayerIndex;
        public OutTurnOperationType Type;
        public Tile Tile;
        public Meld Meld;

        public override string ToString()
        {
            switch (Type)
            {
                case OutTurnOperationType.Skip:
                case OutTurnOperationType.RoundDraw:
                    return $"Type: {Type}";
                case OutTurnOperationType.Chow:
                case OutTurnOperationType.Pong:
                case OutTurnOperationType.Kong:
                    return $"Type: {Type}, Tile: {Tile}, Meld: {Meld}";
                case OutTurnOperationType.Rong:
                    return $"Type: {Type}, Tile: {Tile}";
                default:
                    Debug.LogWarning(Type);
                    throw new Exception("This will never happen");
            }
        }
    }
}