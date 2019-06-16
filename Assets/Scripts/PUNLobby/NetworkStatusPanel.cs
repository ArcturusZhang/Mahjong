using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class NetworkStatusPanel : MonoBehaviour
    {
        public Text statusText;

        private void Update()
        {
            if (statusText == null) return;
            statusText.text = PhotonNetwork.NetworkClientState.ToString();
        }
    }
}
