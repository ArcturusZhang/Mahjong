using System;
using System.Collections.Generic;
using Single;
using Single.MahjongDataType;
using UnityEngine;

namespace Multi
{
    public class PlayerRiverHolder : MonoBehaviour
    {
        public GameObject TilePrefab;
        public float XOffset = 0;
        public float YOffset = 0;
        public float Gap = 0.001f;
        internal readonly List<DiscardedTile> tiles = new List<DiscardedTile>();

        public void DiscardTile(Tile tile, bool richi)
        {
            var discardedTile = new DiscardedTile {Tile = tile, Richi = richi, Offset = new Vector2(XOffset, YOffset)};
            Vector3 position;
            var rotation = MahjongConstants.FaceUp;
            if (richi)
            {
                position = new Vector3(XOffset + MahjongConstants.TileHeight / 2,
                    -(YOffset + MahjongConstants.TileHeight / 2), -MahjongConstants.TileThickness / 2);
                rotation *= Quaternion.Euler(-90, 0, 0);
                XOffset += MahjongConstants.TileHeight + Gap;
            }
            else
            {
                position = new Vector3(XOffset + MahjongConstants.TileWidth / 2,
                    -(YOffset + MahjongConstants.TileHeight / 2), -MahjongConstants.TileThickness / 2);
                XOffset += MahjongConstants.TileWidth + Gap;
            }

            var tileObject = Instantiate(TilePrefab, transform);
            tileObject.transform.localPosition = position;
            tileObject.transform.localRotation = rotation;
            tileObject.name = $"discardTile{tiles.Count}";
            var tileInstance = tileObject.GetComponent<TileInstance>();
            tileInstance.SetTile(tile);
            tiles.Add(discardedTile);
            int currentRow = (tiles.Count - 1) / MahjongManager.Instance.GameSettings.TilesPerRowInRiver;
            if (currentRow == MahjongManager.Instance.GameSettings.MaxRowInRiver - 1) return;

            if (tiles.Count % MahjongManager.Instance.GameSettings.TilesPerRowInRiver == 0) // change row
            {
                XOffset = 0;
                YOffset += MahjongConstants.TileHeight;
            }
        }

        // todo -- need implementation
        public void RetractLastTile()
        {
            throw new NotImplementedException();
        }

        internal struct DiscardedTile
        {
            public Tile Tile;
            public bool Richi;
            public Vector2 Offset;
        }
    }
}