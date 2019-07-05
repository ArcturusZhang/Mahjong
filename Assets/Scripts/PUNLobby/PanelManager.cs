using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace PUNLobby
{
    public class PanelManager : MonoBehaviour
    {
        public RectTransform LoginPanel;
        public RectTransform LobbyPanel;
        [SerializeField] private RoomListPanel roomListPanel;
        [SerializeField] private CreateRoomPanel createPanel;
        public WarningPanel warningPanel;
        public WarningPanel infoPanel;
        private RectTransform currentPanel;

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }
            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }
            currentPanel = newPanel;
        }

        public void ShowCreateRoomPanel()
        {
            createPanel.gameObject.SetActive(true);
        }

        public void SetRoomList(IList<RoomInfo> rooms)
        {
            roomListPanel.SetRoomList(rooms);
        }
    }
}
