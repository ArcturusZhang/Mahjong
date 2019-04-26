using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerRongState : IState
    {
        public int CurrentPlayerIndex;
        public ServerRoundStatus CurrentRoundStatus;
        public int[] RongPlayerIndices;
        public Tile WinningTile;
        public MahjongSet MahjongSet;
        public PointInfo[] RongPointInfos;
        private GameSettings gameSettings;
        private IList<Player> players;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            players = CurrentRoundStatus.Players;
            NetworkServer.RegisterHandler(MessageIds.ClientNextRoundMessage, OnNextRoundMessageReceived);
            var playerNames = RongPlayerIndices.Select(
                playerIndex => players[playerIndex].PlayerName
            ).ToArray();
            var handData = RongPlayerIndices.Select(
                playerIndex => CurrentRoundStatus.HandData(playerIndex)
            ).ToArray();
            var richiStatus = RongPlayerIndices.Select(
                playerIndex => CurrentRoundStatus.RichiStatus(playerIndex)
            ).ToArray();
            var multipliers = RongPlayerIndices.Select(
                playerIndex => gameSettings.GetMultiplier(CurrentRoundStatus.IsDealer(playerIndex), players.Count)
            ).ToArray();
            var netInfos = RongPointInfos.Select(info => new NetworkPointInfo
            {
                Fu = info.Fu,
                YakuValues = info.YakuList.ToArray(),
                Dora = info.Dora,
                UraDora = info.UraDora,
                RedDora = info.RedDora,
                IsQTJ = info.IsQTJ
            }).ToArray();
            Debug.Log($"The following players are claiming rong: {string.Join(",", RongPlayerIndices)}, "
                + $"PlayerNames: {string.Join(",", playerNames)}");
            var rongMessage = new ServerPlayerRongMessage
            {
                RongPlayerIndices = RongPlayerIndices,
                RongPlayerNames = playerNames,
                HandData = handData,
                WinningTile = WinningTile,
                DoraIndicators = MahjongSet.DoraIndicators,
                UraDoraIndicators = MahjongSet.UraDoraIndicators,
                RichiStatus = CurrentRoundStatus.RichiStatusArray,
                RongPointInfos = netInfos,
                Multipliers = multipliers
            };
            for (int i = 0; i < players.Count; i++)
            {
                players[i].connectionToClient.Send(MessageIds.ServerRongMessage, rongMessage);
            }
        }

        private void OnNextRoundMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientNextRoundMessage>();
            Debug.Log($"[Server] Received ClientNextRoundMessage: {content}");
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