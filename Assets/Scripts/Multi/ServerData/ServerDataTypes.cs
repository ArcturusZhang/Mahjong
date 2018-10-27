using Single;
using Single.MahjongDataType;

namespace Multi.ServerData
{
    public struct InTurnOperationData
    {
        public int PlayerIndex;
        public Tile LastDraw;
        public Meld Meld;
        public InTurnOperation Operation;
        public PointInfo PointInfo;
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
        public PlayerData[] PlayerDataArray;
    }

    public struct PlayerData
    {
        public Tile[] HandTiles;
        public Tile[] OpenMelds;
        public Tile WinningTile;
        public int WinPlayerIndex;
        public HandStatus HandStatus;
        public RoundStatus RoundStatus;
        public Tile[] DoraIndicators;
        public Tile[] UraDoraIndicators;
        public int LosePlayerIndex; // valued -1 if tsumo
    }

    /// <summary>
    /// Win -- one or more player has claimed won
    /// Draw -- no one wins when tiles are drawn out
    /// </summary>
    public enum RoundEndType
    {
        Win, Draw
    }
}