using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerBeiDoraState : ServerState
    {
        public int CurrentPlayerIndex;
        public MahjongSet MahjongSet;
        private bool[] responds;
        private OutTurnOperation[] outTurnOperations;
        private float firstTime;
        private float serverTimeOut;
        public override void OnServerStateEnter()
        {
            NetworkServer.RegisterHandler(MessageIds.ClientOutTurnOperationMessage, OnOutTurnMessageReceived);
            // update hand tiles and bei doras
            UpdateRoundStatus();
            // send messages
            for (int i = 0; i < players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                var message = new ServerBeiDoraMessage
                {
                    PlayerIndex = i,
                    BeiDoraPlayerIndex = CurrentPlayerIndex,
                    BeiDoras = CurrentRoundStatus.GetBeiDoras(),
                    HandData = new PlayerHandData
                    {
                        HandTiles = new Tile[CurrentRoundStatus.HandTiles(CurrentPlayerIndex).Length],
                        OpenMelds = CurrentRoundStatus.OpenMelds(CurrentPlayerIndex)
                    },
                    BonusTurnTime = players[i].BonusTurnTime,
                    Operations = GetBeiDoraOperations(i),
                    MahjongSetData = MahjongSet.Data
                };
                players[i].connectionToClient.Send(MessageIds.ServerBeiDoraMessage, message);
            }
            players[CurrentPlayerIndex].connectionToClient.Send(MessageIds.ServerBeiDoraMessage, new ServerBeiDoraMessage
            {
                PlayerIndex = CurrentPlayerIndex,
                BeiDoraPlayerIndex = CurrentPlayerIndex,
                BeiDoras = CurrentRoundStatus.GetBeiDoras(),
                HandData = CurrentRoundStatus.HandData(CurrentPlayerIndex),
                BonusTurnTime = players[CurrentPlayerIndex].BonusTurnTime,
                Operations = GetBeiDoraOperations(CurrentPlayerIndex)
            });
            responds = new bool[players.Count];
            outTurnOperations = new OutTurnOperation[players.Count];
            firstTime = Time.time;
            serverTimeOut = players.Max(p => p.BonusTurnTime) + gameSettings.BaseTurnTime + ServerConstants.ServerTimeBuffer;
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
    }
}