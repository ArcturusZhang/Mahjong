using Mahjong.Model;
using Managers;
using UnityEngine;

namespace GamePlay.Client.View
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TileInstance : MonoBehaviour
    {
        public Tile Tile;
        public Canvas Canvas;
        private MeshRenderer meshRenderer;

        private void OnEnable()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetTile(Tile tile)
        {
            if (tile.Rank == 0)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            Tile = tile;
            var material = meshRenderer.material;
            material.mainTexture = ResourceManager.Instance?.GetTileTexture(tile);
        }

        public void Shine()
        {
            Canvas.gameObject.SetActive(true);
        }

        public void ShineOff()
        {
            Canvas.gameObject.SetActive(false);
        }
    }
}
