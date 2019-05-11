using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Single.MahjongDataType;
using UnityEngine;

namespace Single.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        private static readonly string[] TileSuits = { "m", "s", "p", "z" };
        public static ResourceManager Instance { get; private set; }
        private Sprite[] tileSprites;
        private readonly IDictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();
        private IDictionary<string, Sprite> spriteDict;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadSpritesAsync());
        }

        private IEnumerator LoadSpritesAsync()
        {
            tileSprites = Resources.LoadAll<Sprite>("Textures/UIElements/tile_ui");
            yield return null;
            spriteDict = tileSprites.ToDictionary(sprite => sprite.name);
            for (int i = 0; i < TileSuits.Length; i++)
            {
                for (int rank = 0; rank <= 9; rank++)
                {
                    var key = $"{rank}{TileSuits[i]}";
                    var texture = Resources.Load<Texture2D>($"Textures/{key}");
                    if (texture != null) textureDict.Add(key, texture);
                }
                yield return null;
            }
        }

        public Texture2D GetTileTexture(Tile tile)
        {
            var key = GetTileName(tile);
            return textureDict[key];
        }

        public Sprite GetTileSprite(Tile tile)
        {
            if (tileSprites == null)
            {
                Debug.LogError("tileSprite is null, something is wrong, please wait and try again.");
                return null;
            }
            var key = GetTileName(tile);
            return spriteDict[key];
        }

        public Sprite GetTileSpriteByName(string name)
        {
            return spriteDict[name];
        }

        public static string GetTileName(Tile tile)
        {
            int index = tile.IsRed ? 0 : tile.Rank;
            return index + tile.Suit.ToString().ToLower();
        }
    }
}