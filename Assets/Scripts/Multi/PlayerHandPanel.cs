using System;
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
        private Sprite[] tileSprites;
        private IDictionary<string, Sprite> spriteDict;

        private void Awake()
        {
            tileSprites = Resources.LoadAll<Sprite>("Textures/mjui");
            spriteDict = new Dictionary<string, Sprite>();
        }

        public void Refresh(Player player, List<Tile> tiles)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            AddTiles(player, tiles);
        }

        public void DrawTile(Player player, Tile tile)
        {
            var sprite = GetSprite(tile);
            var tileImage = Instantiate(DrawnTilePrefab, transform);
            var image = tileImage.GetComponent<Image>();
            image.sprite = sprite;
            var button = tileImage.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                player.ClientDiscardTile(tile, true);
            });
        }

        public void AddTiles(Player player, List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                var sprite = GetSprite(tile);
                var tileImage = Instantiate(HandTilePrefab, transform);
                var image = tileImage.GetComponent<Image>();
                image.sprite = sprite;
                var button = tileImage.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    player.ClientDiscardTile(tile, false);
                });
            }
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

        private Sprite GetSprite(Tile tile)
        {
            var key = MahjongConstants.GetTileName(tile);
            if (!spriteDict.ContainsKey(key))
            {
                foreach (var sprite in tileSprites)
                {
                    if (key.Equals(sprite.name, StringComparison.OrdinalIgnoreCase))
                    {
                        spriteDict.Add(key, sprite);
                    }
                }
            }

            return spriteDict[key];
        }
    }
}