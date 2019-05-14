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
    public class PlayerDrawTileState : ServerState
    {
        public int CurrentPlayerIndex;
        public MahjongSet MahjongSet;
        public bool IsLingShang;
        public bool TurnDoraAfterDiscard;
        private Tile justDraw;
        private float firstSendTime;
        private float serverTimeOut;
        private PointInfo tsumoPointInfo;

        public override void OnServerStateEnter()
        {
            NetworkServer.RegisterHandler(MessageIds.ClientInTurnOperationMessage, OnInTurnOperationReceived);
            NetworkServer.RegisterHandler(MessageIds.ClientDiscardTileMessage, OnDiscardTileReceived);
            if (IsLingShang)
                justDraw = MahjongSet.DrawLingShang();
            else
                justDraw = MahjongSet.DrawTile();
            CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            CurrentRoundStatus.LastDraw = justDraw;
            CurrentRoundStatus.CheckFirstTurn(CurrentPlayerIndex);
            CurrentRoundStatus.BreakTempZhenting(CurrentPlayerIndex);
            Debug.Log($"[Server] Distribute a tile {justDraw} to current turn player {CurrentPlayerIndex}, "
                + $"first turn: {CurrentRoundStatus.FirstTurn}.");
            for (int i = 0; i < players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                var message = new ServerDrawTileMessage
                {
                    PlayerIndex = i,
                    DrawPlayerIndex = CurrentPlayerIndex,
                    Richied = CurrentRoundStatus.RichiStatus(CurrentPlayerIndex),
                    MahjongSetData = MahjongSet.Data
                };
                players[i].connectionToClient.Send(MessageIds.ServerDrawTileMessage, message);
            }
            players[CurrentPlayerIndex].connectionToClient.Send(MessageIds.ServerDrawTileMessage, new ServerDrawTileMessage
            {
                PlayerIndex = CurrentPlayerIndex,
                DrawPlayerIndex = CurrentPlayerIndex,
                Tile = justDraw,
                BonusTurnTime = players[CurrentPlayerIndex].BonusTurnTime,
                Richied = CurrentRoundStatus.RichiStatus(CurrentPlayerIndex),
                Zhenting = CurrentRoundStatus.IsZhenting(CurrentPlayerIndex),
                Operations = GetOperations(CurrentPlayerIndex),
                MahjongSetData = MahjongSet.Data
            });
            firstSendTime = Time.time;
            serverTimeOut = gameSettings.BaseTurnTime + players[CurrentPlayerIndex].BonusTurnTime + ServerConstants.ServerTimeBuffer;
        }

        private InTurnOperation[] GetOperations(int playerIndex)
        {
            var operations = new List<InTurnOperation> { new InTurnOperation { Type = InTurnOperationType.Discard } };
            // test tsumo
            TestTsumo(playerIndex, justDraw, operations);
            var handTiles = CurrentRoundStatus.HandTiles(playerIndex);
            var openMelds = CurrentRoundStatus.Melds(playerIndex);
            // test round draw
            Test9Orphans(handTiles, operations);
            // test richi
            TestRichi(playerIndex, handTiles, openMelds, operations);
            // test kongs
            TestKongs(playerIndex, handTiles, operations);
            // test bei -- todo
            return operations.ToArray();
        }

        private void TestTsumo(int playerIndex, Tile tile, IList<InTurnOperation> operations)
        {
            var baseHandStatus = HandStatus.Tsumo;
            // test haidi
            if (MahjongSet.TilesRemain == gameSettings.MountainReservedTiles)
                baseHandStatus |= HandStatus.Haidi;
            // test lingshang
            if (IsLingShang) baseHandStatus |= HandStatus.Lingshang;
            var allTiles = MahjongSet.AllTiles;
            var doraTiles = MahjongSet.DoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var uraDoraTiles = MahjongSet.UraDoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            tsumoPointInfo = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, tile, baseHandStatus,
                doraTiles, uraDoraTiles, yakuSettings);
            // test if enough
            if (gameSettings.CheckConstraint(tsumoPointInfo))
            {
                operations.Add(new InTurnOperation
                {
                    Type = InTurnOperationType.Tsumo,
                    Tile = justDraw
                });
            }
        }

        private void Test9Orphans(Tile[] handTiles, IList<InTurnOperation> operations)
        {
            if (!CurrentRoundStatus.FirstTurn) return;
            if (MahjongLogic.Test9KindsOfOrphans(handTiles, justDraw))
            {
                operations.Add(new InTurnOperation
                {
                    Type = InTurnOperationType.RoundDraw
                });
            }
        }

        private void TestRichi(int playerIndex, Tile[] handTiles, Meld[] openMelds, IList<InTurnOperation> operations)
        {
            var alreadyRichied = CurrentRoundStatus.RichiStatus(playerIndex);
            if (alreadyRichied) return;
            var availability = gameSettings.AllowRichiWhenPointsLow || CurrentRoundStatus.GetPoints(playerIndex) >= gameSettings.RichiMortgagePoints;
            if (!availability) return;
            IList<Tile> availableTiles;
            if (MahjongLogic.TestRichi(handTiles, openMelds, justDraw, gameSettings.AllowRichiWhenNotReady, out availableTiles))
            {
                operations.Add(new InTurnOperation
                {
                    Type = InTurnOperationType.Richi,
                    RichiAvailableTiles = availableTiles.ToArray()
                });
            }
        }

        private void TestKongs(int playerIndex, Tile[] handTiles, IList<InTurnOperation> operations)
        {
            if (CurrentRoundStatus.KongClaimed == MahjongConstants.MaxKongs) return; // no more kong can be claimed after 4 kongs claimed
            var alreadyRichied = CurrentRoundStatus.RichiStatus(playerIndex);
            if (alreadyRichied)
            {
                // test kongs in richied player hand -- todo
            }
            else
            {
                // 1. test self kongs, aka four same tiles in hand and lastdraw
                var selfKongs = MahjongLogic.GetSelfKongs(handTiles, justDraw);
                if (selfKongs.Any())
                {
                    foreach (var kong in selfKongs)
                    {
                        operations.Add(new InTurnOperation
                        {
                            Type = InTurnOperationType.Kong,
                            Meld = kong
                        });
                    }
                }
                // 2. test add kongs, aka whether a single tile in hand and lastdraw is identical to a pong in open melds
                var addKongs = MahjongLogic.GetAddKongs(
                    CurrentRoundStatus.HandTiles(playerIndex), CurrentRoundStatus.OpenMelds(playerIndex), justDraw);
                if (addKongs.Any())
                {
                    foreach (var kong in addKongs)
                    {
                        operations.Add(new InTurnOperation
                        {
                            Type = InTurnOperationType.Kong,
                            Meld = kong
                        });
                    }
                }
            }
        }

        private void OnDiscardTileReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientDiscardTileMessage>();
            if (content.PlayerIndex != CurrentRoundStatus.CurrentPlayerIndex)
            {
                Debug.Log($"[Server] It is not player {content.PlayerIndex}'s turn to discard a tile, ignoring this message");
                return;
            }
            // handle message
            Debug.Log($"[Server] Received ClientDiscardRequestMessage {content}");
            // Change to discardTileState
            ServerBehaviour.Instance.DiscardTile(
                content.PlayerIndex, content.Tile, content.IsRichiing,
                content.DiscardingLastDraw, content.BonusTurnTime, TurnDoraAfterDiscard);
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
            Debug.Log($"[Server] Received ClientInTurnOperationMessage: {content}");
            var operation = content.Operation;
            switch (operation.Type)
            {
                case InTurnOperationType.Tsumo:
                    HandleTsumo(operation);
                    break;
                case InTurnOperationType.Bei:
                // todo
                case InTurnOperationType.Kong:
                    HandleKong(operation);
                    break;
                case InTurnOperationType.RoundDraw:
                    HandleRoundDraw(operation);
                    break;
                default:
                    Debug.LogError($"[Server] This type of in turn operation should not be sent to server.");
                    break;
            }
        }

        private void HandleTsumo(InTurnOperation operation)
        {
            int playerIndex = CurrentRoundStatus.CurrentPlayerIndex;
            ServerBehaviour.Instance.Tsumo(playerIndex, operation.Tile, tsumoPointInfo);
        }

        private void HandleKong(InTurnOperation operation)
        {
            int playerIndex = CurrentRoundStatus.CurrentPlayerIndex;
            var kong = operation.Meld;
            Debug.Log($"Server is handling the operation of player {playerIndex} of claiming kong {kong}");
            ServerBehaviour.Instance.Kong(playerIndex, kong);
        }

        private void HandleRoundDraw(InTurnOperation operation)
        {
            int playerIndex = CurrentRoundStatus.CurrentPlayerIndex;
            Debug.Log($"Player {playerIndex} has claimed 9-orphans");
            ServerBehaviour.Instance.RoundDraw(RoundDrawType.NineOrphans);
        }

        public override void OnStateUpdate()
        {
            // Time out: auto discard
            if (Time.time - firstSendTime > serverTimeOut)
            {
                // force auto discard
                ServerBehaviour.Instance.DiscardTile(CurrentPlayerIndex, (Tile)CurrentRoundStatus.LastDraw, false, true, 0, TurnDoraAfterDiscard);
            }
        }

        public override void OnServerStateExit()
        {
            NetworkServer.UnregisterHandler(MessageIds.ClientInTurnOperationMessage);
            NetworkServer.UnregisterHandler(MessageIds.ClientDiscardTileMessage);
            CurrentRoundStatus.CheckOneShot(CurrentPlayerIndex);
        }
    }
}