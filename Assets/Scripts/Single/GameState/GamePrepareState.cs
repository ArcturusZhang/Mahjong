using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class GamePrepareState : ClientState
    {
        public int[] Points;
        public string[] Names;
        private float firstTime;
        private const float waitTime = 0.25f;
        public override void OnClientStateEnter()
        {
            // assign round status
            controller.AssignRoundStatus(CurrentRoundStatus);
            // update data
            CurrentRoundStatus.UpdatePoints(Points);
            CurrentRoundStatus.UpdateNames(Names);
            firstTime = Time.time;
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
            if (Time.time - firstTime > waitTime)
                localPlayer.ClientReady(localPlayer.PlayerIndex);
        }
    }
}
