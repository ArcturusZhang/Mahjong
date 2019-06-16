using Photon.Pun;
using UnityEngine;

namespace PUNLobby
{
    public class PUN_Player : MonoBehaviourPun, IPunObservable​
    {
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // Debug.Log("Write sending player status code here");
        }
    }
}
