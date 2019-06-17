using Mahjong.Model;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class RoomEntry : MonoBehaviour
    {
        public Text roomNameText;
        public RectTransform QTJStatus;
        public Text playerStatusText;
        public Button checkRuleButton;
        public Button joinButton;
        public void SetRoom(RoomInfo info)
        {
            var setting = (GameSetting)info.CustomProperties[SettingKeys.SETTING];
            roomNameText.text = info.Name;
            var isQTJ = setting == null ? false : setting.GameMode == GameMode.QTJ;
            QTJStatus.gameObject.SetActive(isQTJ);
            playerStatusText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
            checkRuleButton.onClick.RemoveAllListeners();
            checkRuleButton.onClick.AddListener(() => CheckRules(setting));
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() =>
            {
                Launcher.Instance.JoinRoom(info.Name);
            });
        }

        private void CheckRules(GameSetting setting)
        {
            Launcher.Instance.ShowRulePanel(setting);
        }
    }
}
