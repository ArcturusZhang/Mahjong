using GamePlay.Server.Model;
using UnityEngine;

namespace GamePlay.Client.Controller.GameState
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