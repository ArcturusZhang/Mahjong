using System.Collections;
using Lobby;
using UI;
using UnityEngine;

namespace GamePlay.Client.Controller.GameState
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
                controller.StartCoroutine(BackToLobby());
                // todo -- record points (maybe)?
            });
        }

        private IEnumerator BackToLobby()
        {
            Debug.Log("Back to lobby");
            var transition = GameObject.FindObjectOfType<SceneTransitionManager>();
            transition.FadeOut();
            yield return new WaitForSeconds(1f);
            var lobby = LobbyManager.Instance;
            lobby.ServerChangeScene(lobby.offlineScene);
            CurrentRoundStatus.LocalPlayer.connectionToServer.Disconnect();
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}