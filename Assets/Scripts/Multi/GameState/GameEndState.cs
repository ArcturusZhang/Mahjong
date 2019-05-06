using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using StateMachine.Interfaces;
using UnityEngine;

namespace Multi.GameState
{
    public class GameEndState : IState
    {
        public ServerRoundStatus CurrentRoundStatus;
        private IList<Player> players;
        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            players = CurrentRoundStatus.Players;
            var pointsAndIndices = CurrentRoundStatus.Points.Select((p, i) => new KeyValuePair<int, int>(p, i))
                .OrderBy(key => key, new PointsComparer());
            var names = CurrentRoundStatus.PlayerNames.ToArray();
            var points = CurrentRoundStatus.Points.ToArray();
            var places = pointsAndIndices.Select(v => v.Value).ToArray();
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

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }

        public void OnStateUpdate()
        {
        }

        private struct PointsComparer : IComparer<KeyValuePair<int, int>>
        {
            public int Compare(KeyValuePair<int, int> point1, KeyValuePair<int, int> point2)
            {
                var pointsCmp = point1.Key.CompareTo(point2.Key);
                if (pointsCmp != 0) return -pointsCmp;
                return point1.Value.CompareTo(point2.Value);
            }
        }
    }
}