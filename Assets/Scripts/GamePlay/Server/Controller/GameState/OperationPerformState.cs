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
    public class OperationPerformState : ServerState, IOnEventCallback
    {
        public int CurrentPlayerIndex;
        public int DiscardPlayerIndex;
        public OutTurnOperation Operation;
        public MahjongSet MahjongSet;
        private bool turnDoraAfterDiscard;
        private float firstSendTime;
        private float serverTimeOut;

        public override void OnServerStateEnter()
        {
            PhotonNetwork.AddCallbackTarget(this);
            // update hand data
            UpdateRoundStatus();
            // send messages
            for (int i = 0; i < players.Count; i++)
            {
                var info = GetInfo(i);
                var player = CurrentRoundStatus.GetPlayer(i);
                ClientBehaviour.Instance.photonView.RPC("RpcOperationPerform", player, info);
            }
            KongOperation();
            firstSendTime = Time.time;
            serverTimeOut = gameSettings.BaseTurnTime + CurrentRoundStatus.MaxBonusTurnTime + ServerConstants.ServerTimeBuffer;
        }

        private EventMessages.OperationPerformInfo GetInfo(int index)
        {
            if (index == CurrentPlayerIndex)
            {
                return new EventMessages.OperationPerformInfo
                {
                    PlayerIndex = CurrentPlayerIndex,
                    OperationPlayerIndex = CurrentPlayerIndex,
                    Operation = Operation,
                    HandData = CurrentRoundStatus.HandData(CurrentPlayerIndex),
                    BonusTurnTime = CurrentRoundStatus.GetBonusTurnTime(CurrentPlayerIndex),
                    Rivers = CurrentRoundStatus.Rivers,
                    MahjongSetData = MahjongSet.Data
                };
            }
            else
            {
                return new EventMessages.OperationPerformInfo
                {
                    PlayerIndex = index,
                    OperationPlayerIndex = CurrentPlayerIndex,
                    Operation = Operation,
                    HandData = new PlayerHandData
                    {
                        HandTiles = new Tile[CurrentRoundStatus.HandTiles(CurrentPlayerIndex).Length],
                        OpenMelds = CurrentRoundStatus.OpenMelds(CurrentPlayerIndex)
                    },
                    Rivers = CurrentRoundStatus.Rivers,
                    MahjongSetData = MahjongSet.Data
                };
            }
        }

        private void UpdateRoundStatus()
        {
            CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            // update hand tiles and open melds
            CurrentRoundStatus.RemoveFromRiver(DiscardPlayerIndex);
            CurrentRoundStatus.AddMeld(CurrentPlayerIndex, Operation.Meld);
            CurrentRoundStatus.RemoveTile(CurrentPlayerIndex, Operation.Meld);
            turnDoraAfterDiscard = Operation.Type == OutTurnOperationType.Kong;
        }

        private void KongOperation()
        {
            if (Operation.Type != OutTurnOperationType.Kong) return;
            ServerBehaviour.Instance.DrawTile(CurrentPlayerIndex, true, turnDoraAfterDiscard);
        }

        private void OnDiscardTileEvent(EventMessages.DiscardTileInfo info)
        {
            if (info.PlayerIndex != CurrentRoundStatus.CurrentPlayerIndex)
            {
                Debug.Log($"[Server] It is not player {info.PlayerIndex}'s turn to discard a tile, ignoring this message");
                return;
            }
            // Change to discardTileState
            ServerBehaviour.Instance.DiscardTile(
                info.PlayerIndex, info.Tile, info.IsRichiing,
                info.DiscardingLastDraw, info.BonusTurnTime, turnDoraAfterDiscard);
        }

        public override void OnServerStateExit()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnStateUpdate()
        {
            // time out: auto discard
            if (Time.time - firstSendTime > serverTimeOut)
            {
                // force auto discard
                var tiles = CurrentRoundStatus.HandTiles(CurrentPlayerIndex);
                ServerBehaviour.Instance.DiscardTile(CurrentPlayerIndex, tiles[tiles.Length - 1], false, false, 0, turnDoraAfterDiscard);
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            var code = photonEvent.Code;
            var info = photonEvent.CustomData;
            Debug.Log($"{GetType().Name} receives event code: {code} with content {info}");
            switch (code)
            {
                case EventMessages.DiscardTileEvent:
                    OnDiscardTileEvent((EventMessages.DiscardTileInfo)info);
                    break;
            }
        }
    }
}