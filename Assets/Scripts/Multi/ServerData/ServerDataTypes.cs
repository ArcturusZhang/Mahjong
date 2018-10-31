using System;
using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UnityEngine.Assertions;

namespace Multi.ServerData
{
    public struct InTurnOperationData
    {
        public int PlayerIndex;
        public Tile LastDraw;
        public Meld Meld;
        public InTurnOperation Operation;
        public PlayerClientData PlayerClientData;
    }

    public struct OpenMeldData
    {
        public Tile DefaultTile;
        public Tile DiscardTile;
        public Meld OpenMeld;

        public Tile[] ForbiddenTiles
        {
            get
            {
                if (!OpenMeld.ContainsConsiderColor(DiscardTile)) return null;
                if (OpenMeld.Type != MeldType.Sequence) return new[] {DiscardTile};
                int index = -1;
                for (int i = 0; i < OpenMeld.TileCount; i++)
                {
                    if (OpenMeld.Tiles[i].EqualsConsiderColor(DiscardTile))
                    {
                        index = i;
                        break;
                    }
                }

                Assert.IsTrue(index >= 0, "index >= 0");
                switch (index)
                {
                    case 0:
                        if (OpenMeld.Last.Rank != 9) return new[] {DiscardTile, OpenMeld.Last.Next};
                        break;
                    case 2:
                        if (OpenMeld.First.Rank != 1) return new[] {DiscardTile, OpenMeld.First.Previous};
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
                return new[] {DiscardTile};
            }
        }
    }

    public struct DiscardTileData
    {
        public Tile DiscardTile;
        public bool DiscardLastDraw;
        public InTurnOperation Operation;
    }

    public struct RoundEndData
    {
        public RoundEndType RoundEndType;
        public int LosePlayerIndex;
        public int TotalPlayer;
        public PlayerServerData[] PlayerServerDataArray;
    }

    public struct PlayerServerData
    {
        public Tile[] HandTiles;
        public Meld[] OpenMelds;
        public Tile WinningTile;
        public int WinPlayerIndex;
        public HandStatus HandStatus;
        public RoundStatus RoundStatus;
        public Tile[] DoraIndicators;
        public Tile[] UraDoraIndicators;
    }

    /// <summary>
    /// Win -- one or more player has claimed won
    /// Draw -- no one wins when tiles are drawn out
    /// </summary>
    public enum RoundEndType
    {
        Win,
        Draw
    }
}