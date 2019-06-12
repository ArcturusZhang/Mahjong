using System.Collections.Generic;
using Mahjong.Model;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class DiscardHintManager : MonoBehaviour
    {
        private const float TileWidth = 62;
        private const float Gap = 10;
        public RectTransform Background;
        public Image[] WaitingTiles;

        public void SetWaitingTiles(IList<Tile> list)
        {
            var manager = ResourceManager.Instance;
            var size = Background.sizeDelta;
            var count = list.Count;
            Background.sizeDelta = new Vector2(count * TileWidth + (count + 1) * Gap, size.y);
            for (int i = 0; i < count; i++)
            {
                WaitingTiles[i].sprite = manager.GetTileSprite(list[i]);
                WaitingTiles[i].gameObject.SetActive(true);
            }
            for (int i = count; i < WaitingTiles.Length; i++)
            {
                WaitingTiles[i].gameObject.SetActive(false);
            }
        }

        public void Show()
        {
            Background.gameObject.SetActive(true);
        }

        public void Close()
        {
            Background.gameObject.SetActive(false);
        }
    }
}