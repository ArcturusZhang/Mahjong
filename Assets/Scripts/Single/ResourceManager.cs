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
        [Header("Game Settings")] public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        private static readonly string[] TileSuits = {"m", "s", "p", "z"};
        public static ResourceManager Instance { get; private set; }
        private Sprite[] baseTimeSprites;
        private Sprite[] bonusTimeSprites;
        private Sprite[] fanNumbers;
        private Sprite[] fuNumbers;
        private Sprite[] baseNumbers;
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
            baseTimeSprites = new Sprite[10];
            bonusTimeSprites = new Sprite[10];
            fanNumbers = new Sprite[10];
            fuNumbers = new Sprite[10];
            baseNumbers = new Sprite[10];
            placeNumbers = new Sprite[4];
            placeCharacters = new Sprite[4];
            for (int i = 0; i < 10; i++)
            {
                baseTimeSprites[i] = FindByName(mjdesktop3, $"time{i}");
                bonusTimeSprites[i] = FindByName(mjdesktop3, $"bonus{i}");
                fanNumbers[i] = FindByName(mjdesktop3, $"fan{i}");
                fuNumbers[i] = FindByName(mjdesktop3, $"fu{i}");
                baseNumbers[i] = FindByName(mjdesktop3, $"base{i}");
                yield return null;
            }

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

        public Sprite BaseTime(int number)
        {
            return baseTimeSprites[number];
        }

        public Sprite BonusTime(int number)
        {
            return bonusTimeSprites[number];
        }

        public Sprite FanNumber(int number)
        {
            return fanNumbers[number];
        }

        public Sprite FuNumber(int number)
        {
            return fuNumbers[number];
        }

        public Sprite BaseNumber(int number)
        {
            return baseNumbers[number];
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