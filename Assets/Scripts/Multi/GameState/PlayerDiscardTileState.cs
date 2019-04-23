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
    public class PlayerDiscardTileState : IState
    {
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        public int CurrentPlayerIndex;
        public IList<Player> Players;
        public Tile DiscardTile;
        public bool IsRichiing;
        public bool DiscardLastDraw;
        public ServerRoundStatus CurrentRoundStatus;
        public MahjongSetData MahjongSetData;
        private Player currentPlayer;
        private MessageBase[] messages;
        private bool[] responds;
        private float lastSendTime;
        private float firstSendTime;
        private float serverTimeOut;
        private bool[] operationResponds;
        private OutTurnOperation[] outTurnOperations;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            NetworkServer.RegisterHandler(MessageIds.ClientOutTurnOperationMessage, OnOperationMessageReceived);
            if (CurrentRoundStatus.CurrentPlayerIndex != CurrentPlayerIndex)
            {
                Debug.LogError("[Server] currentPlayerIndex does not match, this should not happen");
                CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            }
            // UpdateCurrentPlayerData();
            messages = new MessageBase[Players.Count];
            responds = new bool[Players.Count];
            operationResponds = new bool[Players.Count];
            outTurnOperations = new OutTurnOperation[Players.Count];
            var rivers = CurrentRoundStatus.Rivers;
            // Get messages
            for (int i = 0; i < messages.Length; i++)
            {
                messages[i] = new ServerDiscardOperationMessage
                {
                    PlayerIndex = i,
                    CurrentTurnPlayerIndex = CurrentPlayerIndex,
                    IsRichiing = IsRichiing,
                    DiscardingLastDraw = DiscardLastDraw,
                    Tile = DiscardTile,
                    BonusTurnTime = Players[i].BonusTurnTime,
                    Operations = GetOperations(i),
                    HandTiles = CurrentRoundStatus.HandTiles(i),
                    Rivers = rivers
                };
            }
            // Send messages to players
            SendMessages();
            lastSendTime = Time.time;
            firstSendTime = Time.time;
            serverTimeOut = Players.Max(p => p.BonusTurnTime) + GameSettings.BaseTurnTime + ServerConstants.ServerTimeBuffer;
        }

        private void SendMessages()
        {
            // Send message to the current turn player
            for (int i = 0; i < Players.Count; i++)
            {
                if (responds[i]) continue;
                Players[i].connectionToClient.Send(MessageIds.ServerDiscardOperationMessage, messages[i]);
            }
        }

        // todo -- complete this
        private OutTurnOperation[] GetOperations(int playerIndex)
        {
            if (playerIndex == CurrentPlayerIndex) return new OutTurnOperation[] {
                new OutTurnOperation { Type = OutTurnOperationType.Skip}
            };
            // other players' operations
            var operations = new List<OutTurnOperation> {
                new OutTurnOperation { Type = OutTurnOperationType.Skip}
            };
            var point = GetRongInfo(playerIndex, DiscardTile);
            Debug.Log($"PointInfo: {point}");
            // test if enough
            if (point.FanWithoutDora >= GameSettings.MinimumFanConstraint)
            {
                operations.Add(new OutTurnOperation
                {
                    Type = OutTurnOperationType.Rong,
                    Tile = DiscardTile
                });
            }
            // test kong -- todo
            // test pong -- todo
            // test chow -- todo
            return operations.ToArray();
        }

        private PointInfo GetRongInfo(int playerIndex, Tile discard)
        {
            var baseHandStatus = HandStatus.Nothing;
            // test haidi
            if (MahjongSetData.TilesRemain == GameSettings.MountainReservedTiles)
                baseHandStatus |= HandStatus.Haidi;
            // test lingshang -- not gonna happen
            // just test if this player can claim rong, no need for dora
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, discard, baseHandStatus,
                null, null, YakuSettings);
            return point;
        }

        public void OnStateUpdate()
        {
            // Debug.Log($"Server is in {GetType().Name}");
            // Send messages again until get enough responds or time out
            if (Time.time - firstSendTime > serverTimeOut)
            {
                // Time out, entering next state
                for (int i = 0; i < operationResponds.Length; i++)
                {
                    if (!operationResponds[i])
                    {
                        Players[i].BonusTurnTime = 0;
                        outTurnOperations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                    }
                }
                ServerBehaviour.Instance.TurnEnd(CurrentPlayerIndex, DiscardTile, IsRichiing, outTurnOperations);
                return;
            }
            if (Time.time - lastSendTime > ServerConstants.MessageResendInterval && !responds.All(r => r))
            {
                lastSendTime = Time.time;
                SendMessages();
                return;
            }
            if (operationResponds.All(r => r))
            {
                Debug.Log("[Server] Server received all operation response, ending this turn.");
                ServerBehaviour.Instance.TurnEnd(CurrentPlayerIndex, DiscardTile, IsRichiing, outTurnOperations);
            }
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            Debug.Log($"[Server] Received ClientReadinessMessage: {content}");
            if (content.Content != MessageIds.ServerDiscardOperationMessage)
            {
                Debug.LogError("Something is wrong, the received readiness message contains invalid content.");
                return;
            }
            responds[content.PlayerIndex] = true;
        }

        private void OnOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientOutTurnOperationMessage>();
            Debug.Log($"[Server] Received ClientOperationMessage: {content}");
            operationResponds[content.PlayerIndex] = true;
            outTurnOperations[content.PlayerIndex] = content.Operation;
            Players[content.PlayerIndex].BonusTurnTime = content.BonusTurnTime;
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
            NetworkServer.UnregisterHandler(MessageIds.ClientOutTurnOperationMessage);
        }
    }
}