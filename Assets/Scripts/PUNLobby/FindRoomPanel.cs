using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class FindRoomPanel : MonoBehaviour
    {
        public InputField inputField;
        public WarningPanel warningPanel;
        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void OnJoinButtonClicked()
        {
            var roomName = inputField.text;
            if (string.IsNullOrEmpty(roomName))
            {
                warningPanel.Show(400, 200, "Please enter a room name for search.");
                return;
            }
            Launcher.Instance.JoinRoom(roomName);
        }
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
