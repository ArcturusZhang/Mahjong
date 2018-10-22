using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerOpenMeldState : AbstractMahjongState
    {
        public Tile DefaultTile;
        public GameStatus GameStatus;
        public UnityAction<Tile, bool, InTurnOperation> ServerCallback;
        private int currentPlayerIndex;
        private Player currentTurnPlayer;
        
        public override void OnStateEntered()
        {
            base.OnStateEntered();
            NetworkServer.RegisterHandler(MessageConstants.DiscardTileMessageId, OnDiscardTileMessageReceived);
            currentPlayerIndex = GameStatus.CurrentPlayerIndex;
            currentTurnPlayer = GameStatus.CurrentTurnPlayer;
            currentTurnPlayer.LastDraw = DefaultTile;
            currentTurnPlayer.connectionToClient.Send(MessageConstants.DiscardAfterOpenMessageId,
                new DiscardAfterOpenMessage
                {
                    PlayerIndex = currentPlayerIndex,
                    DefaultTile = DefaultTile
                });
        }

        private void OnDiscardTileMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<DiscardTileMessage>();
            if (content.PlayerIndex != currentPlayerIndex)
            {
                Debug.Log($"[PlayerInTurnState] Received discarding message from player {content.PlayerIndex}"
                          + $" in player {currentPlayerIndex}'s turn, ignoring");
                return;
            }

            if (!content.DiscardLastDraw)
            {
                GameStatus.PlayerHandTiles[currentPlayerIndex].Remove(content.DiscardTile);
                GameStatus.PlayerHandTiles[currentPlayerIndex].Add(currentTurnPlayer.LastDraw);
                GameStatus.PlayerHandTiles[currentPlayerIndex].Sort();
            }

            currentTurnPlayer.BonusTurnTime = content.BonusTurnTime;
            ServerCallback.Invoke(content.DiscardTile, content.DiscardLastDraw, content.Operation);
        }

        public override void OnStateExited()
        {
            base.OnStateExited();
            NetworkServer.UnregisterHandler(MessageConstants.DiscardTileMessageId);
        }
    }
}