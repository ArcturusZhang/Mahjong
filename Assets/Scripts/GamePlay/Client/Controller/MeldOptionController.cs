using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Mahjong.Model;
using Managers;

namespace GamePlay.Client.Controller
{
    public class MeldOptionController : MonoBehaviour, IPointerClickHandler
    {
        [HideInInspector] public OpenMeld OpenMeld;
        private Image[] tiles;
        private RectTransform rect;
        private const float TileWidth = 64;
        private ResourceManager manager;
        private Action<OpenMeld> callback;

        private void Awake()
        {
            tiles = new Image[transform.childCount];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = transform.GetChild(i).GetComponent<Image>();
            }
            rect = GetComponent<RectTransform>();
            manager = ResourceManager.Instance;
        }

        public void SetMeld(OpenMeld meld, Action<OpenMeld> callback)
        {
            OpenMeld = meld;
            this.callback = callback;
            for (int i = 0; i < meld.Tiles.Length; i++)
            {
                var tile = meld.Tiles[i];
                var sprite = manager.GetTileSprite(tile);
                tiles[i].sprite = sprite;
            }
            for (int i = meld.Tiles.Length; i < tiles.Length; i++)
            {
                tiles[i].gameObject.SetActive(false);
            }
            rect.sizeDelta = new Vector2(TileWidth * meld.Tiles.Length, rect.sizeDelta.y);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (callback != null) callback(OpenMeld);
        }
    }
}
