using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mahjong.Model;
using UnityEngine;
using Utils;

namespace Managers
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
            LoadDefaultSettings();
            yield return null;
            tileSprites = Resources.LoadAll<Sprite>("Textures/UIElements/tile_ui");
            yield return null;
            spriteDict = tileSprites.ToDictionary(sprite => sprite.name);
            for (int i = 0; i < TileSuits.Length; i++)
            {
                for (int rank = 0; rank <= 9; rank++)
                {
                    var key = $"{rank}{TileSuits[i]}";
                    var texture = Resources.Load<Texture2D>($"Textures/TileTextures/{key}");
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

        private const string Default_Settings_2 = "Data/default_settings_2";
        private const string Default_Settings_3 = "Data/default_settings_3";
        private const string Default_Settings_4 = "Data/default_settings_4";
        public const string Last_Settings = "/settings.json";
        private readonly IDictionary<GamePlayers, string> defaultSettings = new Dictionary<GamePlayers, string>();

        private void LoadDefaultSettings()
        {
            defaultSettings.Clear();
            defaultSettings.Add(GamePlayers.Two, Resources.Load<TextAsset>(Default_Settings_2).text);
            defaultSettings.Add(GamePlayers.Three, Resources.Load<TextAsset>(Default_Settings_3).text);
            defaultSettings.Add(GamePlayers.Four, Resources.Load<TextAsset>(Default_Settings_4).text);
        }

        public void LoadSettings(out GameSetting gameSetting)
        {
            gameSetting = SerializeUtility.Load<GameSetting>(Last_Settings, defaultSettings[GamePlayers.Four]);
        }

        public void SaveSettings(object setting, string path)
        {
            setting.Save(path);
        }

        public void SaveSettings(GameSetting gameSetting )
        {
            SaveSettings(gameSetting, Last_Settings);
        }

        public void ResetSettings(GameSetting gameSetting)
        {
            Debug.Log("Reset to corresponding default settings");
            var setting = defaultSettings[gameSetting.GamePlayers];
            JsonUtility.FromJsonOverwrite(setting, gameSetting);
        }
    }
}