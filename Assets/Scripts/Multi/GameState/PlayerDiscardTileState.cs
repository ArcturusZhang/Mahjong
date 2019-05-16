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
    public class PlayerDiscardTileState : ServerState
    {
        public int CurrentPlayerIndex;
        public Tile DiscardTile;
        public bool IsRichiing;
        public bool DiscardLastDraw;
        public int BonusTurnTime;
        public MahjongSet MahjongSet;
        public bool TurnDoraAfterDiscard;
        private float firstSendTime;
        private float serverTimeOut;
        private bool[] responds;
        private OutTurnOperation[] operations;

        public override void OnServerStateEnter()
        {
            NetworkServer.RegisterHandler(MessageIds.ClientOutTurnOperationMessage, OnOperationMessageReceived);
            if (CurrentRoundStatus.CurrentPlayerIndex != CurrentPlayerIndex)
            {
                Debug.LogError("[Server] currentPlayerIndex does not match, this should not happen");
                CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            }
            UpdateHandData();
            Debug.Log($"CurrentRoundStatus: {CurrentRoundStatus}");
            responds = new bool[players.Count];
            operations = new OutTurnOperation[players.Count];
            var rivers = CurrentRoundStatus.Rivers;
            // Get messages and send them to players
            for (int i = 0; i < players.Count; i++)
            {
                var message = new ServerDiscardOperationMessage
                {
                    PlayerIndex = i,
                    CurrentTurnPlayerIndex = CurrentPlayerIndex,
                    IsRichiing = IsRichiing,
                    DiscardingLastDraw = DiscardLastDraw,
                    Tile = DiscardTile,
                    BonusTurnTime = players[i].BonusTurnTime,
                    Operations = GetOperations(i),
                    HandTiles = CurrentRoundStatus.HandTiles(i),
                    Rivers = rivers
                };
                players[i].connectionToClient.Send(MessageIds.ServerDiscardOperationMessage, message);
            }
            firstSendTime = Time.time;
            serverTimeOut = players.Max(p => p.BonusTurnTime) + gameSettings.BaseTurnTime + ServerConstants.ServerTimeBuffer;
        }

        private void UpdateHandData()
        {
            players[CurrentPlayerIndex].BonusTurnTime = BonusTurnTime;
            var lastDraw = CurrentRoundStatus.LastDraw;
            CurrentRoundStatus.LastDraw = null;
            if (!DiscardLastDraw)
            {
                CurrentRoundStatus.RemoveTile(CurrentPlayerIndex, DiscardTile);
                if (lastDraw != null)
                    CurrentRoundStatus.AddTile(CurrentPlayerIndex, (Tile)lastDraw);
            }
            CurrentRoundStatus.AddToRiver(CurrentPlayerIndex, DiscardTile, IsRichiing);
            CurrentRoundStatus.SortHandTiles();
        }

        private OutTurnOperation[] GetOperations(int playerIndex)
        {
            if (playerIndex == CurrentPlayerIndex) return new OutTurnOperation[] {
                new OutTurnOperation { Type = OutTurnOperationType.Skip}
            };
            // other players' operations
            var operations = new List<OutTurnOperation> {
                new OutTurnOperation { Type = OutTurnOperationType.Skip}
            };
            // test rong
            TestRong(playerIndex, DiscardTile, operations);
            if (!CurrentRoundStatus.RichiStatus(playerIndex))
            {
                // get side
                var side = GetSide(playerIndex, CurrentPlayerIndex, CurrentRoundStatus.TotalPlayers);
                var handTiles = CurrentRoundStatus.HandTiles(playerIndex);
                // test kong
                TestKongs(handTiles, DiscardTile, side, operations);
                // test pong
                TestPongs(handTiles, DiscardTile, side, operations);
                // test chow
                TestChows(handTiles, DiscardTile, side, operations);
            }
            return operations.ToArray();
        }

        private void TestRong(int playerIndex, Tile discardTile, IList<OutTurnOperation> operations)
        {
            var baseHandStatus = HandStatus.Nothing;
            // test haidi
            if (MahjongSet.Data.TilesDrawn == gameSettings.MountainReservedTiles)
                baseHandStatus |= HandStatus.Haidi;
            // test lingshang -- not gonna happen
            var allTiles = MahjongSet.AllTiles;
            var doraTiles = MahjongSet.DoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var uraDoraTiles = MahjongSet.UraDoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, discardTile, baseHandStatus,
                doraTiles, uraDoraTiles, yakuSettings);
            // test if enough
            if (gameSettings.CheckConstraint(point))
            {
                operations.Add(new OutTurnOperation
                {
                    Type = OutTurnOperationType.Rong,
                    Tile = discardTile,
                    HandData = CurrentRoundStatus.HandData(playerIndex)
                });
            }
        }

        private MeldSide GetSide(int playerIndex, int discardPlayerIndex, int totalPlayer)
        {
            int diff = discardPlayerIndex - playerIndex;
            if (diff < 0) diff += totalPlayer;
            switch (totalPlayer)
            {
                case 2: return GetSide2();
                case 3: return GetSide3(diff);
                case 4: return GetSide4(diff);
                default:
                    Debug.LogError($"TotalPlayer = {totalPlayer}, this should not happen");
                    return MeldSide.Left;

            }
        }

        private MeldSide GetSide2()
        {
            return MeldSide.Left;
        }

        private MeldSide GetSide3(int diff)
        {
            switch (diff)
            {
                case 1: return MeldSide.Right;
                case 2: return MeldSide.Left;
                default:
                    Debug.LogError($"Diff = {diff}, this should not happen");
                    return MeldSide.Left;
            }
        }

        private MeldSide GetSide4(int diff)
        {
            switch (diff)
            {
                case 1: return MeldSide.Right;
                case 2: return MeldSide.Opposite;
                case 3: return MeldSide.Left;
                default:
                    Debug.LogError($"Diff = {diff}, this should not happen");
                    return MeldSide.Left;
            }
        }

        private void TestKongs(IList<Tile> handTiles, Tile discardTile, MeldSide side, IList<OutTurnOperation> operations)
        {
            if (!gameSettings.AllowPongs) return;
            var kongs = MahjongLogic.GetKongs(handTiles, discardTile, side);
            if (kongs.Any())
            {
                foreach (var kong in kongs)
                {
                    operations.Add(new OutTurnOperation
                    {
                        Type = OutTurnOperationType.Kong,
                        Tile = discardTile,
                        Meld = kong
                    });
                }
            }
        }

        private void TestPongs(IList<Tile> handTiles, Tile discardTile, MeldSide side, IList<OutTurnOperation> operations)
        {
            if (!gameSettings.AllowPongs) return;
            var pongs = MahjongLogic.GetPongs(handTiles, discardTile, side);
            if (pongs.Any())
            {
                foreach (var pong in pongs)
                {
                    operations.Add(new OutTurnOperation
                    {
                        Type = OutTurnOperationType.Pong,
                        Tile = discardTile,
                        Meld = pong
                    });
                }
            }
        }

        private void TestChows(IList<Tile> handTiles, Tile discardTile, MeldSide side, IList<OutTurnOperation> operations)
        {
            if (!gameSettings.AllowChows) return;
            if (side != MeldSide.Left) return;
            var chows = MahjongLogic.GetChows(handTiles, discardTile, side);
            if (chows.Any())
            {
                foreach (var chow in chows)
                {
                    operations.Add(new OutTurnOperation
                    {
                        Type = OutTurnOperationType.Chow,
                        Tile = discardTile,
                        Meld = chow
                    });
                }
            }
        }

        public override void OnStateUpdate()
        {
            // Send messages again until get enough responds or time out
            if (Time.time - firstSendTime > serverTimeOut)
            {
                // Time out, entering next state
                for (int i = 0; i < responds.Length; i++)
                {
                    if (responds[i]) continue;
                    players[i].BonusTurnTime = 0;
                    operations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
                TurnEnd();
                return;
            }
            if (responds.All(r => r))
            {
                Debug.Log("[Server] Server received all operation response, ending this turn.");
                TurnEnd();
            }
        }

        private void TurnEnd()
        {
            ServerBehaviour.Instance.TurnEnd(CurrentPlayerIndex, DiscardTile, IsRichiing, operations,
                false, TurnDoraAfterDiscard);
        }

        private void OnOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientOutTurnOperationMessage>();
            Debug.Log($"[Server] Received ClientOutTurnOperationMessage: {content}");
            responds[content.PlayerIndex] = true;
            operations[content.PlayerIndex] = content.Operation;
            players[content.PlayerIndex].BonusTurnTime = content.BonusTurnTime;
        }

        public override void OnServerStateExit()
        {
            NetworkServer.UnregisterHandler(MessageIds.ClientOutTurnOperationMessage);
        }
    }
}