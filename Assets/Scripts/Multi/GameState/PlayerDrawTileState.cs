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
    // todo -- draw lingshang tile
    public class PlayerDrawTileState : IState
    {
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        public int CurrentPlayerIndex;
        public IList<Player> Players;
        public MahjongSet MahjongSet;
        public ServerRoundStatus CurrentRoundStatus;
        private Tile justDraw;
        private MessageBase[] messages;
        private bool[] responds;
        private float lastSendTime;
        private float firstSendTime;
        private float serverTimeOut;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            NetworkServer.RegisterHandler(MessageIds.ClientInTurnOperationMessage, OnInTurnOperationReceived);
            NetworkServer.RegisterHandler(MessageIds.ClientDiscardTileMessage, OnDiscardTileReceived);
            justDraw = MahjongSet.DrawTile();
            Debug.Log($"[Server] Distribute a tile {justDraw} to current turn player {CurrentPlayerIndex}.");
            CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            CurrentRoundStatus.LastDraw = justDraw;
            messages = new MessageBase[Players.Count];
            responds = new bool[Players.Count];
            for (int i = 0; i < Players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                messages[i] = new ServerOtherDrawTileMessage
                {
                    PlayerIndex = i,
                    CurrentTurnPlayerIndex = CurrentPlayerIndex,
                    MahjongSetData = MahjongSet.Data
                };
                Players[i].connectionToClient.Send(MessageIds.ServerOtherDrawTileMessage, messages[i]);
            }
            messages[CurrentPlayerIndex] = new ServerDrawTileMessage
            {
                PlayerIndex = CurrentPlayerIndex,
                Tile = justDraw,
                BonusTurnTime = Players[CurrentPlayerIndex].BonusTurnTime,
                Operations = GetOperations(CurrentPlayerIndex),
                MahjongSetData = MahjongSet.Data
            };
            Players[CurrentPlayerIndex].connectionToClient.Send(MessageIds.ServerDrawTileMessage, messages[CurrentPlayerIndex]);
            firstSendTime = Time.time;
            lastSendTime = Time.time;
            serverTimeOut = GameSettings.BaseTurnTime + Players[CurrentPlayerIndex].BonusTurnTime + ServerConstants.ServerTimeBuffer;
        }

        // todo -- complete this
        private InTurnOperation[] GetOperations(int playerIndex)
        {
            var operations = new List<InTurnOperation> { new InTurnOperation { Type = InTurnOperationType.Discard } };
            var point = GetPointInfo(playerIndex, HandStatus.Tsumo, justDraw);
            // test if enough
            if (point.FanWithoutDora >= GameSettings.MinimumFanConstraint)
            {
                operations.Add(new InTurnOperation
                {
                    Type = InTurnOperationType.Tsumo,
                    Tile = justDraw
                });
            }
            // test richi -- todo
            // test kong -- todo
            // test bei -- todo
            return operations.ToArray();
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            Debug.Log($"[Server] Received ClientReadinessMessage: {content}");
            if (content.Content != MessageIds.ServerDrawTileMessage)
            {
                Debug.LogError("Something is wrong, the received readiness message contains invalid content.");
                return;
            }
            responds[content.PlayerIndex] = true;
        }

        private void OnDiscardTileReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientDiscardRequestMessage>();
            if (content.PlayerIndex != CurrentRoundStatus.CurrentPlayerIndex)
            {
                Debug.Log($"[Server] It is not player {content.PlayerIndex}'s turn to discard a tile, ignoring this message");
                return;
            }
            // handle message
            Debug.Log($"[Server] Received ClientDiscardRequestMessage {content}");
            // Change to discardTileState
            ServerBehaviour.Instance.DiscardTile(content.PlayerIndex, content.Tile, content.IsRichiing, content.DiscardingLastDraw, content.BonusTurnTime);
        }

        private void OnInTurnOperationReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientInTurnOperationMessage>();
            if (content.PlayerIndex != CurrentRoundStatus.CurrentPlayerIndex)
            {
                Debug.Log($"[Server] It is not player {content.PlayerIndex}'s turn to perform a in turn operation, ignoring this message");
                return;
            }
            // handle message according to its type
            Debug.Log($"[Server] Received ClientInTurnOperationMessage {content}");
            var operation = content.Operation;
            switch (operation.Type)
            {
                case InTurnOperationType.Tsumo:
                    HandleTsumo(operation);
                    break;
                case InTurnOperationType.Bei:
                case InTurnOperationType.Kong:
                    // todo
                    break;
                default:
                    Debug.LogError($"[Server] This type of in turn operation should not be sent to server.");
                    break;
            }
        }

        private void HandleTsumo(InTurnOperation operation)
        {
            int playerIndex = CurrentRoundStatus.CurrentPlayerIndex;
            var point = GetPointInfo(playerIndex, HandStatus.Tsumo, operation.Tile);
            if (point.FanWithoutDora < GameSettings.MinimumFanConstraint)
                Debug.LogError($"Tsumo requires minimum fan of {GameSettings.MinimumFanConstraint}, but the point only contains {point.FanWithoutDora} fans");
            ServerBehaviour.Instance.HandleTsumo(playerIndex, point);
        }

        private PointInfo GetPointInfo(int playerIndex, HandStatus baseHandStatus, Tile tile)
        {
            var handTiles = CurrentRoundStatus.HandTiles(playerIndex);
            var openMelds = CurrentRoundStatus.OpenMelds(playerIndex);
            var handStatus = baseHandStatus;
            if (MahjongLogic.TestMenqing(openMelds))
                handStatus |= HandStatus.Menqing;
            // test richi
            if (CurrentRoundStatus.RichiStatus[playerIndex])
            {
                handStatus |= HandStatus.Richi;
                // test one-shot
                if (CurrentRoundStatus.OneShotStatus[playerIndex])
                    handStatus |= HandStatus.OneShot;
                // test WRichi -- todo
            }
            // test first turn -- todo
            // test lingshang -- todo
            // test haidi
            if (MahjongSet.TilesRemain == GameSettings.MountainReservedTiles)
                handStatus |= HandStatus.Haidi;
            var roundStatus = new RoundStatus
            {
                PlayerIndex = playerIndex,
                OyaPlayerIndex = CurrentRoundStatus.OyaPlayerIndex,
                CurrentExtraRound = CurrentRoundStatus.Extra,
                RichiSticks = CurrentRoundStatus.RichiSticks,
                FieldCount = CurrentRoundStatus.Field,
                TotalPlayer = Players.Count
            };
            var allTiles = MahjongSet.AllTiles;
            var doraTiles = MahjongSet.DoraIndicators.Select(indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var uraDoraTiles = MahjongSet.UraDoraIndicators.Select(indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var point = MahjongLogic.GetPointInfo(handTiles, openMelds, tile,
                handStatus, roundStatus, YakuSettings, doraTiles, uraDoraTiles);
            Debug.Log($"[Server] HandTiles: {string.Join("", handTiles)}\n"
                + $"OpenMelds: {string.Join(",", openMelds)}\n"
                + $"PointInfo: {point}");
            return point;
        }

        public void OnStateUpdate()
        {
            // Debug.Log($"Server is in {GetType().Name}");
            // Sending messages until received all responds from all players
            if (Time.time - lastSendTime > ServerConstants.MessageResendInterval && !responds.All(r => r))
            {
                // resend message
                for (int i = 0; i < Players.Count; i++)
                {
                    if (responds[i]) continue;
                    Players[i].connectionToClient.Send(
                        i == CurrentPlayerIndex ? MessageIds.ServerDrawTileMessage : MessageIds.ServerOtherDrawTileMessage,
                        messages[i]);
                }
            }
            // Time out
            if (Time.time - firstSendTime > serverTimeOut)
            {
                // force auto discard
                ServerBehaviour.Instance.DiscardTile(CurrentPlayerIndex, (Tile)CurrentRoundStatus.LastDraw, false, true, 0);
            }
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
            NetworkServer.UnregisterHandler(MessageIds.ClientDiscardTileMessage);
        }
    }
}