using System.Collections.Generic;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class OperationPerformState : ServerState
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
            NetworkServer.RegisterHandler(MessageIds.ClientDiscardTileMessage, OnDiscardMessageReceived);
            // update hand data
            UpdateRoundStatus();
            // send messages
            for (int i = 0; i < players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                var message = new ServerOperationPerformMessage
                {
                    PlayerIndex = i,
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
                players[i].connectionToClient.Send(MessageIds.ServerOperationPerformMessage, message);
            }
            players[CurrentPlayerIndex].connectionToClient.Send(MessageIds.ServerOperationPerformMessage, new ServerOperationPerformMessage
            {
                PlayerIndex = CurrentPlayerIndex,
                OperationPlayerIndex = CurrentPlayerIndex,
                Operation = Operation,
                HandData = CurrentRoundStatus.HandData(CurrentPlayerIndex),
                BonusTurnTime = players[CurrentPlayerIndex].BonusTurnTime,
                Rivers = CurrentRoundStatus.Rivers,
                MahjongSetData = MahjongSet.Data
            });
            KongOperation();
            firstSendTime = Time.time;
            serverTimeOut = gameSettings.BaseTurnTime + players[CurrentPlayerIndex].BonusTurnTime + ServerConstants.ServerTimeBuffer;
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

        private void OnDiscardMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientDiscardTileMessage>();
            if (content.PlayerIndex != CurrentRoundStatus.CurrentPlayerIndex)
            {
                Debug.Log($"[Server] It is not player {content.PlayerIndex}'s turn to discard a tile, ignoring this message");
                return;
            }
            // handle message
            Debug.Log($"[Server] received ClientDiscardRequestMessage: {content}");
            // Change to discardTileState
            ServerBehaviour.Instance.DiscardTile(
                content.PlayerIndex, content.Tile, content.IsRichiing,
                content.DiscardingLastDraw, content.BonusTurnTime, turnDoraAfterDiscard);
        }

        public override void OnServerStateExit()
        {
            NetworkServer.UnregisterHandler(MessageIds.ClientDiscardTileMessage);
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
    }
}