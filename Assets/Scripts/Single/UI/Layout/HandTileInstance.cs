using System.Collections;
using System.Collections.Generic;
using Multi;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI.Layout
{
    public class HandTileInstance : MonoBehaviour
    {
        private Tile Tile;
        [SerializeField] private Image TileImage;
        [SerializeField] private bool IsLastDraw;

        public void SetTile(Tile tile)
        {
            var sprite = ResourceManager.Instance?.GetTileSprite(tile);
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite gets null when applied on tile {tile}");
            }
            Tile = tile;
            TileImage.sprite = sprite;
        }

        public void OnClick()
        {
            var p = transform.localPosition;
            transform.localPosition = new Vector3(p.x, 0, p.z);
            Debug.Log($"Requesting discard tile {Tile}");
            ClientBehaviour.Instance.OnDiscardTile(Tile, IsLastDraw);
        }
    }
}
