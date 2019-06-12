using Mahjong.Model;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.Elements
{
    [RequireComponent(typeof(Image))]
    public class SimpleTile : MonoBehaviour
    {
        [SerializeField] private Tile tile;
        private Image tileImage;
        private Sprite sprite;

        public Tile Tile => tile;

        private void OnEnable()
        {
            tileImage = GetComponent<Image>();
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
    }
}
