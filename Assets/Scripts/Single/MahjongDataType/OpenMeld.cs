using System;

namespace Single.MahjongDataType
{
    [Serializable]
    public struct OpenMeld
    {
        public Meld Meld;
        public Tile DiscardTile;
        public MeldSide Side;

        public MeldType Type => Meld.Type;
        public Tile First => Meld.First;
        public bool IsKong => Meld.IsKong;
        public bool Revealed => Meld.Revealed;
        public Tile[] Tiles => Meld.Tiles;

        public override string ToString()
        {
            return $"{Meld}/{DiscardTile}/{Side}";
        }
    }

    public enum MeldSide
    {
        Left, Opposite, Right, Self
    }
}