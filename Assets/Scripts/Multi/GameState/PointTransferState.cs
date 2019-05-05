using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PointTransferState : IState
    {
        public ServerRoundStatus CurrentRoundStatus;
        public IList<PointTransfer> PointTransferList;
        public bool NextRound;
        public bool ExtraRound;
        public bool KeepSticks;
        private IList<Player> players;
        private ServerPointTransferMessage[] messages;
        private bool[] responds;
        private float firstTime;
        private float serverTimeOut;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            Debug.Log($"[Server] Transfers: {string.Join(", ", PointTransferList)}");
            NetworkServer.RegisterHandler(MessageIds.ClientNextRoundMessage, OnNextMessageReceived);
            players = CurrentRoundStatus.Players;
            messages = new ServerPointTransferMessage[players.Count];
            var names = players.Select(player => player.PlayerName).ToArray();
            // update points of each player
            foreach (var transfer in PointTransferList)
            {
                ChangePoints(transfer);
            }
            for (int i = 0; i < players.Count; i++)
            {
                messages[i] = new ServerPointTransferMessage
                {
                    PlayerNames = names,
                    Points = CurrentRoundStatus.Points.ToArray(),
                    PointTransfers = PointTransferList.ToArray()
                };
                players[i].connectionToClient.Send(MessageIds.ServerPointTransferMessage, messages[i]);
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

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }

        public void OnStateUpdate()
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
            ServerBehaviour.Instance.RoundStart(NextRound, ExtraRound, KeepSticks);
        }

        private void ChangePoints(PointTransfer transfer)
        {
            CurrentRoundStatus.ChangePoints(transfer.To, transfer.Amount);
            if (transfer.From >= 0)
                CurrentRoundStatus.ChangePoints(transfer.From, -transfer.Amount);
        }
    }
}