using System.Linq;
using Mahjong.Logic;
using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Client.View
{
    public class PlayerRiverManager : MonoBehaviour
    {
        private const float Width = MahjongConstants.TileWidth + MahjongConstants.TileRiverGapCol;
        private const float Height = -(MahjongConstants.TileHeight + MahjongConstants.TileRiverGapRow);
        private const float Thickness = -MahjongConstants.TileThickness / 2;
        [HideInInspector] public RiverTile[] RiverTiles;
        private Transform[] tiles;
        private TileInstance[] tileInstances;

        private void Start()
        {
            tiles = new Transform[transform.childCount];
            tileInstances = new TileInstance[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                tiles[i] = transform.GetChild(i);
                tileInstances[i] = tiles[i].GetComponent<TileInstance>();
            }
        }

        private void Update()
        {
            if (RiverTiles == null)
            {
                DisableInvalidRiver();
                return;
            }
            int validTileCount = 0;
            int lastValidRichi = -1;
            // show river tiles
            for (int i = 0; i < RiverTiles.Length; i++)
            {
                var riverTile = RiverTiles[i];
                if (riverTile.IsGone) continue;
                var t = tiles[validTileCount];
                var instance = tileInstances[validTileCount];
                t.gameObject.SetActive(true);
                if (riverTile.IsRichi)
                {
                    lastValidRichi = validTileCount;
                    t.localRotation = MahjongConstants.RichiTile;
                }
                else
                {
                    t.localRotation = MahjongConstants.RiverTile;
                }
                // calculate and set position of this tile
                t.localPosition = GetLocalPosition(validTileCount, lastValidRichi);
                instance.SetTile(riverTile.Tile);
                validTileCount++;
            }
            // disable extra tiles
            for (int i = validTileCount; i < tiles.Length; i++)
            {
                tiles[i].gameObject.SetActive(false);
            }
        }

        public TileInstance GetLastTile()
        {
            if (RiverTiles == null) return null;
            int count = RiverTiles.Count(t => !t.IsGone);
            return tileInstances[count - 1];
        }

        public void ShineOff()
        {
            foreach (var tile in tileInstances)
            {
                tile.ShineOff();
            }
        }

        private Vector3 GetLocalPosition(int validTileIndex, int lastValidRichi)
        {
            int row = validTileIndex / MahjongConstants.TilesPerRowInRiver;
            int col = validTileIndex % MahjongConstants.TilesPerRowInRiver;
            if (row >= MahjongConstants.MaxRowInRiver)
            {
                col += (row - MahjongConstants.MaxRowInRiver + 1) * MahjongConstants.TilesPerRowInRiver;
                row = MahjongConstants.MaxRowInRiver - 1;
            }
            if (lastValidRichi < 0)
                return new Vector3(col * Width, row * Height, Thickness);
            else
            {
                int richiRow = lastValidRichi / MahjongConstants.TilesPerRowInRiver;
                if (richiRow < row) return new Vector3(col * Width, row * Height, Thickness);
                else if (validTileIndex == lastValidRichi)
                    return new Vector3(col * Width + (MahjongConstants.TileHeight - MahjongConstants.TileWidth) / 2, row * Height, Thickness);
                else
                    return new Vector3(col * Width + MahjongConstants.TileHeight - MahjongConstants.TileWidth, row * Height, Thickness);
            }
        }

        private void DisableInvalidRiver()
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].gameObject.SetActive(false);
            }
        }
    }
}
