using Multi.Messages;
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
        public PlayerClientData PlayerClientData;
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
        Win, Draw
    }
}