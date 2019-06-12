using System.Collections.Generic;
using System.Linq;
using GamePlay.Server.Model.Messages;
using Mahjong.Logic;

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
            var message = new ServerGameEndMessage
            {
                PlayerNames = names,
                Points = points,
                Places = places
            };
            for (int i = 0; i < players.Count; i++)
            {
                players[i].connectionToClient.Send(MessageIds.ServerGameEndMessage, message);
            }
        }

        public override void OnServerStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}