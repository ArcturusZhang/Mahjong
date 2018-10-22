using Multi;
using UnityEngine;
using UnityEngine.Networking;

namespace Lobby
{
	public class LobbyPlayerHook : LobbyHook {
		public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
		{
			var lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
			var player = gamePlayer.GetComponent<Player>();
			player.PlayerName = lobby.playerName;
		}
	}
}
