using Multi.Messages;
using Multi.ServerData;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerOpenMeldState : AbstractMahjongState
    {
        public OpenMeldData OpenMeldData;
        public GameStatus GameStatus;
        public UnityAction<DiscardTileData> ServerCallback;
        private int currentPlayerIndex;
        private Player currentTurnPlayer;
        
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            NetworkServer.RegisterHandler(MessageConstants.DiscardTileMessageId, OnDiscardTileMessageReceived);
            currentPlayerIndex = GameStatus.CurrentPlayerIndex;
            currentTurnPlayer = GameStatus.CurrentTurnPlayer;
            currentTurnPlayer.LastDraw = OpenMeldData.DefaultTile;
            currentTurnPlayer.connectionToClient.Send(MessageConstants.DiscardAfterOpenMessageId,
                new DiscardAfterOpenMessage
                {
                    PlayerIndex = currentPlayerIndex,
                    DefaultTile = OpenMeldData.DefaultTile,
                    ForbiddenTiles = OpenMeldData.ForbiddenTiles
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
            ServerCallback.Invoke(new DiscardTileData
            {
                DiscardTile = content.DiscardTile,
                DiscardLastDraw = content.DiscardLastDraw,
                Operation = content.Operation
            });
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            NetworkServer.UnregisterHandler(MessageConstants.DiscardTileMessageId);
        }
    }
}