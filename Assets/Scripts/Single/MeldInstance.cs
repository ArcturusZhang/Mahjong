using System;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public class MeldInstance : MonoBehaviour
    {
        public MeldInstanceType InstanceType;
        public Meld Meld;

        public TileInstance[] TileInstances;

        public void SetMeld(Meld meld, Tile discardTile)
        {
            Meld = meld;
            var index = Array.FindIndex(meld.Tiles, tile => tile.EqualsConsiderColor(discardTile));
            int tileCount = 1;
            for (int i = 0; i < meld.Tiles.Length; i++)
            {
                if (i == index) continue;
                TileInstances[tileCount++].SetTile(meld.Tiles[i]);
            }

            TileInstances[0].SetTile(discardTile);
        }

        public void AddToKong(Tile addedTile)
        {
            if (Meld.Type != MeldType.Triplet || Meld.IsKong) return;
            if (!Meld.First.EqualsIgnoreColor(addedTile)) return;
            var tiles = new List<Tile>(Meld.Tiles) {addedTile};
            Meld = new Meld(true, tiles.ToArray());
            // visually add a tile
            var tileObject = Instantiate(TileInstances[0], transform);
            tileObject.transform.localPosition += new Vector3(-MahjongConstants.TileWidth, 0, 0);
        }

        public float MeldWidth
        {
            get
            {
                switch (InstanceType)
                {
                    case MeldInstanceType.Left:
                    case MeldInstanceType.Opposite:
                    case MeldInstanceType.Right:
                        return 2 * MahjongConstants.TileWidth + MahjongConstants.TileHeight;
                    case MeldInstanceType.LeftKong:
                    case MeldInstanceType.OppositeKong:
                    case MeldInstanceType.RightKong:
                        return 3 * MahjongConstants.TileWidth + MahjongConstants.TileHeight;
                    case MeldInstanceType.SelfKong:
                        return 4 * MahjongConstants.TileWidth;
                    default:
                        throw new ArgumentException("Will not happen");
                }
            }
        }
    }

    public enum MeldInstanceType
    {
        Left,
        Opposite,
        Right,
        LeftKong,
        OppositeKong,
        RightKong,
        SelfKong
    }
}