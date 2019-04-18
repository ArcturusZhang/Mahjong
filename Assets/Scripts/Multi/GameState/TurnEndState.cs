using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using Debug = Single.Debug;

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
        private OutTurnOperation operationChosen;
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
            // check if game ends
            if (MahjongSet.TilesRemain == GameSettings.MountainReservedTiles) // no more tiles to draw
            {
                operationChosen = new OutTurnOperation { Type = OutTurnOperationType.RoundEnd };
            }
            else if (Operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                // all player choose to skip
                operationChosen = Operations[CurrentPlayerIndex];
            }
            // todo -- other circumstances
            // Send messages to clients
            for (int i = 0; i < Players.Count; i++)
            {
                messages[i] = new ServerTurnEndMessage
                {
                    PlayerIndex = i,
                    OperationPlayerIndex = CurrentPlayerIndex,
                    Operation = operationChosen
                };
                Players[i].connectionToClient.Send(MessageIds.ServerTurnEndMessage, messages[i]);
            }
        }

        public void OnStateUpdate()
        {
            Debug.Log($"Server is in {GetType().Name}", false);
            if (Time.time - firstTime > serverTurnEndTimeOut)
            {
                TurnEndTimeOut();
            }
        }

        private void TurnEndTimeOut()
        {
            // determines which state the server should transfer to by operationChosen.
            if (operationChosen.Type == OutTurnOperationType.RoundEnd)
            {
                Debug.Log($"[Server] Round draw.");
                ServerBehaviour.Instance.RoundDraw();
                return;
            }
            else if (operationChosen.Type == OutTurnOperationType.Skip)
            {
                var nextPlayer = CurrentPlayerIndex + 1;
                if (nextPlayer >= Players.Count) nextPlayer -= Players.Count;
                Debug.Log($"[Server] Next turn player index: {nextPlayer}");
                ServerBehaviour.Instance.DrawTile(nextPlayer);
                return;
            }
            // todo -- other operations
            Debug.Log($"Operation {operationChosen.Type} not supported yet, server idles.");
            ServerBehaviour.Instance.Idle();
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }
    }
}