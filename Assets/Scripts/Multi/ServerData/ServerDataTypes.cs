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
}