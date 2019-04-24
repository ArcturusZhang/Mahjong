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
    public class PlayerTsumoState : IState
    {
        public int TsumoPlayerIndex;
        public ServerRoundStatus CurrentRoundStatus;
        public Tile WinningTile;
        public MahjongSet MahjongSet;
        public PointInfo TsumoPointInfo;
        private GameSettings gameSettings;
        private IList<Player> players;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            players = CurrentRoundStatus.Players;
            NetworkServer.RegisterHandler(MessageIds.ClientNextRoundMessage, OnNextRoundMessageReceived);
            int multiplier = gameSettings.GetMultiplier(CurrentRoundStatus.IsDealer(TsumoPlayerIndex), players.Count);
            var netInfo = new NetworkPointInfo {
                Fu = TsumoPointInfo.Fu,
                YakuValues = TsumoPointInfo.YakuList.ToArray(),
                Dora = TsumoPointInfo.Dora,
                UraDora = TsumoPointInfo.UraDora,
                RedDora = TsumoPointInfo.RedDora,
                IsQTJ = TsumoPointInfo.Is青天井
            };
            var tsumoMessage = new ServerPlayerTsumoMessage
            {
                TsumoPlayerIndex = TsumoPlayerIndex,
                TsumoPlayerName = players[TsumoPlayerIndex].PlayerName,
                TsumoHandData = CurrentRoundStatus.HandData(TsumoPlayerIndex),
                WinningTile = WinningTile,
                DoraIndicators = MahjongSet.DoraIndicators,
                IsRichi = CurrentRoundStatus.RichiStatus[TsumoPlayerIndex],
                UraDoraIndicators = MahjongSet.UraDoraIndicators,
                TsumoPointInfo = netInfo,
                Multiplier = multiplier
            };
            for (int i = 0; i < players.Count; i++)
            {
                players[i].connectionToClient.Send(MessageIds.ServerTsumoMessage, tsumoMessage);
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
            NetworkServer.UnregisterHandler(MessageIds.ClientNextRoundMessage);
        }

        public void OnStateUpdate()
        {
        }
    }
}