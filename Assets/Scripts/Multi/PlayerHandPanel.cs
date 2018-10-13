using System.Collections.Generic;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace Multi
{
    public class PlayerHandPanel : MonoBehaviour
    {
        public GameObject HandTilePrefab;
        public GameObject DrawnTilePrefab;

        public void LockTiles()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var button = transform.GetChild(i).GetComponent<Button>();
                button.interactable = false;
            }
        }

        public void UnlockTiles()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var button = transform.GetChild(i).GetComponent<Button>();
                button.interactable = true;
            }
        }

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        public void Refresh(Player player, List<Tile> tiles)
        {
            Clear();
            AddTiles(player, tiles);
        }

        public void DrawTile(Player player, Tile tile)
        {
            DrawTile(player, tile, true, DrawnTilePrefab);
        }

        public void AddTiles(Player player, List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                DrawTile(player, tile, false, HandTilePrefab);
            }
        }

        private void DrawTile(Player player, Tile tile, bool discardLastDraw, GameObject prefab)
        {
            var sprite = ResourceManager.Instance.GetTileSprite(tile);
            var tileImage = Instantiate(prefab, transform);
            var image = tileImage.GetComponent<Image>();
            image.sprite = sprite;
            var button = tileImage.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                player.ClientDiscardTile(tile, discardLastDraw);
            });
        }

        public void DiscardTile()
        {
            DiscardTile(transform.childCount - 1);
        }

        public void DiscardTile(int index)
        {
            var obj = transform.GetChild(index).gameObject;
            Destroy(obj);
        }
    }
}