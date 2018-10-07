using System;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public static class MahjongConstants
    {
        public const int WallCount = 4;
        public const int WallTilesCount = 34;
        public const int TotalTilesCount = WallCount * WallTilesCount;
        public const int TileKinds = 34;
        public const int SuitCount = 4;
        public const int CompleteHandTilesCount = 13;
        public const int FullHandTilesCount = 14;
        public const float Gap = 0.0005f;
        public const float TileWidth = 0.026667f;
        public const float TileThickness = 0.016f;
        public const float TileHeight = 0.0393334f;
        public const float UiGap = 20;
        public static readonly Quaternion FacePlayer = Quaternion.Euler(270, 0, -90);
        public static readonly Quaternion FaceUp = Quaternion.Euler(-90, 0, -90);

        public static readonly IEqualityComparer<Tile>
            TileConcernColorEqualityComparer = new TileConcernColorEqualityComparerImpl();
        public static readonly IEqualityComparer<Tile>
            TileIgnoreColorEqualityComparer = new TileIgnoreColorEqualityComparerImpl();

        public static int RepeatIndex(int index, int length)
        {
            while (index >= length) index -= length;
            while (index < 0) index += length;
            return index;
        }

        public static string GetTileName(Tile tile)
        {
            int index = tile.IsRed ? 0 : tile.Rank;
            return index + tile.Suit.ToString().ToLower();
        }

        private static readonly IDictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

        public static Texture2D GetTileTexture(Tile tile)
        {
            var key = GetTileName(tile);
            Texture2D texture;
            if (!textureDict.ContainsKey(key))
            {
                texture = Resources.Load<Texture2D>($"Textures/{key}");
                textureDict.Add(key, texture);
            }

//            if (textureDict[key] == null) textureDict[key] = Resources.Load<Texture2D>($"Textures/{key}");
            return textureDict[key];
        }

        private struct TileConcernColorEqualityComparerImpl : IEqualityComparer<Tile>
        {
            public bool Equals(Tile x, Tile y)
            {
                return x.EqualsConsiderColor(y);
            }

            public int GetHashCode(Tile obj)
            {
                int hash = (int) obj.Suit;
                hash = hash * 31 + obj.Rank;
                hash = hash * 31 + (obj.IsRed ? 1 : 0);
                return hash;
            }
        }

        private struct TileIgnoreColorEqualityComparerImpl : IEqualityComparer<Tile>
        {
            public bool Equals(Tile x, Tile y)
            {
                return x.EqualsIgnoreColor(y);
            }

            public int GetHashCode(Tile obj)
            {
                int hash = (int) obj.Suit;
                hash = hash * 31 + obj.Rank;
                return hash;
            }
        }
    }

    public enum GameTurnState
    {
        RoundPrepare,
        RoundStart,
        PlayerDrawTile,
        PlayerInTurn,
        PlayerDiscardTile,
        PlayerOutTurnOperation,
        RoundEnd
    }

    [Flags]
    public enum InTurnOperation
    {
        Discard = 1 << 0,
        Richi = 1 << 1,
        Tsumo = 1 << 2,
        ConcealedKong = 1 << 3,
        AddedKong = 1 << 4
    }

    [Flags]
    public enum OutTurnOperation
    {
        Skip = 1 << 0,
        Chow = 1 << 1,
        Pong = 1 << 2,
        Kong = 1 << 3,
        Rong = 1 << 4,
    }
}