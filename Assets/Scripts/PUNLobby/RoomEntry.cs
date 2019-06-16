using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class RoomEntry : MonoBehaviour
    {
        public Text roomNameText;
        public Text playerStatusText;
        public Button joinButton;
        public void SetRoom(RoomInfo info)
        {
            roomNameText.text = info.Name;
            playerStatusText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() =>
            {
                Launcher.Instance.JoinRoom(info.Name);
            });
        }
    }
}
