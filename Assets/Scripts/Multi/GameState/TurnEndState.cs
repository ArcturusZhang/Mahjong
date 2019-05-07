using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Multi.GameState
{
    /// <summary>
    /// This turn is used to end a player's turn, complete richi declaration, etc.
    /// Transfers to PlayerDiscardTileState (when a opening is claimed), PlayerDrawTileState (when kong is claimed or nothing claimed),
    /// RoundEndState (when a rong is claimed or when there are no more tiles to draw)
    /// </summary>
    public class TurnEndState : IState
    {
        public int CurrentPlayerIndex;
        public ServerRoundStatus CurrentRoundStatus;
        public Tile DiscardingTile;
        public bool IsRichiing;
        public OutTurnOperation[] Operations;
        public MahjongSet MahjongSet;
        public bool TurnDoraAfterDiscard;
        private GameSettings gameSettings;
        private YakuSettings yakuSettings;
        private IList<Player> players;
        private MessageBase[] messages;
        private OutTurnOperationType operationChosen;
        private float serverTurnEndTimeOut;
        private float firstTime;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            yakuSettings = CurrentRoundStatus.YakuSettings;
            players = CurrentRoundStatus.Players;
            if (CurrentRoundStatus.CurrentPlayerIndex != CurrentPlayerIndex)
            {
                Debug.LogError("CurrentPlayerIndex does not match, this should not happen");
                CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            }
            messages = new ServerTurnEndMessage[players.Count];
            firstTime = Time.time;
            // determines the operation to take when turn ends
            ChooseOperations();
            Debug.Log($"The operation chosen by this round is {operationChosen}");
            // if operation is not rong or round-draw, perform richi
            if (operationChosen != OutTurnOperationType.Rong && operationChosen != OutTurnOperationType.RoundDraw)
                CurrentRoundStatus.TryRichi(CurrentPlayerIndex, IsRichiing);
            // Send messages to clients
            for (int i = 0; i < players.Count; i++)
            {
                messages[i] = new ServerTurnEndMessage
                {
                    PlayerIndex = i,
                    ChosenOperationType = operationChosen,
                    Operations = Operations,
                    Points = CurrentRoundStatus.Points.ToArray(),
                    RichiStatus = CurrentRoundStatus.RichiStatusArray,
                    RichiSticks = CurrentRoundStatus.RichiSticks,
                    MahjongSetData = MahjongSet.Data
                };
                players[i].connectionToClient.Send(MessageIds.ServerTurnEndMessage, messages[i]);
            }
            serverTurnEndTimeOut = operationChosen == OutTurnOperationType.Skip ?
                ServerConstants.ServerTurnEndTimeOut : ServerConstants.ServerTurnEndTimeOutExtra;
            if (operationChosen == OutTurnOperationType.Chow
                || operationChosen == OutTurnOperationType.Pong
                || operationChosen == OutTurnOperationType.Kong)
                CurrentRoundStatus.BreakOneShotsAndFirstTurn();
        }

        private void ChooseOperations()
        {
            Debug.Log($"Operation before choosing: {string.Join(",", Operations)}");
            // test every circumstances by priority
            // test for rong
            if (Operations.Any(op => op.Type == OutTurnOperationType.Rong))
            {
                for (int i = 0; i < Operations.Length; i++)
                {
                    if (Operations[i].Type != OutTurnOperationType.Rong)
                        Operations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
                operationChosen = OutTurnOperationType.Rong;
                return;
            }
            // check if round draws
            if (MahjongSet.TilesRemain == gameSettings.MountainReservedTiles)
            {
                // no more tiles to draw and no one choose a rong operation.
                Debug.Log("No more tiles to draw and nobody claims a rong, the round has drawn.");
                for (int i = 0; i < Operations.Length; i++)
                    Operations[i] = new OutTurnOperation { Type = OutTurnOperationType.RoundDraw };
                operationChosen = OutTurnOperationType.RoundDraw;
                return;
            }
            // check if some one claimed kong
            if (Operations.Any(op => op.Type == OutTurnOperationType.Kong))
            {
                for (int i = 0; i < Operations.Length; i++)
                {
                    if (Operations[i].Type != OutTurnOperationType.Kong)
                        Operations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
                operationChosen = OutTurnOperationType.Kong;
                return;
            }
            // check if some one claimed pong
            if (Operations.Any(op => op.Type == OutTurnOperationType.Pong))
            {
                for (int i = 0; i < Operations.Length; i++)
                {
                    if (Operations[i].Type != OutTurnOperationType.Pong)
                        Operations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
                operationChosen = OutTurnOperationType.Pong;
                return;
            }
            // check if some one claimed chow
            if (Operations.Any(op => op.Type == OutTurnOperationType.Chow))
            {
                for (int i = 0; i < Operations.Length; i++)
                {
                    if (Operations[i].Type != OutTurnOperationType.Chow)
                        Operations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
                operationChosen = OutTurnOperationType.Chow;
                return;
            }
            // no particular operations -- skip
            operationChosen = OutTurnOperationType.Skip;
            // todo -- check if 3 rong
            Debug.Log($"Operation after choosing: {string.Join(",", Operations)}");
        }

        public void OnStateUpdate()
        {
            if (Time.time - firstTime > serverTurnEndTimeOut)
            {
                TurnEndTimeOut();
            }
        }

        // todo -- choose which state to transfer
        private void TurnEndTimeOut()
        {
            // determines which state the server should transfer to by operationChosen.
            switch (operationChosen)
            {
                case OutTurnOperationType.Rong:
                    HandleRong();
                    break;
                case OutTurnOperationType.RoundDraw:
                    ServerBehaviour.Instance.RoundDraw(RoundDrawType.RoundDraw);
                    break;
                case OutTurnOperationType.Kong:
                case OutTurnOperationType.Pong:
                case OutTurnOperationType.Chow:
                    int index = System.Array.FindIndex(Operations, op => op.Type != OutTurnOperationType.Skip);
                    Assert.IsTrue(index >= 0, "There should be a valid operation to find");
                    ServerBehaviour.Instance.PerformOutTurnOperation(index, Operations[index]);
                    break;
                case OutTurnOperationType.Skip:
                    int nextPlayer = CurrentPlayerIndex + 1;
                    if (nextPlayer >= players.Count) nextPlayer -= players.Count;
                    Debug.Log($"[Server] Next turn player index: {nextPlayer}");
                    ServerBehaviour.Instance.DrawTile(nextPlayer);
                    break;
                default:
                    Debug.LogError($"Unknown type of out turn operation: {operationChosen}");
                    break;
            }
        }

        private void HandleRong()
        {
            var rongPlayerIndexList = new List<int>();
            for (int i = 0; i < Operations.Length; i++)
            {
                if (Operations[i].Type == OutTurnOperationType.Rong)
                    rongPlayerIndexList.Add(i);
            }
            // sort this array
            rongPlayerIndexList.Sort(new RongPlayerIndexComparer(CurrentPlayerIndex, players.Count));
            var rongPlayerIndices = rongPlayerIndexList.ToArray();
            var rongPointInfos = new PointInfo[rongPlayerIndices.Length];
            for (int i = 0; i < rongPlayerIndices.Length; i++)
            {
                int playerIndex = rongPlayerIndices[i];
                rongPointInfos[i] = GetRongInfo(playerIndex, DiscardingTile);
            }
            Debug.Log($"[Server] Players who claimed rong: {string.Join(", ", rongPlayerIndices)}, "
                + $"corresponding pointInfos: {string.Join(";", rongPointInfos)}");
            ServerBehaviour.Instance.Rong(CurrentPlayerIndex, DiscardingTile, rongPlayerIndices, rongPointInfos);
        }

        private PointInfo GetRongInfo(int playerIndex, Tile discard)
        {
            var baseHandStatus = HandStatus.Nothing;
            // test haidi
            if (MahjongSet.TilesRemain == gameSettings.MountainReservedTiles)
                baseHandStatus |= HandStatus.Haidi;
            // test lingshang -- not gonna happen
            var allTiles = MahjongSet.AllTiles;
            var doraTiles = MahjongSet.DoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var uraDoraTiles = MahjongSet.UraDoraIndicators.Select(
                indicator => MahjongLogic.GetDoraTile(indicator, allTiles)).ToArray();
            var point = ServerMahjongLogic.GetPointInfo(
                playerIndex, CurrentRoundStatus, discard, baseHandStatus,
                doraTiles, uraDoraTiles, yakuSettings);
            Debug.Log($"TurnEndState: pointInfo: {point}");
            return point;
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
            if (operationChosen != OutTurnOperationType.Rong && TurnDoraAfterDiscard)
                MahjongSet.TurnDora();
        }

        private struct RongPlayerIndexComparer : IComparer<int>
        {
            private int current;
            private int total;

            public RongPlayerIndexComparer(int current, int total)
            {
                this.current = current;
                this.total = total;
            }
            public int Compare(int x, int y)
            {
                int dx = x - current;
                int dy = y - current;
                if (dx < 0) dx += total;
                if (dy < 0) dy += total;
                return dx - dy;
            }
        }
    }
}