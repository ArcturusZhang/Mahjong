using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;


namespace Multi.GameState
{
    /// <summary>
    /// This turn is used to end a player's turn, complete richi declaration, etc.
    /// Transfers to PlayerDiscardTileState (when a opening is claimed), PlayerDrawTileState (when kong is claimed or nothing claimed),
    /// RoundEndState (when a rong is claimed or when there are no more tiles to draw)
    /// </summary>
    public class TurnEndState : IState
    {
        public GameSettings GameSettings;
        public int CurrentPlayerIndex;
        public List<Player> Players;
        public bool IsRichiing;
        public OutTurnOperation[] Operations;
        public ServerRoundStatus CurrentRoundStatus;
        public MahjongSet MahjongSet;
        private ServerTurnEndMessage[] messages;
        private OutTurnOperationType operationChosen;
        private float serverTurnEndTimeOut = ServerConstants.ServerTurnEndTimeOut;
        private float firstTime;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            if (CurrentRoundStatus.CurrentPlayerIndex != CurrentPlayerIndex)
            {
                Debug.LogError("CurrentPlayerIndex does not match, this should not happen");
                CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            }
            messages = new ServerTurnEndMessage[Players.Count];
            firstTime = Time.time;
            // determines the operation to take when turn ends
            ChooseOperations();
            Debug.Log($"The operation chosen by this round is {operationChosen}");
            // Send messages to clients
            for (int i = 0; i < Players.Count; i++)
            {
                messages[i] = new ServerTurnEndMessage
                {
                    PlayerIndex = i,
                    ChosenOperationType = operationChosen,
                    Operations = Operations
                };
                Players[i].connectionToClient.Send(MessageIds.ServerTurnEndMessage, messages[i]);
            }
        }

        private void ChooseOperations()
        {
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
            if (MahjongSet.TilesRemain == GameSettings.MountainReservedTiles)
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
        }

        public void OnStateUpdate()
        {
            Debug.Log($"Server is in {GetType().Name}");
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
                    ServerBehaviour.Instance.RoundSummary(CurrentPlayerIndex, Operations);
                    break;
                case OutTurnOperationType.RoundDraw:
                    ServerBehaviour.Instance.RoundDraw();
                    break;
                case OutTurnOperationType.Kong:
                case OutTurnOperationType.Pong:
                case OutTurnOperationType.Chow:
                    int index = System.Array.FindIndex(Operations, op => op.Type != OutTurnOperationType.Skip);
                    Assert.IsTrue(index >= 0, "There should be a valid operation to find");
                    ServerBehaviour.Instance.PerformOperation(index, Operations[index]);
                    break;
                case OutTurnOperationType.Skip:
                    int nextPlayer = CurrentPlayerIndex + 1;
                    if (nextPlayer >= Players.Count) nextPlayer -= Players.Count;
                    Debug.Log($"[Server] Next turn player index: {nextPlayer}");
                    ServerBehaviour.Instance.DrawTile(nextPlayer);
                    break;
                default:
                    Debug.LogError($"Unknown type of out turn operation: {operationChosen}");
                    break;
            }
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }
    }
}