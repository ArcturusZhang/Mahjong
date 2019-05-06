using System.Collections.Generic;
using System.IO;
using UI.Attribute;
using UnityEngine;

namespace Single.MahjongDataType
{
    [CreateAssetMenu(menuName = "Mahjong/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("General settings")] public GameMode GameMode = GameMode.Normal;
        public GamePlayers GamePlayers = GamePlayers.Four;
        public RoundCount RoundCount = RoundCount.ES;
        public MinimumFanConstraintType MinimumFanConstraintType = MinimumFanConstraintType.One;
        public PointsToGameEnd PointsToGameEnd = PointsToGameEnd.Negative;
        public bool GameEndsWhenAllLastTop = true;
        public bool AllowDiscardSameAfterOpen = false;
        public bool AllowRichiWhenPointsLow = false;
        public bool AllowRichiWhenNotReady = true;
        public bool AllowChows = true;
        public bool AllowPongs = true;
        public int InitialPoints = 25000;
        public int FirstPlacePoints = 30000;
        public int RichiMortgagePoints = 1000;
        public int ExtraRoundBonusPerPlayer = 100;
        public int NotReadyPunishPerPlayer = 1000;

        public int MaxPlayer
        {
            get
            {
                switch (GamePlayers)
                {
                    case GamePlayers.Two:
                        return 2;
                    case GamePlayers.Three:
                        return 3;
                    case GamePlayers.Four:
                        return 4;
                    default:
                        Debug.LogError($"Unknown GamePlayers option: {GamePlayers}");
                        return 4;
                }
            }
        }

        public bool CheckConstraint(PointInfo point)
        {
            int baseFan = point.FanWithoutDora;
            int fan = point.TotalFan;
            int basePoint = point.BasePoint;
            switch (MinimumFanConstraintType)
            {
                case MinimumFanConstraintType.One:
                    return baseFan >= 1;
                case MinimumFanConstraintType.Two:
                    return baseFan >= 1 && fan >= 2;
                case MinimumFanConstraintType.Three:
                    return baseFan >= 1 && fan >= 3;
                case MinimumFanConstraintType.Four:
                    return baseFan >= 1 && fan >= 4;
                case MinimumFanConstraintType.Mangan:
                    return baseFan >= 1 && basePoint >= MahjongConstants.Mangan;
                case MinimumFanConstraintType.Haneman:
                    return baseFan >= 1 && basePoint >= MahjongConstants.Haneman;
                case MinimumFanConstraintType.Baiman:
                    return baseFan >= 1 && basePoint >= MahjongConstants.Baiman;
                case MinimumFanConstraintType.Yakuman:
                    return baseFan >= 1 && basePoint >= MahjongConstants.Yakuman;
                default:
                    Debug.LogError($"Unknown type {MinimumFanConstraintType}");
                    return false;
            }
        }

        [Header("Time settings")] public int BaseTurnTime = 5;
        public int BonusTurnTime = 20;

        [Header("Tile drawing settings")] public int InitialDrawRound = 3;
        public int TilesEveryRound = 4;
        public int TilesLastRound = 1;

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

        public int GetMultiplier(bool isDealer, int totalPlayers)
        {
            return isDealer ? 6 : 4; // this is for 4-player mahjong -- todo
        }

        public bool IsAllLast(int oyaIndex, int field, int totalPlayers)
        {
            return oyaIndex == totalPlayers - 1 && field == FieldThreshold - 1;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public void Save()
        {
            var json = ToJson();
            var filepath = Application.persistentDataPath + "Settings.json";
            var writer = new StreamWriter(filepath);
            writer.WriteLine(json);
            writer.Close();
        }

        private int FieldThreshold
        {
            get
            {
                switch (RoundCount)
                {
                    case RoundCount.E:
                        return 1;
                    case RoundCount.ES:
                        return 2;
                    case RoundCount.FULL:
                        return 4;
                    default:
                        Debug.LogError($"Unknown type {RoundCount}");
                        return 2;
                }
            }
        }
    }

    public enum GameMode
    {
        Normal, QTJ
    }

    public enum GamePlayers
    {
        Two, Three, Four
    }

    public enum RoundCount
    {
        E, ES, FULL
    }

    public enum MinimumFanConstraintType
    {
        One, Two, Three, Four, Mangan, Haneman, Baiman, Yakuman
    }

    public enum PointsToGameEnd
    {
        Negative, Zero, Never
    }
}