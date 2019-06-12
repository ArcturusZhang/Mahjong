using System;

namespace Mahjong.Model
{
    [Serializable]
    public struct OpenMeld
    {
        public Meld Meld;
        public Tile Tile;
        public MeldSide Side;
        public Tile Extra;
        public bool IsAdded;

        public MeldType Type => Meld.Type;
        public Tile First => Meld.First;
        public bool IsKong => Meld.IsKong;
        public bool Revealed => Meld.Revealed;
        public Tile[] Tiles => Meld.Tiles;

        public Tile[] GetForbiddenTiles(Tile tile)
        {
            return Meld.GetForbiddenTiles(tile);
        }

        public OpenMeld AddToKong(Tile extra)
        {
            return new OpenMeld
            {
                Meld = Meld.AddToKong(extra),
                Tile = Tile,
                Side = Side,
                Extra = extra,
                IsAdded = true
            };
        }

        public override string ToString()
        {
            return $"{Meld}/{Tile}/{Side}";
        }
    }

    public enum MeldSide
    {
        Left, Opposite, Right, Self
    }
}