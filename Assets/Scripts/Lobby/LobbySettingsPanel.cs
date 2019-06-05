using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using Single.Managers;
using UI.DataBinding;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Lobby
{
    public class LobbySettingsPanel : MonoBehaviour
    {
        private readonly List<UIBinder> binders = new List<UIBinder>();
        public LobbyManager lobbyManager;
        public UIBinder GameSettingBinder;
        public UIBinder YakuSettingBinder;
        private ResourceManager manager;
        private GameSetting GameSetting;
        private YakuSetting YakuSetting;

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

        private void LoadSettings()
        {
            Debug.Log("Loading last settings...");
            binders.Clear();
            binders.AddRange(GetComponentsInChildren<UIBinder>(true));
            manager = ResourceManager.Instance;
            manager.LoadSettings(out GameSetting, out YakuSetting);
            GameSettingBinder.Target = GameSetting;
            YakuSettingBinder.Target = YakuSetting;
        }

        public void ResetSettings()
        {
            Debug.Log("Reset to corresponding default settings");
            manager.ResetSettings(GameSetting, YakuSetting);
            binders.ForEach(binder => binder?.ApplyBinds());
        }

        /*
        callbacks
         */
        public void OnStartHostButtonClicked()
        {
            lobbyManager.maxPlayers = GameSetting.MaxPlayer;
            lobbyManager.StartHost();
            gameObject.SetActive(false);
            // save settings to data folder
            manager.SaveSettings(GameSetting, YakuSetting);
            Debug.Log($"GameSettings: {GameSetting}\nYakuSetting: {YakuSetting}");
        }

        public void OnTotalPlayerChanged(int value)
        {
            var players = (GamePlayers)value;
            Debug.Log($"GamePlayers has been changed to {players}");
            ResetSettings();
        }

    }
}
