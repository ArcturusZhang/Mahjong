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
        public MinimumFanConstraintType MinimumFanConstraintType = MinimumFanConstraintType.One;
        public PointsToGameEnd PointsToGameEnd = PointsToGameEnd.Negative;
        public bool AllowDiscardSameAfterOpen = false;
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
                        throw new System.ArgumentException($"Unknown GamePlayers option: {GamePlayers}");
                }
            }
        }

        public bool CheckConstraint(PointInfo point)
        {
            int fan = point.FanWithoutDora;
            int basePoint = point.BasePoint;
            switch (MinimumFanConstraintType)
            {
                case MinimumFanConstraintType.One:
                    return fan >= 1;
                case MinimumFanConstraintType.Two:
                    return fan >= 2;
                case MinimumFanConstraintType.Three:
                    return fan >= 3;
                case MinimumFanConstraintType.Four:
                    return fan >= 4;
                    // todo -- others needs a refactor on PointInfo struct
            }
            return true;
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
    }

    public enum GameMode
    {
        Normal, QTJ
    }

    public enum GamePlayers
    {
        Two, Three, Four
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