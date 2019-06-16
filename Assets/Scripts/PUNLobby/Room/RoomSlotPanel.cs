using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby.Room
{
    public class RoomSlotPanel : MonoBehaviour
    {
        public Image roomMaster;
        public Image readySign;
        public Text playerNameText;

        public void Set(bool isMaster, string playerName, bool isReady)
        {
            roomMaster.gameObject.SetActive(isMaster);
            readySign.gameObject.SetActive(isMaster || isReady);
            playerNameText.text = playerName;
        }
    }
}
