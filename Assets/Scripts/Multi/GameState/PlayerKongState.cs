using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerKongState : ServerState
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
            NetworkServer.RegisterHandler(MessageIds.ClientOutTurnOperationMessage, OnOutTurnMessageReceived);
            // update hand tiles and open melds
            UpdateRoundStatus();
            // send messages
            for (int i = 0; i < players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                var message = new ServerKongMessage
                {
                    PlayerIndex = i,
                    KongPlayerIndex = CurrentPlayerIndex,
                    HandData = new PlayerHandData
                    {
                        HandTiles = new Tile[CurrentRoundStatus.HandTiles(CurrentPlayerIndex).Length],
                        OpenMelds = CurrentRoundStatus.OpenMelds(CurrentPlayerIndex)
                    },
                    BonusTurnTime = players[i].BonusTurnTime,
                    Operations = GetKongOperations(i),
                    MahjongSetData = MahjongSet.Data
                };
                players[i].connectionToClient.Send(MessageIds.ServerKongMessage, message);
            }
            players[CurrentPlayerIndex].connectionToClient.Send(MessageIds.ServerKongMessage, new ServerKongMessage
            {
                PlayerIndex = CurrentPlayerIndex,
                KongPlayerIndex = CurrentPlayerIndex,
                HandData = CurrentRoundStatus.HandData(CurrentPlayerIndex),
                BonusTurnTime = players[CurrentPlayerIndex].BonusTurnTime,
                Operations = GetKongOperations(CurrentPlayerIndex)
            });
            responds = new bool[players.Count];
            outTurnOperations = new OutTurnOperation[players.Count];
            firstTime = Time.time;
            serverTimeOut = players.Max(p => p.BonusTurnTime) + gameSettings.BaseTurnTime + ServerConstants.ServerTimeBuffer;
        }

        private void UpdateRoundStatus()
        {
            if (Kong.Revealed) // add kong
            {
                CurrentRoundStatus.AddKong(CurrentPlayerIndex, Kong);
                CurrentRoundStatus.RemoveTile(CurrentPlayerIndex, Kong);
            }
            else // self kong
            {
                CurrentRoundStatus.AddMeld(CurrentPlayerIndex, Kong);
                CurrentRoundStatus.RemoveTile(CurrentPlayerIndex, Kong);
            }
            // turn dora if this is a self kong
            if (Kong.Side == MeldSide.Self)
                MahjongSet.TurnDora();
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
                if (yakuSettings.AllowGswsRobConcealedKong &&
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
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, tile, baseHandStatus, null, null, yakuSettings);
            return point;
        }

        private void OnOutTurnMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientOutTurnOperationMessage>();
            Debug.Log($"[Server] received ClientOutTurnOperationMessage: {content}");
            responds[content.PlayerIndex] = true;
            outTurnOperations[content.PlayerIndex] = content.Operation;
            players[content.PlayerIndex].BonusTurnTime = content.BonusTurnTime;
        }

        public override void OnServerStateExit()
        {
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
                    players[i].BonusTurnTime = 0;
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
    }
}