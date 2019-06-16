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
    public class PlayerBeiDoraState : ServerState, IOnEventCallback
    {
        public int CurrentPlayerIndex;
        public MahjongSet MahjongSet;
        private bool[] responds;
        private OutTurnOperation[] outTurnOperations;
        private float firstTime;
        private float serverTimeOut;
        public override void OnServerStateEnter()
        {
            PhotonNetwork.AddCallbackTarget(this);
            // update hand tiles and bei doras
            UpdateRoundStatus();
            // send messages
            for (int i = 0; i < players.Count; i++)
            {
                var info = GetInfo(i);
                var player = CurrentRoundStatus.GetPlayer(i);
                ClientBehaviour.Instance.photonView.RPC("RpcBeiDora", player, info);
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
            CurrentRoundStatus.RemoveTile(CurrentPlayerIndex, new Tile(Suit.Z, 4));
            CurrentRoundStatus.AddBeiDoras(CurrentPlayerIndex);
            CurrentRoundStatus.SortHandTiles();
        }

        private EventMessages.BeiDoraInfo GetInfo(int index)
        {
            if (index == CurrentPlayerIndex)
            {
                return new EventMessages.BeiDoraInfo
                {
                    PlayerIndex = CurrentPlayerIndex,
                    BeiDoraPlayerIndex = CurrentPlayerIndex,
                    BeiDoras = CurrentRoundStatus.GetBeiDoras(),
                    HandData = CurrentRoundStatus.HandData(CurrentPlayerIndex),
                    BonusTurnTime = CurrentRoundStatus.GetBonusTurnTime(CurrentPlayerIndex),
                    Operations = GetBeiDoraOperations(CurrentPlayerIndex)
                };
            }
            else
            {
                return new EventMessages.BeiDoraInfo
                {
                    PlayerIndex = index,
                    BeiDoraPlayerIndex = CurrentPlayerIndex,
                    BeiDoras = CurrentRoundStatus.GetBeiDoras(),
                    HandData = new PlayerHandData
                    {
                        HandTiles = new Tile[CurrentRoundStatus.HandTiles(CurrentPlayerIndex).Length],
                        OpenMelds = CurrentRoundStatus.OpenMelds(CurrentPlayerIndex)
                    },
                    BonusTurnTime = CurrentRoundStatus.GetBonusTurnTime(index),
                    Operations = GetBeiDoraOperations(index),
                    MahjongSetData = MahjongSet.Data
                };
            }
        }

        private OutTurnOperation[] GetBeiDoraOperations(int playerIndex)
        {
            var operations = new List<OutTurnOperation>();
            operations.Add(new OutTurnOperation
            {
                Type = OutTurnOperationType.Skip
            });
            if (playerIndex == CurrentPlayerIndex) return operations.ToArray();
            // rong test
            TestBeiDoraRong(playerIndex, operations);
            return operations.ToArray();
        }

        private void TestBeiDoraRong(int playerIndex, IList<OutTurnOperation> operations)
        {
            var tile = new Tile(Suit.Z, 4);
            var point = GetRongInfo(playerIndex, tile);
            if (!gameSettings.CheckConstraint(point)) return;
            operations.Add(new OutTurnOperation
            {
                Type = OutTurnOperationType.Rong,
                Tile = tile,
                HandData = CurrentRoundStatus.HandData(playerIndex)
            });
        }

        private PointInfo GetRongInfo(int playerIndex, Tile tile)
        {
            var baseHandStatus = HandStatus.Nothing;
            if (gameSettings.AllowBeiDoraRongAsRobbKong) baseHandStatus |= HandStatus.RobKong;
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
                // no one claimed rong
                var turnDoraAfterDiscard = false;
                var isLingShang = gameSettings.AllowBeiDoraTsumoAsLingShang;
                CurrentRoundStatus.BreakOneShotsAndFirstTurn();
                ServerBehaviour.Instance.DrawTile(CurrentPlayerIndex, isLingShang, turnDoraAfterDiscard);
                return;
            }
            if (outTurnOperations.Any(op => op.Type == OutTurnOperationType.Rong))
            {
                var tile = new Tile(Suit.Z, 4);
                var turnDoraAfterDiscard = false;
                var isRobKong = gameSettings.AllowBeiDoraRongAsRobbKong;
                ServerBehaviour.Instance.TurnEnd(CurrentPlayerIndex, tile, false, outTurnOperations, isRobKong, turnDoraAfterDiscard);
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