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
            CurrentRoundStatus.UpdatePoints(Points);
            CurrentRoundStatus.UpdateNames(Names);
            // assign round status
            controller.AssignRoundStatus(CurrentRoundStatus);
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}
