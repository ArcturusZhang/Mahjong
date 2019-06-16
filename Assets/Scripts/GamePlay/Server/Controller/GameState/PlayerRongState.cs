using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GamePlay.Client.Controller;
using GamePlay.Server.Model;
using GamePlay.Server.Model.Events;
using Mahjong.Model;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GamePlay.Server.Controller.GameState
{
    public class PlayerRongState : ServerState, IOnEventCallback
    {
        public int CurrentPlayerIndex;
        public int[] RongPlayerIndices;
        public Tile WinningTile;
        public MahjongSet MahjongSet;
        public PointInfo[] RongPointInfos;
        private IList<PointTransfer> transfers;
        private bool[] responds;
        private float serverTimeOut;
        private float firstTime;
        private bool next;
        private const float ServerMaxTimeOut = 10;

        public override void OnServerStateEnter()
        {
            PhotonNetwork.AddCallbackTarget(this);
            responds = new bool[players.Count];
            var playerNames = RongPlayerIndices.Select(
                playerIndex => CurrentRoundStatus.GetPlayerName(playerIndex)
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
            var totalPoints = RongPointInfos.Select((info, i) => info.BasePoint * multipliers[i]).ToArray();
            var netInfos = RongPointInfos.Select(info => new NetworkPointInfo
            {
                Fu = info.Fu,
                YakuValues = info.YakuList.ToArray(),
                Dora = info.Dora,
                UraDora = info.UraDora,
                RedDora = info.RedDora,
                BeiDora = info.BeiDora,
                IsQTJ = info.IsQTJ
            }).ToArray();
            Debug.Log($"The following players are claiming rong: {string.Join(",", RongPlayerIndices)}, "
                + $"PlayerNames: {string.Join(",", playerNames)}");
            var rongInfo = new EventMessages.RongInfo
            {
                RongPlayerIndices = RongPlayerIndices,
                RongPlayerNames = playerNames,
                HandData = handData,
                WinningTile = WinningTile,
                DoraIndicators = MahjongSet.DoraIndicators,
                UraDoraIndicators = MahjongSet.UraDoraIndicators,
                RongPlayerRichiStatus = richiStatus,
                RongPointInfos = netInfos,
                TotalPoints = totalPoints
            };
            // send rpc calls
            ClientBehaviour.Instance.photonView.RPC("RpcRong", RpcTarget.AllBufferedViaServer, rongInfo);
            // get point transfers
            transfers = new List<PointTransfer>();
            for (int i = 0; i < RongPlayerIndices.Length; i++)
            {
                var rongPlayerIndex = RongPlayerIndices[i];
                var point = RongPointInfos[i];
                var multiplier = multipliers[i];
                int pointValue = point.BasePoint * multiplier;
                int extraPoints = i == 0 ? CurrentRoundStatus.ExtraPoints * (players.Count - 1) : 0;
                transfers.Add(new PointTransfer
                {
                    From = CurrentPlayerIndex,
                    To = rongPlayerIndex,
                    Amount = pointValue + extraPoints
                });
            }
            // richi-sticks-points
            transfers.Add(new PointTransfer
            {
                From = -1,
                To = RongPlayerIndices[0],
                Amount = CurrentRoundStatus.RichiSticksPoints
            });
            next = !RongPlayerIndices.Contains(CurrentRoundStatus.OyaPlayerIndex);
            // determine server time out
            serverTimeOut = ServerMaxTimeOut * RongPointInfos.Length + ServerConstants.ServerTimeBuffer;
            firstTime = Time.time;
        }

        public override void OnServerStateExit()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnStateUpdate()
        {
            if (Time.time - firstTime > serverTimeOut || responds.All(r => r))
            {
                PointTransfer();
                return;
            }
        }

        private void PointTransfer()
        {
            ServerBehaviour.Instance.PointTransfer(transfers, next, !next, false);
        }

        private void OnClientReadyEvent(int index)
        {
            responds[index] = true;
        }

        public void OnEvent(EventData photonEvent)
        {
            var code = photonEvent.Code;
            var info = photonEvent.CustomData;
            Debug.Log($"{GetType().Name} receives event code: {code} with content {info}");
            switch (code)
            {
                case EventMessages.ClientReadyEvent:
                    OnClientReadyEvent((int)photonEvent.CustomData);
                    break;
            }
        }
    }
}