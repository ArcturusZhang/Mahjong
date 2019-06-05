using Multi.ServerData;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class PointTransferState : ClientState
    {
        public string[] PlayerNames;
        public int[] Points;
        public PointTransfer[] PointTransfers;

        public override void OnClientStateEnter()
        {
            CurrentRoundStatus.UpdatePoints(Points);
            Debug.Log($"Current points: {string.Join(",", CurrentRoundStatus.Points)}");
            controller.PointTransferManager.SetTransfer(CurrentRoundStatus, PointTransfers, () =>
            {
                localPlayer.RequestNewRound();
            });
        }

        public override void OnClientStateExit()
        {
            controller.PointTransferManager.Close();
        }

        public override void OnStateUpdate()
        {
        }
    }
}