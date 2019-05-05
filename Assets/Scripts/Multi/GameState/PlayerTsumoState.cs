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
        private IList<PointTransfer> transfers;
        private bool[] responds;
        private float serverTimeOut;
        private float firstTime;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            players = CurrentRoundStatus.Players;
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            int multiplier = gameSettings.GetMultiplier(CurrentRoundStatus.IsDealer(TsumoPlayerIndex), players.Count);
            var netInfo = new NetworkPointInfo
            {
                Fu = TsumoPointInfo.Fu,
                YakuValues = TsumoPointInfo.YakuList.ToArray(),
                Dora = TsumoPointInfo.Dora,
                UraDora = TsumoPointInfo.UraDora,
                RedDora = TsumoPointInfo.RedDora,
                IsQTJ = TsumoPointInfo.IsQTJ
            };
            var tsumoMessage = new ServerPlayerTsumoMessage
            {
                TsumoPlayerIndex = TsumoPlayerIndex,
                TsumoPlayerName = players[TsumoPlayerIndex].PlayerName,
                TsumoHandData = CurrentRoundStatus.HandData(TsumoPlayerIndex),
                WinningTile = WinningTile,
                DoraIndicators = MahjongSet.DoraIndicators,
                UraDoraIndicators = MahjongSet.UraDoraIndicators,
                IsRichi = CurrentRoundStatus.RichiStatus(TsumoPlayerIndex),
                TsumoPointInfo = netInfo,
                TotalPoints = TsumoPointInfo.BasePoint * multiplier
            };
            for (int i = 0; i < players.Count; i++)
            {
                players[i].connectionToClient.Send(MessageIds.ServerTsumoMessage, tsumoMessage);
            }
            // get point transfers
            // todo -- tsumo loss related, now there is tsumo loss by default
            transfers = new List<PointTransfer>();
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                if (playerIndex == TsumoPlayerIndex) continue;
                int amount = TsumoPointInfo.BasePoint;
                if (CurrentRoundStatus.IsDealer(playerIndex)) amount *= 2;
                int extraPoints = CurrentRoundStatus.ExtraPoints;
                transfers.Add(new PointTransfer
                {
                    From = playerIndex,
                    To = TsumoPlayerIndex,
                    Amount = amount + extraPoints
                });
            }
            // richi-sticks-points
            transfers.Add(new PointTransfer
            {
                From = -1,
                To = TsumoPlayerIndex,
                Amount = CurrentRoundStatus.RichiSticksPoints
            });
            responds = new bool[players.Count];
            // determine server time out
            serverTimeOut = MahjongConstants.SummaryPanelDelayTime * TsumoPointInfo.YakuList.Count
                + MahjongConstants.SummaryPanelWaitingTime
                + ServerConstants.ServerTimeBuffer;
            firstTime = Time.time;
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            Debug.Log($"[Server] Received ClientReadinessMessage: {content}");
            if (content.Content != MessageIds.ServerPointTransferMessage)
            {
                Debug.LogError("The message contains invalid content.");
                return;
            }
            responds[content.PlayerIndex] = true;
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
        }

        public void OnStateUpdate()
        {
            if (responds.All(r => r))
            {
                PointTransfer();
                return;
            }
            if (Time.time - firstTime > serverTimeOut)
            {
                PointTransfer();
                return;
            }
        }

        private void PointTransfer()
        {
            ServerBehaviour.Instance.PointTransfer(transfers, false, true, false);
        }
    }
}