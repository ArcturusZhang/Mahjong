using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace PUNLobby
{
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField] private InputField nameInputField;
        private const string LAST_LOGIN = "/last_login.txt";

        private void OnEnable()
        {
            var lastLoginName = SerializeUtility.LoadContentOrDefault(Application.persistentDataPath + LAST_LOGIN, "");
            nameInputField.text = lastLoginName;
        }

        public void Login()
        {
            var launcher = Launcher.Instance;
            var playerName = nameInputField.text;
            if (string.IsNullOrEmpty(playerName))
            {
                launcher.PanelManager.warningPanel.Show(400, 200, "Please input a player name.");
                return;
            }
            launcher.Connect(playerName);
            SerializeUtility.SaveContent(Application.persistentDataPath + LAST_LOGIN, playerName);
            launcher.PanelManager.infoPanel.Show(400, 200, "Connecting...");
        }

        public void ExitGame()
        {
            Debug.Log("Quit game...");
            Application.Quit();
        }
    }
}
