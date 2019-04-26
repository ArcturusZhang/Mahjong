using System;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public enum InTurnOperationType
    {
        Discard, Richi, Tsumo, Bei, Kong,
    }

    [Serializable]
    public struct InTurnOperation
    {
        public InTurnOperationType Type;
        public Tile Tile;
        public Meld Meld;
        public Tile[] RichiAvailableTiles;

        public override string ToString()
        {
            switch (Type)
            {
                case InTurnOperationType.Discard:
                case InTurnOperationType.Tsumo:
                    return $"Type: {Type}, Tile: {Tile}";
                case InTurnOperationType.Richi:
                    return $"Type: {Type}, Tile: {Tile}, RichiAvailableTiles: {string.Join("", RichiAvailableTiles)}";
                case InTurnOperationType.Bei:
                    return $"Type: {Type}";
                case InTurnOperationType.Kong:
                    return $"Type: {Type}, Tile: {Tile}, Meld: {Meld}";
                default:
                    Debug.LogWarning($"Unknown type: {Type}");
                    throw new Exception("This will never happen");
            }
        }
    }

    public enum OutTurnOperationType
    {
        Skip, Chow, Pong, Kong, Rong, RoundDraw,
    }

    [Serializable]
    public struct OutTurnOperation
    {
        public OutTurnOperationType Type;
        public Tile Tile;
        public Meld Meld;
        public PlayerHandData HandData;

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
                    Debug.LogWarning($"Unknown type: {Type}");
                    throw new Exception("This will never happen");
            }
        }
    }
}