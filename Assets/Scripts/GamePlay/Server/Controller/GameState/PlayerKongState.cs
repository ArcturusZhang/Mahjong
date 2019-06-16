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
    public class PlayerKongState : ServerState, IOnEventCallback
    {
        public int CurrentPlayerIndex;
        public MahjongSet MahjongSet;
        public OpenMeld Kong;
        private bool[] responds;
        private OutTurnOperation[] outTurnOperations;
        private float firstTime;
        private float serverTimeOut;

        public override void OnServerStateEnter()
        {
            PhotonNetwork.AddCallbackTarget(this);
            // update hand tiles and open melds
            UpdateRoundStatus();
            // send messages
            for (int i = 0; i < players.Count; i++)
            {
                var info = GetInfo(i);
                var player = CurrentRoundStatus.GetPlayer(i);
                ClientBehaviour.Instance.photonView.RPC("RpcKong", player, info);
            }
            responds = new bool[players.Count];
            outTurnOperations = new OutTurnOperation[players.Count];
            firstTime = Time.time;
            serverTimeOut = CurrentRoundStatus.MaxBonusTurnTime + gameSettings.BaseTurnTime + ServerConstants.ServerTimeBuffer;
        }

        private void UpdateRoundStatus()
        {
            var lastDraw = (Tile)CurrentRoundStatus.LastDraw;
            CurrentRoundStatus.LastDraw = null;
            CurrentRoundStatus.AddTile(CurrentPlayerIndex, lastDraw);
            if (Kong.IsAdded) // add kong
            {
                CurrentRoundStatus.AddKong(CurrentPlayerIndex, Kong);
                CurrentRoundStatus.RemoveTile(CurrentPlayerIndex, Kong.Extra);
            }
            else // self kong
            {
                CurrentRoundStatus.AddMeld(CurrentPlayerIndex, Kong);
                CurrentRoundStatus.RemoveTile(CurrentPlayerIndex, Kong);
            }
            CurrentRoundStatus.SortHandTiles();
            // turn dora if this is a self kong
            if (Kong.Side == MeldSide.Self)
                MahjongSet.TurnDora();
        }

        private EventMessages.KongInfo GetInfo(int index)
        {
            if (index == CurrentPlayerIndex)
            {
                return new EventMessages.KongInfo
                {
                    PlayerIndex = CurrentPlayerIndex,
                    KongPlayerIndex = CurrentPlayerIndex,
                    HandData = CurrentRoundStatus.HandData(CurrentPlayerIndex),
                    BonusTurnTime = CurrentRoundStatus.GetBonusTurnTime(CurrentPlayerIndex),
                    Operations = GetKongOperations(CurrentPlayerIndex),
                    MahjongSetData = MahjongSet.Data
                };
            }
            else
            {
                return new EventMessages.KongInfo
                {
                    PlayerIndex = index,
                    KongPlayerIndex = CurrentPlayerIndex,
                    HandData = new PlayerHandData
                    {
                        HandTiles = new Tile[CurrentRoundStatus.HandTiles(CurrentPlayerIndex).Length],
                        OpenMelds = CurrentRoundStatus.OpenMelds(CurrentPlayerIndex)
                    },
                    BonusTurnTime = CurrentRoundStatus.GetBonusTurnTime(CurrentPlayerIndex),
                    Operations = GetKongOperations(index),
                    MahjongSetData = MahjongSet.Data
                };
            }
        }

        private OutTurnOperation[] GetKongOperations(int playerIndex)
        {
            var operations = new List<OutTurnOperation>();
            operations.Add(new OutTurnOperation
            {
                Type = OutTurnOperationType.Skip
            });
            if (playerIndex == CurrentPlayerIndex) return operations.ToArray();
            // rob kong test
            TestRobKong(playerIndex, operations);
            return operations.ToArray();
        }

        private Tile GetTileFromKong()
        {
            if (Kong.Side == MeldSide.Self) return Kong.First;
            return Kong.Tile;
        }

        private void TestRobKong(int playerIndex, IList<OutTurnOperation> operations)
        {
            var tile = GetTileFromKong();
            var point = GetRongInfo(playerIndex, tile);
            if (!gameSettings.CheckConstraint(point)) return;
            if (Kong.Side == MeldSide.Self)
            {
                // handle self kong
                if (gameSettings.AllowGswsRobConcealedKong &&
                    point.YakuList.Any(yaku => yaku.Name.StartsWith("国士无双")))
                {
                    operations.Add(new OutTurnOperation
                    {
                        Type = OutTurnOperationType.Rong,
                        Tile = tile,
                        HandData = CurrentRoundStatus.HandData(playerIndex)
                    });
                }
            }
            else
            {
                // handle added kong
                operations.Add(new OutTurnOperation
                {
                    Type = OutTurnOperationType.Rong,
                    Tile = tile,
                    HandData = CurrentRoundStatus.HandData(playerIndex)
                });
            }
        }

        private PointInfo GetRongInfo(int playerIndex, Tile tile)
        {
            var baseHandStatus = HandStatus.RobKong;
            var allTiles = MahjongSet.AllTiles;
            var doraTiles = MahjongSet.DoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var uraDoraTiles = MahjongSet.UraDoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var beiDora = CurrentRoundStatus.GetBeiDora(playerIndex);
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, tile, baseHandStatus,
                doraTiles, uraDoraTiles, beiDora, gameSettings);
            return point;
        }

        public override void OnServerStateExit()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnStateUpdate()
        {
            // check operations: if all operations are skip, let the current player to draw his lingshang
            // if some one claimed rong, transfer to TurnEndState handling rong operations
            if (Time.time - firstTime > serverTimeOut)
            {
                for (int i = 0; i < responds.Length; i++)
                {
                    if (responds[i]) continue;
                    // players[i].BonusTurnTime = 0;
                    outTurnOperations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                    NextState();
                    return;
                }
            }
            if (responds.All(r => r))
            {
                Debug.Log("[Server] Server received all operation response, handling results.");
                NextState();
            }
        }

        private void NextState()
        {
            if (outTurnOperations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                // no one claimed a rob kong
                var turnDoraAfterDiscard = Kong.Side != MeldSide.Self;
                CurrentRoundStatus.BreakOneShotsAndFirstTurn();
                ServerBehaviour.Instance.DrawTile(CurrentPlayerIndex, true, turnDoraAfterDiscard);
                return;
            }
            if (outTurnOperations.Any(op => op.Type == OutTurnOperationType.Rong))
            {
                var discardingTile = GetTileFromKong();
                ServerBehaviour.Instance.TurnEnd(CurrentPlayerIndex, discardingTile, false, outTurnOperations, true, false);
                return;
            }
            Debug.LogError($"[Server] Logically cannot reach here, operations are {string.Join("|", outTurnOperations)}");
        }

        private void OnOutTurnOperationEvent(EventMessages.OutTurnOperationInfo info)
        {
            var index = info.PlayerIndex;
            if (responds[index]) return;
            responds[index] = true;
            outTurnOperations[index] = info.Operation;
            CurrentRoundStatus.SetBonusTurnTime(index, info.BonusTurnTime);
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