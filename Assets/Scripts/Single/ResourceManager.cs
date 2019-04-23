using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single
{
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
        }

        public Texture2D GetTileTexture(Tile tile)
        {
            var key = MahjongConstants.GetTileName(tile);
            return textureDict[key];
        }

        public Sprite GetTileSprite(Tile tile)
        {
            if (tileSprites == null) {
                Debug.LogError("tileSprite is null, something is wrong, please wait and try again.");
                return null;
            }
            var key = MahjongConstants.GetTileName(tile);
            return spriteDict[key];
        }

        public Sprite GetTileSpriteByName(string name) {
            return spriteDict[name];
        }

        private static Sprite FindByName(Sprite[] sprites, string name)
        {
            return sprites.FirstOrDefault(sprite => sprite.name == name);
        }
    }
}