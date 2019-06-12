using System.Collections.Generic;
using Mahjong.Model;
using UnityEngine;

namespace Mahjong.Logic
{
    public static class MahjongConstants
    {
        // game constants
        public const int WallCount = 4;
        public const int WallTilesCount = 34;
        public const int TotalTilesCount = WallCount * WallTilesCount;
        public const int TileKinds = 34;
        public const int SuitCount = 4;
        public const int FullHandTilesCount = 14;
        public const int MaxKongs = 4;
        public const int TilesPerRowInRiver = 6;
        public const int MaxRowInRiver = 3;
        public const int InitialDrawRound = 3;
        public const int TilesEveryRound = 4;
        public const int TilesLastRound = 1;
        // ui constants
        public const float TileRiverGapCol = 0.002f;
        public const float TileRiverGapRow = 0.0025f;
        public const float TileWidth = 0.026667f;
        public const float TileThickness = 0.016f;
        public const float TileHeight = 0.0393334f;
        public const float UiGap = 20;
        public const float HandTileWidth = 0.028f;
        public const float LastDrawGap = HandTileWidth / 2;
        public const float PlayerHandTilesSortDelay = 0.75f;
        public const float AutoDiscardDelayAfterRichi = 0.5f;
        public const float SummaryPanelDelayTime = 0.5f;
        public const int SummaryPanelWaitingTime = 5;
        public const float HandTilesRevealDelay = 0.5f;
        public const float FadeDuration = 1f;
        public static readonly Quaternion FacePlayer = Quaternion.Euler(270, 0, -90);
        public static readonly Quaternion FaceUp = Quaternion.Euler(-90, 0, -90);
        public static readonly Quaternion FaceDownOnWall = Quaternion.Euler(180, 180, -90);
        public static readonly Quaternion FaceUpOnWall = Quaternion.Euler(0, 180, -90);
        public static readonly Quaternion RichiTile = Quaternion.Euler(0, -90, 0);
        public static readonly Quaternion RiverTile = Quaternion.Euler(-90, -90, 0);
        public const int YakuItemColumns = 2;
        public const int FullItemCountPerColumn = 4;

        // Yaku ranks
        public const int Mangan = 2000;
        public const int Haneman = 3000;
        public const int Baiman = 4000;
        public const int Sanbaiman = 6000;
        public const int Yakuman = 8000;
        public const int YakumanBaseFan = 13;

        // Character constants
        public static readonly string[] PositionWinds = {
            "东", "南", "西", "北"
        };

        public static readonly string[] NumberCharacters =
        {
            "零", "一", "二", "三", "四", "五", "六", "七", "八", "九"
        };

        public static List<Tile> FullTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1),
            new Tile(Suit.M, 2), new Tile(Suit.M, 2), new Tile(Suit.M, 2), new Tile(Suit.M, 2),
            new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 3),
            new Tile(Suit.M, 4), new Tile(Suit.M, 4), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5),
            new Tile(Suit.M, 6), new Tile(Suit.M, 6), new Tile(Suit.M, 6), new Tile(Suit.M, 6),
            new Tile(Suit.M, 7), new Tile(Suit.M, 7), new Tile(Suit.M, 7), new Tile(Suit.M, 7),
            new Tile(Suit.M, 8), new Tile(Suit.M, 8), new Tile(Suit.M, 8), new Tile(Suit.M, 8),
            new Tile(Suit.M, 9), new Tile(Suit.M, 9), new Tile(Suit.M, 9), new Tile(Suit.M, 9),
            new Tile(Suit.P, 1), new Tile(Suit.P, 1), new Tile(Suit.P, 1), new Tile(Suit.P, 1),
            new Tile(Suit.P, 2), new Tile(Suit.P, 2), new Tile(Suit.P, 2), new Tile(Suit.P, 2),
            new Tile(Suit.P, 3), new Tile(Suit.P, 3), new Tile(Suit.P, 3), new Tile(Suit.P, 3),
            new Tile(Suit.P, 4), new Tile(Suit.P, 4), new Tile(Suit.P, 4), new Tile(Suit.P, 4),
            new Tile(Suit.P, 5), new Tile(Suit.P, 5), new Tile(Suit.P, 5), new Tile(Suit.P, 5),
            new Tile(Suit.P, 6), new Tile(Suit.P, 6), new Tile(Suit.P, 6), new Tile(Suit.P, 6),
            new Tile(Suit.P, 7), new Tile(Suit.P, 7), new Tile(Suit.P, 7), new Tile(Suit.P, 7),
            new Tile(Suit.P, 8), new Tile(Suit.P, 8), new Tile(Suit.P, 8), new Tile(Suit.P, 8),
            new Tile(Suit.P, 9), new Tile(Suit.P, 9), new Tile(Suit.P, 9), new Tile(Suit.P, 9),
            new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 1),
            new Tile(Suit.S, 2), new Tile(Suit.S, 2), new Tile(Suit.S, 2), new Tile(Suit.S, 2),
            new Tile(Suit.S, 3), new Tile(Suit.S, 3), new Tile(Suit.S, 3), new Tile(Suit.S, 3),
            new Tile(Suit.S, 4), new Tile(Suit.S, 4), new Tile(Suit.S, 4), new Tile(Suit.S, 4),
            new Tile(Suit.S, 5), new Tile(Suit.S, 5), new Tile(Suit.S, 5), new Tile(Suit.S, 5),
            new Tile(Suit.S, 6), new Tile(Suit.S, 6), new Tile(Suit.S, 6), new Tile(Suit.S, 6),
            new Tile(Suit.S, 7), new Tile(Suit.S, 7), new Tile(Suit.S, 7), new Tile(Suit.S, 7),
            new Tile(Suit.S, 8), new Tile(Suit.S, 8), new Tile(Suit.S, 8), new Tile(Suit.S, 8),
            new Tile(Suit.S, 9), new Tile(Suit.S, 9), new Tile(Suit.S, 9), new Tile(Suit.S, 9),
            new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1),
            new Tile(Suit.Z, 2), new Tile(Suit.Z, 2), new Tile(Suit.Z, 2), new Tile(Suit.Z, 2),
            new Tile(Suit.Z, 3), new Tile(Suit.Z, 3), new Tile(Suit.Z, 3), new Tile(Suit.Z, 3),
            new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4),
            new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5),
            new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6),
            new Tile(Suit.Z, 7), new Tile(Suit.Z, 7), new Tile(Suit.Z, 7), new Tile(Suit.Z, 7)
        };

        public static List<Tile> TwoPlayerTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1),
            new Tile(Suit.M, 9), new Tile(Suit.M, 9), new Tile(Suit.M, 9), new Tile(Suit.M, 9),
            new Tile(Suit.P, 1), new Tile(Suit.P, 1), new Tile(Suit.P, 1), new Tile(Suit.P, 1),
            new Tile(Suit.P, 9), new Tile(Suit.P, 9), new Tile(Suit.P, 9), new Tile(Suit.P, 9),
            new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 1),
            new Tile(Suit.S, 2), new Tile(Suit.S, 2), new Tile(Suit.S, 2), new Tile(Suit.S, 2),
            new Tile(Suit.S, 3), new Tile(Suit.S, 3), new Tile(Suit.S, 3), new Tile(Suit.S, 3),
            new Tile(Suit.S, 4), new Tile(Suit.S, 4), new Tile(Suit.S, 4), new Tile(Suit.S, 4),
            new Tile(Suit.S, 5), new Tile(Suit.S, 5), new Tile(Suit.S, 5), new Tile(Suit.S, 5),
            new Tile(Suit.S, 6), new Tile(Suit.S, 6), new Tile(Suit.S, 6), new Tile(Suit.S, 6),
            new Tile(Suit.S, 7), new Tile(Suit.S, 7), new Tile(Suit.S, 7), new Tile(Suit.S, 7),
            new Tile(Suit.S, 8), new Tile(Suit.S, 8), new Tile(Suit.S, 8), new Tile(Suit.S, 8),
            new Tile(Suit.S, 9), new Tile(Suit.S, 9), new Tile(Suit.S, 9), new Tile(Suit.S, 9),
            new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1),
            new Tile(Suit.Z, 2), new Tile(Suit.Z, 2), new Tile(Suit.Z, 2), new Tile(Suit.Z, 2),
            new Tile(Suit.Z, 3), new Tile(Suit.Z, 3), new Tile(Suit.Z, 3), new Tile(Suit.Z, 3),
            new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4),
            new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5),
            new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6),
            new Tile(Suit.Z, 7), new Tile(Suit.Z, 7), new Tile(Suit.Z, 7), new Tile(Suit.Z, 7)
        };

        public static List<Tile> ThreePlayerTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1),
            new Tile(Suit.M, 9), new Tile(Suit.M, 9), new Tile(Suit.M, 9), new Tile(Suit.M, 9),
            new Tile(Suit.P, 1), new Tile(Suit.P, 1), new Tile(Suit.P, 1), new Tile(Suit.P, 1),
            new Tile(Suit.P, 2), new Tile(Suit.P, 2), new Tile(Suit.P, 2), new Tile(Suit.P, 2),
            new Tile(Suit.P, 3), new Tile(Suit.P, 3), new Tile(Suit.P, 3), new Tile(Suit.P, 3),
            new Tile(Suit.P, 4), new Tile(Suit.P, 4), new Tile(Suit.P, 4), new Tile(Suit.P, 4),
            new Tile(Suit.P, 5), new Tile(Suit.P, 5), new Tile(Suit.P, 5), new Tile(Suit.P, 5),
            new Tile(Suit.P, 6), new Tile(Suit.P, 6), new Tile(Suit.P, 6), new Tile(Suit.P, 6),
            new Tile(Suit.P, 7), new Tile(Suit.P, 7), new Tile(Suit.P, 7), new Tile(Suit.P, 7),
            new Tile(Suit.P, 8), new Tile(Suit.P, 8), new Tile(Suit.P, 8), new Tile(Suit.P, 8),
            new Tile(Suit.P, 9), new Tile(Suit.P, 9), new Tile(Suit.P, 9), new Tile(Suit.P, 9),
            new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 1),
            new Tile(Suit.S, 2), new Tile(Suit.S, 2), new Tile(Suit.S, 2), new Tile(Suit.S, 2),
            new Tile(Suit.S, 3), new Tile(Suit.S, 3), new Tile(Suit.S, 3), new Tile(Suit.S, 3),
            new Tile(Suit.S, 4), new Tile(Suit.S, 4), new Tile(Suit.S, 4), new Tile(Suit.S, 4),
            new Tile(Suit.S, 5), new Tile(Suit.S, 5), new Tile(Suit.S, 5), new Tile(Suit.S, 5),
            new Tile(Suit.S, 6), new Tile(Suit.S, 6), new Tile(Suit.S, 6), new Tile(Suit.S, 6),
            new Tile(Suit.S, 7), new Tile(Suit.S, 7), new Tile(Suit.S, 7), new Tile(Suit.S, 7),
            new Tile(Suit.S, 8), new Tile(Suit.S, 8), new Tile(Suit.S, 8), new Tile(Suit.S, 8),
            new Tile(Suit.S, 9), new Tile(Suit.S, 9), new Tile(Suit.S, 9), new Tile(Suit.S, 9),
            new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1),
            new Tile(Suit.Z, 2), new Tile(Suit.Z, 2), new Tile(Suit.Z, 2), new Tile(Suit.Z, 2),
            new Tile(Suit.Z, 3), new Tile(Suit.Z, 3), new Tile(Suit.Z, 3), new Tile(Suit.Z, 3),
            new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4),
            new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5),
            new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6),
            new Tile(Suit.Z, 7), new Tile(Suit.Z, 7), new Tile(Suit.Z, 7), new Tile(Suit.Z, 7)
        };
    }
}