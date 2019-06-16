using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GamePlay.Client.Controller;
using GamePlay.Server.Model;
using GamePlay.Server.Model.Events;
using Mahjong.Logic;
using Mahjong.Model;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GamePlay.Server.Controller.GameState
{
    public class PlayerDrawTileState : ServerState, IOnEventCallback
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
            PhotonNetwork.AddCallbackTarget(this);
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
            var room = PhotonNetwork.CurrentRoom;
            for (int i = 0; i < players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                var info = new EventMessages.DrawTileInfo
                {
                    PlayerIndex = i,
                    DrawPlayerIndex = CurrentPlayerIndex,
                    MahjongSetData = MahjongSet.Data
                };
                var player = CurrentRoundStatus.GetPlayer(i);
                ClientBehaviour.Instance.photonView.RPC("RpcDrawTile", player, info);
            }
            var currentPlayer = CurrentRoundStatus.GetPlayer(CurrentPlayerIndex);
            ClientBehaviour.Instance.photonView.RPC("RpcDrawTile", currentPlayer, new EventMessages.DrawTileInfo
            {
                PlayerIndex = CurrentPlayerIndex,
                DrawPlayerIndex = CurrentPlayerIndex,
                Tile = justDraw,
                BonusTurnTime = CurrentRoundStatus.GetBonusTurnTime(CurrentPlayerIndex),
                Zhenting = CurrentRoundStatus.IsZhenting(CurrentPlayerIndex),
                Operations = GetOperations(CurrentPlayerIndex),
                MahjongSetData = MahjongSet.Data
            });
            firstSendTime = Time.time;
            serverTimeOut = gameSettings.BaseTurnTime
                + CurrentRoundStatus.GetBonusTurnTime(CurrentPlayerIndex)
                + ServerConstants.ServerTimeBuffer;
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
            // test bei
            TestBei(playerIndex, handTiles, operations);
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
            var beiDora = CurrentRoundStatus.GetBeiDora(playerIndex);
            tsumoPointInfo = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, tile, baseHandStatus,
                doraTiles, uraDoraTiles, beiDora, gameSettings);
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
            if (!gameSettings.Allow9OrphanDraw) return;
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
                // test kongs in richied player hand
                var richiKongs = MahjongLogic.GetRichiKongs(handTiles, justDraw);
                if (richiKongs.Any())
                {
                    foreach (var kong in richiKongs)
                    {
                        operations.Add(new InTurnOperation
                        {
                            Type = InTurnOperationType.Kong,
                            Meld = kong
                        });
                    }
                }
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

        public void TestBei(int playerIndex, Tile[] handTiles, IList<InTurnOperation> operations)
        {
            if (!gameSettings.AllowBeiDora) return;
            var beiTile = new Tile(Suit.Z, 4);
            int bei = handTiles.Count(tile => tile.EqualsIgnoreColor(beiTile));
            if (bei > 0)
            {
                operations.Add(new InTurnOperation
                {
                    Type = InTurnOperationType.Bei
                });
            }
            else if (justDraw.EqualsIgnoreColor(beiTile))
            {
                operations.Add(new InTurnOperation
                {
                    Type = InTurnOperationType.Bei
                });
            }
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
                info.DiscardingLastDraw, info.BonusTurnTime, TurnDoraAfterDiscard);
        }

        private void OnInTurnOperationEvent(EventMessages.InTurnOperationInfo info)
        {
            if (info.PlayerIndex != CurrentRoundStatus.CurrentPlayerIndex)
            {
                Debug.Log($"[Server] It is not player {info.PlayerIndex}'s turn to perform a in turn operation, ignoring this message");
                return;
            }
            // handle message according to its type
            var operation = info.Operation;
            switch (operation.Type)
            {
                case InTurnOperationType.Tsumo:
                    HandleTsumo(operation);
                    break;
                case InTurnOperationType.Kong:
                    HandleKong(operation);
                    break;
                case InTurnOperationType.RoundDraw:
                    HandleRoundDraw(operation);
                    break;
                case InTurnOperationType.Bei:
                    HandleBei(operation);
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

        private void HandleBei(InTurnOperation operation)
        {
            int playerIndex = CurrentRoundStatus.CurrentPlayerIndex;
            Debug.Log($"Player {playerIndex} has claimed a bei-dora");
            ServerBehaviour.Instance.BeiDora(playerIndex);
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
            PhotonNetwork.RemoveCallbackTarget(this);
            CurrentRoundStatus.CheckOneShot(CurrentPlayerIndex);
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
                case EventMessages.InTurnOperationEvent:
                    OnInTurnOperationEvent((EventMessages.InTurnOperationInfo)info);
                    break;
            }
        }
    }
}