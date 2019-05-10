using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class GameEndState : ClientState
    {
        // public ClientRoundStatus CurrentRoundStatus;
        public string[] PlayerNames;
        public int[] Points;
        public int[] Places;
        // private ViewController controller;
        public override void OnClientStateEnter()
        {
            controller.GameEndPanelManager.SetPoints(PlayerNames, Points, Places, () =>
            {
                Debug.Log("Back to lobby");
                // todo
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