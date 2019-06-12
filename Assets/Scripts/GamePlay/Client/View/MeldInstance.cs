using System;
using Mahjong.Logic;
using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Client.View
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
            else if (meld.IsAdded)
                AddedKong();
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
            for (int i = OpenMeld.Tiles.Length; i < instances.Length; i++)
            {
                instances[i].gameObject.SetActive(false);
            }
        }

        public void AddedKong()
        {
            var index1 = Array.FindIndex(OpenMeld.Tiles, tile => tile.EqualsConsiderColor(OpenMeld.Tile));
            var index2 = Array.FindIndex(OpenMeld.Tiles, tile => tile.EqualsConsiderColor(OpenMeld.Extra));
            int tileCount = 1;
            for (int i = 0; i < OpenMeld.Tiles.Length; i++)
            {
                if (i == index1 || i == index2) continue;
                instances[tileCount++].SetTile(OpenMeld.Tiles[i]);
            }
            instances[0].SetTile(OpenMeld.Tile);
            instances[3].SetTile(OpenMeld.Extra);
        }

        public float MeldWidth
        {
            get
            {
                if (OpenMeld.Side == MeldSide.Self)
                    return 4 * MahjongConstants.TileWidth;
                if (OpenMeld.IsKong && !OpenMeld.IsAdded)
                    return 3 * MahjongConstants.TileWidth + MahjongConstants.TileHeight;
                return 2 * MahjongConstants.TileWidth + MahjongConstants.TileHeight;
            }
        }
    }
}