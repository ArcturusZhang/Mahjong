using System.Collections.Generic;
using System.Linq;
using Single;
using Single.MahjongDataType;
using UI.Layout;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Multi
{
    public class PlayerHandPanel : MonoBehaviour
    {
        public GameObject HandTilePrefab;
        public GameObject DrawnTilePrefab;

        public void LockTiles()
        {
            transform.TraversalChildren(child =>
            {
                var button = child.GetComponent<Button>();
                button.interactable = false;
            });
        }

        public void LockTiles(Tile[] tiles)
        {
            if (tiles == null) return;
            Debug.Log($"[LockTiles] Locking tiles {string.Join(", ", tiles)}");
            transform.TraversalChildren(child =>
            {
                var tileLayoutElement = child.GetComponent<TileLayoutElement>();
                Debug.Log($"[LockTiles] Investigating tile {tileLayoutElement.Tile}");
                if (!tiles.Contains(tileLayoutElement.Tile, Tile.TileIgnoreColorEqualityComparer)) return;
                Debug.Log($"[LockTiles] Locked tile: {tileLayoutElement.Tile}");
                var button = child.GetComponent<Button>();
                button.interactable = false;
            });
        }

        public void UnlockTiles()
        {
            transform.TraversalChildren(child =>
            {
                var button = child.GetComponent<Button>();
                button.interactable = true;
            });
        }

        public void Clear()
        {
            transform.DestroyAllChild();
        }

        public void Refresh(Player player, List<Tile> tiles, bool richi = false)
        {
            Clear();
            AddTiles(player, tiles, richi);
        }

        public void DrawTile(Player player, Tile tile)
        {
            DrawTile(player, tile, true, DrawnTilePrefab);
        }

        public void AddTiles(Player player, List<Tile> tiles, bool richi = false)
        {
            foreach (var tile in tiles)
            {
                DrawTile(player, tile, false, HandTilePrefab, richi);
            }
        }

        private void DrawTile(Player player, Tile tile, bool discardLastDraw, GameObject prefab, bool richi = false)
        {
            var sprite = ResourceManager.Instance.GetTileSprite(tile);
            var tileImageObject = Instantiate(prefab, transform);
            var tileLayoutElement = tileImageObject.GetComponent<TileLayoutElement>();
            tileLayoutElement.Tile = tile;
            var image = tileImageObject.GetComponent<Image>();
            image.sprite = sprite;
            var button = tileImageObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            if (richi) return;
            button.onClick.AddListener(() => { player.ClientDiscardTile(tile, discardLastDraw); });
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