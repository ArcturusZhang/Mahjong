using System.Collections;
using System.Collections.Generic;
using Multi;
using Single;
using Single.MahjongDataType;
using Single.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI.Layout
{
    [RequireComponent(typeof(Image))]
    public class HandTileInstance : MonoBehaviour
    {
        [SerializeField] private Tile tile;
        private Image tileImage;
        private Button tileButton;
        [SerializeField] private bool IsLastDraw;
        private Sprite sprite;

        public Tile Tile => tile;

        private void OnEnable()
        {
            tileImage = GetComponent<Image>();
            tileButton = GetComponent<Button>();
        }

        private void Start()
        {
            tileImage.sprite = sprite;
        }

        private void Update()
        {
            if (tileImage != null)
            {
                tileImage.sprite = sprite;
            }
        }

        public void SetTile(Tile tile)
        {
            var sprite = ResourceManager.Instance.GetTileSprite(tile);
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite gets null when applied on tile {tile}");
            }
            this.tile = tile;
            this.sprite = sprite;
        }

        public void Lock()
        {
            tileButton.interactable = false;
        }

        public void Unlock()
        {
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
