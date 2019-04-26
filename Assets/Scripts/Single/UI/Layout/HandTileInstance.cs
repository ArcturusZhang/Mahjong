using System.Collections;
using System.Collections.Generic;
using Multi;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI.Layout
{
    [RequireComponent(typeof(Image))]
    public class HandTileInstance : MonoBehaviour
    {
        private Tile tile;
        private Image tileImage;
        private Button tileButton;
        [SerializeField] private bool IsLastDraw;
        private bool available = true;

        public Tile Tile => tile;

        private void Start()
        {
            tileImage = GetComponent<Image>();
            tileButton = GetComponent<Button>();
        }

        public void SetTile(Tile tile)
        {
            gameObject.SetActive(true);
            var sprite = ResourceManager.Instance?.GetTileSprite(tile);
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite gets null when applied on tile {tile}");
            }
            this.tile = tile;
            tileImage.sprite = sprite;
            tileButton.interactable = available;
            tileImage.color = available ? NormalColor : TintColor;
        }

        public void SetAvailable(bool available)
        {
            this.available = available;
        }

        public void Lock() {
            tileButton.interactable = false;
        }

        public void Unlock() {
            tileButton.interactable = true;
        }

        public void OnClick()
        {
            var p = transform.localPosition;
            transform.localPosition = new Vector3(p.x, 0, p.z);
            Debug.Log($"Requesting discard tile {tile}");
            ClientBehaviour.Instance.OnDiscardTile(tile, IsLastDraw);
        }

        private readonly static Color NormalColor = Color.white;
        private readonly static Color TintColor = Color.gray;
    }
}
