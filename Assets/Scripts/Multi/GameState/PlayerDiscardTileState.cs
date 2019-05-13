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
        public int CurrentPlayerIndex;
        public Tile DiscardTile;
        public bool IsRichiing;
        public bool DiscardLastDraw;
        public ServerRoundStatus CurrentRoundStatus;
        public MahjongSet MahjongSet;
        public bool TurnDoraAfterDiscard;
        private GameSettings gameSettings;
        private YakuSettings yakuSettings;
        private IList<Player> players;
        private float firstSendTime;
        private float serverTimeOut;
        private bool[] operationResponds;
        private OutTurnOperation[] outTurnOperations;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            yakuSettings = CurrentRoundStatus.YakuSettings;
            players = CurrentRoundStatus.Players;
            NetworkServer.RegisterHandler(MessageIds.ClientOutTurnOperationMessage, OnOperationMessageReceived);
            if (CurrentRoundStatus.CurrentPlayerIndex != CurrentPlayerIndex)
            {
                Debug.LogError("[Server] currentPlayerIndex does not match, this should not happen");
                CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            }
            operationResponds = new bool[players.Count];
            outTurnOperations = new OutTurnOperation[players.Count];
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
            if (gameSettings.CheckConstraint(point))
            {
                operations.Add(new OutTurnOperation
                {
                    Type = OutTurnOperationType.Rong,
                    Tile = DiscardTile,
                    HandData = CurrentRoundStatus.HandData(playerIndex)
                });
            }
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

        private MeldSide GetSide(int playerIndex, int discardPlayerIndex, int totalPlayer)
        {
            int diff = discardPlayerIndex - playerIndex;
            if (diff < 0) diff += totalPlayer;
            switch (diff)
            {
                case 1:
                    return MeldSide.Right;
                case 2:
                    return MeldSide.Opposite;
                case 3:
                    return MeldSide.Left;
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

        private PointInfo GetRongInfo(int playerIndex, Tile discard)
        {
            var baseHandStatus = HandStatus.Nothing;
            // test haidi
            if (MahjongSet.Data.TilesRemain == gameSettings.MountainReservedTiles)
                baseHandStatus |= HandStatus.Haidi;
            // test lingshang -- not gonna happen
            // just test if this player can claim rong, no need for dora
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, discard, baseHandStatus,
                null, null, yakuSettings);
            return point;
        }

        public void OnStateUpdate()
        {
            // Send messages again until get enough responds or time out
            if (Time.time - firstSendTime > serverTimeOut)
            {
                // Time out, entering next state
                for (int i = 0; i < operationResponds.Length; i++)
                {
                    if (operationResponds[i]) continue;
                    players[i].BonusTurnTime = 0;
                    outTurnOperations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
                TurnEnd();
                return;
            }
            if (operationResponds.All(r => r))
            {
                Debug.Log("[Server] Server received all operation response, ending this turn.");
                TurnEnd();
            }
        }

        private void TurnEnd()
        {
            ServerBehaviour.Instance.TurnEnd(CurrentPlayerIndex, DiscardTile, IsRichiing, outTurnOperations, TurnDoraAfterDiscard);
        }

        private void OnOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientOutTurnOperationMessage>();
            Debug.Log($"[Server] Received ClientOutTurnOperationMessage: {content}");
            operationResponds[content.PlayerIndex] = true;
            outTurnOperations[content.PlayerIndex] = content.Operation;
            players[content.PlayerIndex].BonusTurnTime = content.BonusTurnTime;
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
            NetworkServer.UnregisterHandler(MessageIds.ClientOutTurnOperationMessage);
        }
    }
}