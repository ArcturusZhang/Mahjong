using System.Collections.Generic;
using System.Linq;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PointSummaryPanel
{
    public class HandPanelController : MonoBehaviour
    {
        public float Width = 62;
        public float Height = 100;
        public float Gap = 20;
        public Sprite TileBack;

        private void OnDisable()
        {
            gameObject.SetActive(false);
        }

        public void SetTiles(List<Tile> handTiles, List<Meld> openMelds, Tile winningTile)
        {
            gameObject.SetActive(true);
            handTiles.Sort();
            var meldTiles = new List<SummaryMeldTile>();
            foreach (var meld in openMelds)
            {
                for (int i = 0; i < meld.TileCount; i++)
                {
                    meldTiles.Add(new SummaryMeldTile
                    {
                        Tile = meld.Tiles[i],
                        Front = meld.Revealed || i > 0 && i < meld.TileCount - 1
                    });
                }
            }

            int handCount = handTiles.Count;
            int meldCount = handCount + meldTiles.Count;
            int totalCount = meldCount + 1;
            for (int i = 0; i < transform.childCount; i++)
            {
                var obj = transform.GetChild(i);
                var rectTransform = obj.GetComponent<RectTransform>();
                var image = obj.GetComponent<Image>();
                if (i < handCount)
                {
                    obj.gameObject.SetActive(true);
                    image.sprite = ResourceManager.Instance.GetTileSprite(handTiles[i]);
                    rectTransform.anchoredPosition = new Vector2(i * Width, 0);
                }
                else if (i < meldCount)
                {
                    obj.gameObject.SetActive(true);
                    var summaryTile = meldTiles[i - handCount];
                    image.sprite = summaryTile.Front
                        ? ResourceManager.Instance.GetTileSprite(meldTiles[i - handCount].Tile)
                        : TileBack;
                    rectTransform.anchoredPosition = new Vector2(i * Width + Gap, 0);
                }
                else if (i < totalCount)
                {
                    obj.gameObject.SetActive(true);
                    image.sprite = ResourceManager.Instance.GetTileSprite(winningTile);
                    rectTransform.anchoredPosition = new Vector2(i * Width + 2 * Gap, 0);
                }
                else
                {
                    obj.gameObject.SetActive(false);
                }
            }
        }

        private struct SummaryMeldTile
        {
            internal Tile Tile;
            internal bool Front;
        }
    }
}