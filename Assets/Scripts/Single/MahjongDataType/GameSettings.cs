using System.Collections.Generic;
using UnityEngine;

namespace Single.MahjongDataType
{
    [CreateAssetMenu(menuName = "Mahjong/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("General settings")] public int InitialPoints = 25000;
        public int RichiMortgagePoints = 1000;
        public int ExtraRoundBonusPerPlayer = 100;
        public int NotReadyPunishPerPlayer = 1000;
        public PointsToGameEnd GameEndsWhenPointsLow = PointsToGameEnd.Negative;
        public int MinimumFanConstraint = 1;
        public bool AllowDiscardSameAfterOpen = false;
        public bool AllowChows = true;
        public bool AllowRichiWhenNotReady = true;

        [Header("Tile drawing settings")] public int InitialDrawRound = 3;
        public int TilesEveryRound = 4;
        public int TilesLastRound = 1;

        [Header("Time settings")] public int BaseTurnTime = 5;
        public int BonusTurnTime = 20;

        [Header("Mahjong settings")]
        public int DiceMin = 2;
        public int DiceMax = 12;
        public int MountainReservedTiles = 14;
        public int LingshangTilesCount = 4;
        public int InitialDora = 1;
        public int MaxDora = 5;

        public bool IsChowAllowed => AllowChows;

        public Tile[] redTiles = new Tile[] {
            new Tile(Suit.M, 5, true),
            new Tile(Suit.P, 5, true),
            new Tile(Suit.S, 5, true)
        };

        public Tile[] GetAllTiles(int totalPlayers)
        {
            switch (totalPlayers)
            {
                case 2:
                    return MahjongConstants.TwoPlayerTiles.ToArray();
                case 3:
                    return MahjongConstants.ThreePlayerTiles.ToArray();
                case 4:
                    return MahjongConstants.FullTiles.ToArray();
                default:
                    Debug.LogError($"This should not happen, totalPlayers: {totalPlayers}");
                    return null;
            }
        }

        public int GetMultiplier(bool isDealer, int totalPlayers) {
            return isDealer ? 6 : 4; // this is for 4-player mahjong -- todo
        }

        public enum PointsToGameEnd {
            Negative,
            Zero,
            Never
        }
    }
}