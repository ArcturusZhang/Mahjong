using System.Collections;
using Managers;
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
            yield return null;
            // todo -- button for "back to lobby" or "back to room"
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}