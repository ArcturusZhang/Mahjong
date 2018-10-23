using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public class ResourceManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        private static readonly string[] TileSuits = {"m", "s", "p", "z"};
        public static ResourceManager Instance { get; private set; }
        private Sprite[] baseTimeSprites;
        private Sprite[] bonusTimeSprites;
        private Sprite[] tileSprites;
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
            var sprites = Resources.LoadAll<Sprite>("Textures/UIElements/mjdesktop3");
            baseTimeSprites = new Sprite[10];
            bonusTimeSprites = new Sprite[10];
            for (int i = 0; i < 10; i++)
            {
                baseTimeSprites[i] = sprites.FirstOrDefault(sprite => sprite.name == $"time{i}");
                bonusTimeSprites[i] = sprites.FirstOrDefault(sprite => sprite.name == $"bonus{i}");
            }

            yield return null;
            for (int i = 0; i < TileSuits.Length; i++)
            {
                for (int rank = 0; rank <= 9; rank++)
                {
                    var key = $"{rank}{TileSuits[i]}";
                    var texture = Resources.Load<Texture2D>($"Textures/{key}");
                    if (texture != null) textureDict.Add(key, texture);
                    yield return null;
                }
            }

            tileSprites = Resources.LoadAll<Sprite>("Textures/UIElements/tile_ui");
        }

        public Sprite GetBaseTimeSprite(int index)
        {
            return baseTimeSprites[index];
        }

        public Sprite GetBonusTimeSprite(int index)
        {
            return bonusTimeSprites[index];
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
    }
}