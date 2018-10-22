using UnityEngine;
using UnityEngine.Networking;

namespace Lobby
{
    // Subclass this and redefine the function you want
    // then add it to the lobby prefab
    public abstract class LobbyHook : MonoBehaviour
    {
        public virtual void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer,
            GameObject gamePlayer)
        {
        }
    }
}