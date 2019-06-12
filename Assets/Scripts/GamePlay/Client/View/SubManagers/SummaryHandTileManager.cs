using System.Collections.Generic;
using Mahjong.Model;
using Managers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class SummaryHandTileManager : MonoBehaviour
    {
        private const float TileWidth = 62f;
        private const float Gap = 30f;
        private const string back = "back";
        private Image[] tileImages;
        private ResourceManager manager;
        private RectTransform rect;
        private const float MaxWidth = 900f;

        private void OnEnable()
        {
            tileImages = new Image[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                tileImages[i] = transform.GetChild(i).GetComponent<Image>();
            }
            manager = ResourceManager.Instance;
            rect = GetComponent<RectTransform>();
        }

        public void SetHandTiles(IList<Tile> handTiles, IList<OpenMeld> openMelds, Tile winningTile)
        {
            if (manager == null) manager = ResourceManager.Instance;
            var offset = 0f;
            int count = 0;
            // hand tiles
            for (; count < handTiles.Count; count++)
            {
                tileImages[count].enabled = true;
                tileImages[count].sprite = manager.GetTileSprite(handTiles[count]);
                tileImages[count].rectTransform.anchoredPosition = new Vector2(offset, 0);
                offset += TileWidth;
            }
            if (openMelds.Count > 0) offset += Gap;
            // open melds
            for (int i = 0; i < openMelds.Count; i++)
            {
                if (openMelds[i].IsKong && !openMelds[i].Revealed)
                {
                    Assert.AreEqual(openMelds[i].Tiles.Length, 4);
                    for (int j = 0; j < 4; j++)
                    {
                        if (j >= 1 && j <= 2)
                            tileImages[count].sprite = manager.GetTileSpriteByName(back);
                        else
                            tileImages[count].sprite = manager.GetTileSprite(openMelds[i].Tiles[j]);
                        tileImages[count].enabled = true;
                        tileImages[count].rectTransform.anchoredPosition = new Vector2(offset, 0);
                        count++;
                        offset += TileWidth;
                    }
                }
                else
                {
                    foreach (var tile in openMelds[i].Tiles)
                    {
                        tileImages[count].enabled = true;
                        tileImages[count].sprite = manager.GetTileSprite(tile);
                        tileImages[count].rectTransform.anchoredPosition = new Vector2(offset, 0);
                        count++;
                        offset += TileWidth;
                    }
                }
            }
            // winning tile
            offset += Gap;
            tileImages[count].enabled = true;
            tileImages[count].sprite = manager.GetTileSprite(winningTile);
            tileImages[count].rectTransform.anchoredPosition = new Vector2(offset, 0);
            count++;
            // rest of unused images
            for (; count < tileImages.Length; count++)
            {
                tileImages[count].enabled = false;
            }
            // change scale if necessary
            var width = offset + TileWidth;
            if (width <= MaxWidth)
            {
                rect.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                var factor = MaxWidth / width;
                rect.localScale = new Vector3(factor, factor, factor);
            }
        }
    }
}
