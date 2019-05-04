using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using StateMachine.Interfaces;
using UnityEngine;

namespace Multi.GameState
{
    public class PointTransferState : IState
    {
        // todo -- add round end type: tsumo, rong, or draw
        public ServerRoundStatus CurrentRoundStatus;
        public IList<PointTransfer> PointTransferList;
        private IList<Player> players;
        private ServerPointTransferMessage[] messages;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            Debug.Log($"[Server] Transfers: {string.Join(", ", PointTransferList)}");
            // todo -- receiving PointTransferDoneMessage
            players = CurrentRoundStatus.Players;
            messages = new ServerPointTransferMessage[players.Count];
            var names = players.Select(player => player.PlayerName).ToArray();
            for (int i = 0; i < players.Count; i++)
            {
                messages[i] = new ServerPointTransferMessage
                {
                    PointTransfers = PointTransferList.ToArray(),
                    PlayerNames = names
                };
                players[i].connectionToClient.Send(MessageIds.ServerPointTransferMessage, messages[i]);
            }
            // todo -- update points of each player
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }

        public void OnStateUpdate()
        {
        }
    }
}