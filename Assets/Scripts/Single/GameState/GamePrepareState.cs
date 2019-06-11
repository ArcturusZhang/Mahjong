using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class GamePrepareState : ClientState
    {
        public int[] Points;
        public string[] Names;
        public override void OnClientStateEnter()
        {
            // assign round status
            controller.AssignRoundStatus(CurrentRoundStatus);
            // update data
            CurrentRoundStatus.UpdatePoints(Points);
            CurrentRoundStatus.UpdateNames(Names);
            localPlayer.ClientReady(localPlayer.PlayerIndex);
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}
