using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UI.DataBinding;
using UnityEngine;

namespace Lobby
{
    public class LobbySettingsPanel : MonoBehaviour
    {
        private readonly List<UIBinder> binders = new List<UIBinder>();
        private const string Default_Settings_2 = "Data/default_settings_2";
        private const string Default_Settings_3 = "Data/default_settings_3";
        private const string Default_Settings_4 = "Data/default_settings_4";
        private const string Default_Yaku_Settings = "Data/YakuSettings";
        private readonly IDictionary<GamePlayers, string> defaultSettings = new Dictionary<GamePlayers, string>();
        private string defaultYakuSettings;
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        private void Awake()
        {
            binders.Clear();
            binders.AddRange(GetComponentsInChildren<UIBinder>(true));
            defaultSettings.Add(GamePlayers.Two, Resources.Load<TextAsset>(Default_Settings_2).text);
            defaultSettings.Add(GamePlayers.Three, Resources.Load<TextAsset>(Default_Settings_3).text);
            defaultSettings.Add(GamePlayers.Four, Resources.Load<TextAsset>(Default_Settings_4).text);
            defaultYakuSettings = Resources.Load<TextAsset>(Default_Yaku_Settings).text;
        }

        public void CloseSettingsPanel()
        {
            gameObject.SetActive(false);
        }

        public void OpenSettingsPanel()
        {
            gameObject.SetActive(true);
        }

        public void ResetSettings()
        {
            Debug.Log("Reset to default settings");
            var gameSettings = defaultSettings[GameSettings.GamePlayers];
            JsonUtility.FromJsonOverwrite(gameSettings, LobbyManager.Instance.GameSettings);
            JsonUtility.FromJsonOverwrite(defaultYakuSettings, LobbyManager.Instance.YakuSettings);
            binders.ForEach(binder => binder?.ApplyBinds());
        }
    }
}
