using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    [RequireComponent(typeof(Text))]
    public class PlayerNameDisplayer : MonoBehaviour
    {
        private Text text;

        private void Start()
        {
            text = GetComponent<Text>();
        }
        private void Update()
        {
            text.text = PhotonNetwork.NickName;
        }
    }
}
