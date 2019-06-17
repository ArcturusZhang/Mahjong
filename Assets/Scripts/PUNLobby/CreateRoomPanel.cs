using System.Collections;
using System.Collections.Generic;
using Mahjong.Model;
using Managers;
using Photon.Pun;
using UI.DataBinding;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class CreateRoomPanel : MonoBehaviour
    {
        public InputField roomNameInputField;
        public RectTransform baseSettingPanel;
        public RectTransform yakuSettingPanel;
        private readonly List<UIBinder> binders = new List<UIBinder>();
        private ResourceManager manager;
        private GameSetting gameSettings;
        private string roomName;
        private void OnEnable()
        {
            binders.Clear();
            binders.AddRange(GetComponentsInChildren<UIBinder>(true));
            // load settings or load preset
            manager = ResourceManager.Instance;
            LoadSettings();
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
            roomName = $"{PhotonNetwork.NickName}'s Room";
            roomNameInputField.text = roomName;
        }

        private void LoadSettings()
        {
            Debug.Log("Loading last settings...");
            manager.LoadSettings(out gameSettings);
        }

        public void ResetSettings()
        {
            Debug.Log("Reset to corresponding default settings");
            manager.ResetSettings(gameSettings);
            binders.ForEach(binder => binder?.ApplyBinds());
        }

        public void OpenYakuSettingPanel()
        {
            baseSettingPanel.gameObject.SetActive(false);
            yakuSettingPanel.gameObject.SetActive(true);
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
        }

        public void CloseYakuSettingPanel()
        {
            baseSettingPanel.gameObject.SetActive(true);
            yakuSettingPanel.gameObject.SetActive(false);
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
        }

        public void SetRoomName(string value)
        {
            roomName = value;
        }

        public void CreateRoom()
        {
            Launcher.Instance.CreateRoom(roomName, gameSettings);
            gameObject.SetActive(false);
        }

        public void BackToLobby()
        {
            Debug.Log("Back to lobby");
            gameObject.SetActive(false);
        }

        public void OnTotalPlayerChanged(int value)
        {
            var players = (GamePlayers)value;
            ResetSettings();
        }
    }
}
