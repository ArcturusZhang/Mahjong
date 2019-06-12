using System;
using System.Text;

namespace Mahjong.Model
{
    [Serializable]
    public struct RiverTile
    {
        public Tile Tile;
        public bool IsRichi;
        public bool IsGone;

        public override string ToString()
        {
            var builder = new StringBuilder(Tile.ToString());
            if (IsRichi)
                builder.Append("R");
            if (IsGone)
                builder.Append("G");
            return builder.ToString();
        }
    }

    [Serializable]
    public struct RiverData
    {
        public RiverTile[] River;

        public override string ToString()
        {
            return string.Join("", River);
        }
    }
}