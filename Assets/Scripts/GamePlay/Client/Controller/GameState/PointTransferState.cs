using System.Linq;
using GamePlay.Server.Model;
using Mahjong.Logic;
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
            var pointAndPlace = MahjongLogic.SortPointsAndPlaces(Points);
            var places = pointAndPlace.Select(v => v.Value).ToArray();
            controller.PointTransferManager.SetTransfer(CurrentRoundStatus, places, PointTransfers, () =>
            {
                ClientBehaviour.Instance.NextRound();
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