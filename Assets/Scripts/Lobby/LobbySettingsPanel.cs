using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UI.DataBinding;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class LobbySettingsPanel : MonoBehaviour
    {
        private readonly List<UIBinder> binders = new List<UIBinder>();
        private const string Default_Settings_2 = "Data/default_settings_2";
        private const string Default_Settings_3 = "Data/default_settings_3";
        private const string Default_Settings_4 = "Data/default_settings_4";
        private const string Default_Yaku_Settings = "Data/default_yaku_settings";
        private const string Last_Settings = "/settings.json";
        private const string Last_Yaku_Settings = "/yaku_settings.json";
        private readonly IDictionary<GamePlayers, string> defaultSettings = new Dictionary<GamePlayers, string>();
        private string defaultYakuSettings;
        public LobbyManager lobbyManager;
        public Button StartHostButton;
        private GameSettings GameSettings;
        private YakuSettings YakuSettings;

        public void CloseSettingsPanel()
        {
            gameObject.SetActive(false);
        }

        public void OpenSettingsPanelForHost()
        {
            // load last settings
            LoadSettings();
            gameObject.SetActive(true);
        }

        public void OnStartHostButtonClicked()
        {
            lobbyManager.StartHost();
            gameObject.SetActive(false);
            // save settings to data folder
            SaveSettings();
        }

        public void OnTotalPlayerChanged(int value)
        {
            var players = (GamePlayers)value;
            Debug.Log($"GamePlayers has been changed to {players}");
            ResetSettings();
        }

        private void LoadSettings()
        {
            Debug.Log("Loading last settings...");
            GameSettings = LobbyManager.Instance.GameSettings;
            YakuSettings = LobbyManager.Instance.YakuSettings;
            LoadDefaultSettings();
            GameSettings.Load(Last_Settings, defaultSettings[GamePlayers.Four]);
            YakuSettings.Load(Last_Yaku_Settings, defaultYakuSettings);
        }

        private void SaveSettings()
        {
            GameSettings.Save(Last_Settings);
            YakuSettings.Save(Last_Yaku_Settings);
        }

        public void ResetSettings()
        {
            Debug.Log("Reset to default settings");
            var gameSettings = defaultSettings[GameSettings.GamePlayers];
            JsonUtility.FromJsonOverwrite(gameSettings, GameSettings);
            JsonUtility.FromJsonOverwrite(defaultYakuSettings, YakuSettings);
            binders.ForEach(binder => binder?.ApplyBinds());
        }

        private void LoadDefaultSettings()
        {
            binders.Clear();
            binders.AddRange(GetComponentsInChildren<UIBinder>(true));
            defaultSettings.Clear();
            defaultSettings.Add(GamePlayers.Two, Resources.Load<TextAsset>(Default_Settings_2).text);
            defaultSettings.Add(GamePlayers.Three, Resources.Load<TextAsset>(Default_Settings_3).text);
            defaultSettings.Add(GamePlayers.Four, Resources.Load<TextAsset>(Default_Settings_4).text);
            defaultYakuSettings = Resources.Load<TextAsset>(Default_Yaku_Settings).text;
        }
    }
}
