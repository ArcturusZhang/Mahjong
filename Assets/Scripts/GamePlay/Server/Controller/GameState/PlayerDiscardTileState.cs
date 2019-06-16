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
    public class PlayerDiscardTileState : ServerState, IOnEventCallback
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
            PhotonNetwork.AddCallbackTarget(this);
            if (CurrentRoundStatus.CurrentPlayerIndex != CurrentPlayerIndex)
            {
                Debug.LogError("[Server] currentPlayerIndex does not match, this should not happen");
                CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            }
            UpdateRoundStatus();
            Debug.Log($"[Server] CurrentRoundStatus: {CurrentRoundStatus}");
            responds = new bool[players.Count];
            operations = new OutTurnOperation[players.Count];
            var rivers = CurrentRoundStatus.Rivers;
            // Get messages and send them to players
            for (int i = 0; i < players.Count; i++)
            {
                var info = new EventMessages.DiscardOperationInfo
                {
                    PlayerIndex = i,
                    CurrentTurnPlayerIndex = CurrentPlayerIndex,
                    IsRichiing = IsRichiing,
                    DiscardingLastDraw = DiscardLastDraw,
                    Tile = DiscardTile,
                    BonusTurnTime = CurrentRoundStatus.GetBonusTurnTime(i),
                    Zhenting = CurrentRoundStatus.IsZhenting(i),
                    Operations = GetOperations(i),
                    HandTiles = CurrentRoundStatus.HandTiles(i),
                    Rivers = rivers
                };
                var player = CurrentRoundStatus.GetPlayer(i);
                ClientBehaviour.Instance.photonView.RPC("RpcDiscardOperation", player, info);
            }
            firstSendTime = Time.time;
            serverTimeOut = gameSettings.BaseTurnTime + CurrentRoundStatus.MaxBonusTurnTime
                + ServerConstants.ServerTimeBuffer;
        }

        private void UpdateRoundStatus()
        {
            CurrentRoundStatus.SetBonusTurnTime(CurrentPlayerIndex, BonusTurnTime);
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
            CurrentRoundStatus.UpdateDiscardZhenting();
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
            var beiDora = CurrentRoundStatus.GetBeiDora(playerIndex);
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, discardTile, baseHandStatus,
                doraTiles, uraDoraTiles, beiDora, gameSettings);
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
                        Meld = kong,
                        ForbiddenTiles = gameSettings.AllowDiscardSameAfterOpen ? null : kong.GetForbiddenTiles(discardTile)
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
                        Meld = pong,
                        ForbiddenTiles = gameSettings.AllowDiscardSameAfterOpen ? null : pong.GetForbiddenTiles(discardTile)
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
                        Meld = chow,
                        ForbiddenTiles = gameSettings.AllowDiscardSameAfterOpen ? null : chow.GetForbiddenTiles(discardTile)
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
                    CurrentRoundStatus.SetBonusTurnTime(i, 0);
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

        private void OnOutTurnOperationEvent(EventMessages.OutTurnOperationInfo info)
        {
            var index = info.PlayerIndex;
            if (responds[index]) return;
            responds[index] = true;
            operations[index] = info.Operation;
            CurrentRoundStatus.SetBonusTurnTime(index, info.BonusTurnTime);
        }

        public override void OnServerStateExit()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            var code = photonEvent.Code;
            var info = photonEvent.CustomData;
            Debug.Log($"{GetType().Name} receives event code: {code} with content {info}");
            switch (code)
            {
                case EventMessages.OutTurnOperationEvent:
                    OnOutTurnOperationEvent((EventMessages.OutTurnOperationInfo)info);
                    break;
            }
        }
    }
}