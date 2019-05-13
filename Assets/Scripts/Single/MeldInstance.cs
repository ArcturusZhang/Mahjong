using System;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public class MeldInstance : MonoBehaviour
    {
        public OpenMeld OpenMeld;
        private TileInstance[] instances;

        private void OnEnable()
        {
            instances = new TileInstance[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                instances[i] = t.GetComponent<TileInstance>();
            }
        }

        public void SetMeld(OpenMeld meld)
        {
            OpenMeld = meld;
            if (meld.Side == MeldSide.Self)
                SelfKong();
            else
                OtherMeld();
        }

        private void SelfKong()
        {
            for (int i = 0; i < OpenMeld.Tiles.Length; i++)
            {
                instances[i].SetTile(OpenMeld.Tiles[i]);
            }
        }

        private void OtherMeld()
        {
            var index = Array.FindIndex(OpenMeld.Tiles, tile => tile.EqualsConsiderColor(OpenMeld.Tile));
            int tileCount = 1;
            for (int i = 0; i < OpenMeld.Tiles.Length; i++)
            {
                if (i == index) continue;
                instances[tileCount++].SetTile(OpenMeld.Tiles[i]);
            }
            instances[0].SetTile(OpenMeld.Tile);
        }

        // public void AddToKong(Tile addedTile)
        // {
        //     if (OpenMeld.Type != MeldType.Triplet || OpenMeld.IsKong) return;
        //     if (!OpenMeld.First.EqualsIgnoreColor(addedTile)) return;
        //     var tiles = new List<Tile>(OpenMeld.Tiles) { addedTile };
        //     Meld = new Meld(true, tiles.ToArray());
        //     // visually add a tile
        //     var tileObject = Instantiate(TileInstances[0], transform);
        //     tileObject.transform.localPosition += new Vector3(-MahjongConstants.TileWidth, 0, 0);
        // }

        public float MeldWidth
        {
            get
            {
                if (OpenMeld.Side == MeldSide.Self)
                    return 4 * MahjongConstants.TileWidth;
                if (OpenMeld.IsKong)
                    return 3 * MahjongConstants.TileWidth + MahjongConstants.TileHeight;
                return 2 * MahjongConstants.TileWidth + MahjongConstants.TileHeight;
            }
        }
    }
}