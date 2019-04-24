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
        public int CurrentPlayerIndex;
        public MahjongSet MahjongSet;
        public ServerRoundStatus CurrentRoundStatus;
        private GameSettings gameSettings;
        private YakuSettings yakuSettings;
        private IList<Player> players;
        private Tile justDraw;
        private MessageBase[] messages;
        private bool[] responds;
        private float lastSendTime;
        private float firstSendTime;
        private float serverTimeOut;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            yakuSettings = CurrentRoundStatus.YakuSettings;
            players = CurrentRoundStatus.Players;
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            NetworkServer.RegisterHandler(MessageIds.ClientInTurnOperationMessage, OnInTurnOperationReceived);
            NetworkServer.RegisterHandler(MessageIds.ClientDiscardTileMessage, OnDiscardTileReceived);
            justDraw = MahjongSet.DrawTile();
            Debug.Log($"[Server] Distribute a tile {justDraw} to current turn player {CurrentPlayerIndex}.");
            CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            CurrentRoundStatus.LastDraw = justDraw;
            messages = new MessageBase[players.Count];
            responds = new bool[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                messages[i] = new ServerOtherDrawTileMessage
                {
                    PlayerIndex = i,
                    CurrentTurnPlayerIndex = CurrentPlayerIndex,
                    MahjongSetData = MahjongSet.Data
                };
                players[i].connectionToClient.Send(MessageIds.ServerOtherDrawTileMessage, messages[i]);
            }
            messages[CurrentPlayerIndex] = new ServerDrawTileMessage
            {
                PlayerIndex = CurrentPlayerIndex,
                Tile = justDraw,
                BonusTurnTime = players[CurrentPlayerIndex].BonusTurnTime,
                Operations = GetOperations(CurrentPlayerIndex),
                MahjongSetData = MahjongSet.Data
            };
            players[CurrentPlayerIndex].connectionToClient.Send(MessageIds.ServerDrawTileMessage, messages[CurrentPlayerIndex]);
            firstSendTime = Time.time;
            lastSendTime = Time.time;
            serverTimeOut = gameSettings.BaseTurnTime + players[CurrentPlayerIndex].BonusTurnTime + ServerConstants.ServerTimeBuffer;
        }

        // todo -- complete this
        private InTurnOperation[] GetOperations(int playerIndex)
        {
            var operations = new List<InTurnOperation> { new InTurnOperation { Type = InTurnOperationType.Discard } };
            var point = GetTsumoInfo(playerIndex, justDraw);
            // test if enough
            if (gameSettings.CheckConstraint(point))
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
            var point = GetTsumoInfo(playerIndex, operation.Tile);
            if (gameSettings.CheckConstraint(point))
                Debug.LogError($"Tsumo requires minimum fan of {gameSettings.MinimumFanConstraintType}, but the point only contains {point.FanWithoutDora}");
            ServerBehaviour.Instance.HandleTsumo(playerIndex, operation.Tile, point);
        }

        private PointInfo GetTsumoInfo(int playerIndex, Tile tile)
        {
            var baseHandStatus = HandStatus.Tsumo;
            // test haidi
            if (MahjongSet.TilesRemain == gameSettings.MountainReservedTiles)
                baseHandStatus |= HandStatus.Haidi;
            // test lingshang -- todo
            var allTiles = MahjongSet.AllTiles;
            var doraTiles = MahjongSet.DoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var uraDoraTiles = MahjongSet.UraDoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, tile, baseHandStatus,
                doraTiles, uraDoraTiles, yakuSettings);
            return point;
        }

        public void OnStateUpdate()
        {
            // Debug.Log($"Server is in {GetType().Name}");
            // Sending messages until received all responds from all players
            if (Time.time - lastSendTime > ServerConstants.MessageResendInterval && !responds.All(r => r))
            {
                // resend message
                for (int i = 0; i < players.Count; i++)
                {
                    if (responds[i]) continue;
                    players[i].connectionToClient.Send(
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