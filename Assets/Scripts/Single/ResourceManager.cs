using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single
{
    [Obsolete]
    public class ResourceManager : MonoBehaviour
    {
        private static readonly string[] TileSuits = {"m", "s", "p", "z"};
        public static ResourceManager Instance { get; private set; }
        private Sprite[] placeCharacters;
        private Sprite[] placeNumbers;
        private Sprite[] tileSprites;
        private Sprite fourWinds;
        private Sprite fourRichi;
        private Sprite fourKongs;
        private readonly IDictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();
        private readonly IDictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadSpritesAsync());
        }

        private IEnumerator LoadSpritesAsync()
        {
            var mjdesktop3 = Resources.LoadAll<Sprite>("Textures/UIElements/mjdesktop3");
            placeNumbers = new Sprite[4];
            placeCharacters = new Sprite[4];

            for (int i = 0; i < 4; i++)
            {
                placeCharacters[i] = FindByName(mjdesktop3, $"place{i + 1}");
                placeNumbers[i] = FindByName(mjdesktop3, $"no{i + 1}");
            }

            yield return null;

            fourWinds = FindByName(mjdesktop3, "4winds");
            fourRichi = FindByName(mjdesktop3, "4richi");
            fourKongs = FindByName(mjdesktop3, "4kongs");
            yield return null;

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

            tileSprites = Resources.LoadAll<Sprite>("Textures/UIElements/tile_ui");
        }

        public Pair<Sprite, Sprite> Place(int place)
        {
            if (place <= 0 || place > 4) return new Pair<Sprite, Sprite>(null, null);
            return new Pair<Sprite, Sprite>(placeNumbers[place - 1], placeCharacters[place - 1]);
        }

        public Texture2D GetTileTexture(Tile tile)
        {
            var key = MahjongConstants.GetTileName(tile);
            return textureDict[key];
        }

        public Sprite GetTileSprite(Tile tile)
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

        private static Sprite FindByName(Sprite[] sprites, string name)
        {
            return sprites.FirstOrDefault(sprite => sprite.name == name);
        }
    }
}