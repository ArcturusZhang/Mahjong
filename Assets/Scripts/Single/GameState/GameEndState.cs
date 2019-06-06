using Lobby;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Single.GameState
{
    public class GameEndState : ClientState
    {
        public string[] PlayerNames;
        public int[] Points;
        public int[] Places;
        public override void OnClientStateEnter()
        {
            controller.GameEndPanelManager.SetPoints(PlayerNames, Points, Places, () =>
            {
                Debug.Log("Back to lobby");
                var lobby = LobbyManager.Instance;
                lobby.ServerChangeScene(lobby.offlineScene);
                // todo -- record points (maybe)?
                CurrentRoundStatus.LocalPlayer.connectionToServer.Disconnect();
            });
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}