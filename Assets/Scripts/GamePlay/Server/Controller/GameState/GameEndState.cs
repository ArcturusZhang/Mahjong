using System.Linq;
using GamePlay.Client.Controller;
using GamePlay.Server.Model.Events;
using Mahjong.Logic;
using Photon.Pun;

namespace GamePlay.Server.Controller.GameState
{
    public class GameEndState : ServerState
    {
        public override void OnServerStateEnter()
        {
            var pointsAndPlaces = MahjongLogic.SortPointsAndPlaces(CurrentRoundStatus.Points);
            var names = CurrentRoundStatus.PlayerNames.ToArray();
            var points = CurrentRoundStatus.Points.ToArray();
            var places = pointsAndPlaces.Select(v => v.Value).ToArray();
            var info = new EventMessages.GameEndInfo
            {
                PlayerNames = names,
                Points = points,
                Places = places
            };
            ClientBehaviour.Instance.photonView.RPC("RpcGameEnd", RpcTarget.AllBufferedViaServer, info);
        }

        public override void OnServerStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}