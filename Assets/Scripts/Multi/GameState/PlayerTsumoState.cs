using System.Collections.Generic;
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
        public GameSettings GameSettings;
        public int TsumoPlayerIndex;
        public List<Player> Players;
        public ServerRoundStatus CurrentRoundStatus;
        public MahjongSet MahjongSet;
        public PointInfo TsumoPointInfo;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            NetworkServer.RegisterHandler(MessageIds.ClientNextRoundMessage, OnNextRoundMessageReceived);
            var tsumoMessage = new ServerPlayerTsumoMessage
            {
                TsumoPlayerIndex = TsumoPlayerIndex,
                TsumoPlayerHandTiles = CurrentRoundStatus.HandTiles(TsumoPlayerIndex),
                TsumoPlayerOpenMelds = CurrentRoundStatus.OpenMelds(TsumoPlayerIndex),
                DoraIndicators = MahjongSet.DoraIndicators,
                IsRichi = CurrentRoundStatus.RichiStatus[TsumoPlayerIndex],
                UraDoraIndicators = MahjongSet.UraDoraIndicators,
                TsumoPointInfo = TsumoPointInfo
            };
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].connectionToClient.Send(MessageIds.ServerPlayerTsumoMessage, tsumoMessage);
            }
        }

        public void OnStateUpdate()
        {
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
    }
}