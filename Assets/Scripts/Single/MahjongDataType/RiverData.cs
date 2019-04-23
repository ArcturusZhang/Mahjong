using System;

namespace Single.MahjongDataType
{
    [Serializable]
    public struct RiverTile
    {
        public Tile Tile;
        public bool IsRichi;
        public bool IsGone;
    }

    [Serializable]
    public struct RiverData
    {
        public RiverTile[] River;
    }
}