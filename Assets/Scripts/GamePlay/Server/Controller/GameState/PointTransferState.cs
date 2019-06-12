using System.Collections.Generic;
using System.Linq;
using GamePlay.Server.Model;
using GamePlay.Server.Model.Messages;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace GamePlay.Server.Controller.GameState
{
    public class PointTransferState : ServerState
    {
        public IList<PointTransfer> PointTransferList;
        public bool NextRound;
        public bool ExtraRound;
        public bool KeepSticks;
        private bool[] responds;
        private float firstTime;
        private float serverTimeOut;

        public override void OnServerStateEnter()
        {
            Debug.Log($"[Server] Transfers: {string.Join(", ", PointTransferList)}");
            NetworkServer.RegisterHandler(MessageIds.ClientNextRoundMessage, OnNextMessageReceived);
            var names = players.Select(player => player.PlayerName).ToArray();
            // update points of each player
            foreach (var transfer in PointTransferList)
            {
                ChangePoints(transfer);
            }
            for (int i = 0; i < players.Count; i++)
            {
                var message = new ServerPointTransferMessage
                {
                    PlayerNames = names,
                    Points = CurrentRoundStatus.Points.ToArray(),
                    PointTransfers = PointTransferList.ToArray()
                };
                players[i].connectionToClient.Send(MessageIds.ServerPointTransferMessage, message);
            }
            responds = new bool[players.Count];
            firstTime = Time.time;
            serverTimeOut = ServerConstants.ServerPointTransferTimeOut;
        }

        private void OnNextMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientNextRoundMessage>();
            Debug.Log($"[Server] Received ClientNextRoundMessage: {content}");
            responds[content.PlayerIndex] = true;
        }

        public override void OnServerStateExit()
        {
        }

        public override void OnStateUpdate()
        {
            if (responds.All(r => r))
            {
                Debug.Log("[Server] All players has responded, start next round");
                StartNewRound();
                return;
            }
            if (Time.time - firstTime > serverTimeOut)
            {
                // time out
                Debug.Log("[Server] Server time out, start next round");
                StartNewRound();
                return;
            }
        }

        private void StartNewRound()
        {
            if (CheckIfGameEnds())
                ServerBehaviour.Instance.GameEnd();
            else
                ServerBehaviour.Instance.RoundStart(NextRound, ExtraRound, KeepSticks);
        }

        /// <summary>
        /// Check if game ends. When the game ends, return true, otherwise return false
        /// </summary>
        /// <returns></returns>
        private bool CheckIfGameEnds()
        {
            // check if allow zero or negative points
            int lowestPoint = CurrentRoundStatus.Points.Min();
            switch (CurrentRoundStatus.GameSettings.PointsToGameEnd)
            {
                case PointsToGameEnd.Zero:
                    if (lowestPoint <= 0) return true;
                    break;
                case PointsToGameEnd.Negative:
                    if (lowestPoint < 0) return true;
                    break;
            }
            if (CurrentRoundStatus.GameForceEnd) return true;
            var isAllLast = CurrentRoundStatus.IsAllLast;
            if (!isAllLast) return false;
            // is all last
            var maxPoint = CurrentRoundStatus.Points.Max();
            if (NextRound) // if next round
            {
                return maxPoint >= CurrentRoundStatus.GameSettings.FirstPlacePoints;
            }
            else // if not next -- same oya
            {
                if (maxPoint < CurrentRoundStatus.GameSettings.FirstPlacePoints)
                {
                    return false;
                }
                int playerIndex = CurrentRoundStatus.Points.IndexOf(maxPoint);
                if (playerIndex == CurrentRoundStatus.OyaPlayerIndex) // last oya is top
                {
                    return CurrentRoundStatus.GameSettings.GameEndsWhenAllLastTop;
                }
                return false;
            }
        }

        private void ChangePoints(PointTransfer transfer)
        {
            CurrentRoundStatus.ChangePoints(transfer.To, transfer.Amount);
            if (transfer.From >= 0)
                CurrentRoundStatus.ChangePoints(transfer.From, -transfer.Amount);
        }
    }
}