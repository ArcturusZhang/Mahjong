using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerDrawTileState : AbstractMahjongState
    {
        public MahjongSetManager MahjongSetManager;
        public GameStatus GameStatus;
        public UnityAction<Tile, bool, InTurnOperation> ServerCallback;
        public bool Lingshang;
        private int currentPlayerIndex;
        private Player currentTurnPlayer;

        public override void OnStateEntered()
        {
            base.OnStateEntered();
            NetworkServer.RegisterHandler(MessageConstants.DiscardTileMessageId, OnDiscardTileMessageReceived);
            currentPlayerIndex = GameStatus.CurrentPlayerIndex;
            currentTurnPlayer = GameStatus.CurrentTurnPlayer;
            DrawTile(Lingshang);
            Debug.Log("Server now waiting for turn time to expire or receive a DiscardTileMessage");
        }

        private void DrawTile(bool lingshang)
        {
            var nextIndex = lingshang ? MahjongSetManager.NextLingshangIndex : MahjongSetManager.NextIndex;
            var tile = lingshang ? MahjongSetManager.DrawLingshang() : MahjongSetManager.DrawTile();
            GameStatus.RoundStatus = GameStatus.RoundStatus.RemoveTiles(1);
            foreach (var player in GameStatus.Players)
            {
                player.RoundStatus = GameStatus.RoundStatus;
            }
            currentTurnPlayer.LastDraw = tile;
            currentTurnPlayer.RpcYourTurnToDraw(nextIndex);
            currentTurnPlayer.connectionToClient.Send(MessageConstants.DrawTileMessageId,
                new DrawTileMessage {PlayerIndex = GameStatus.CurrentPlayerIndex, Tile = tile, Lingshang = false});
        }

        // todo -- in turn operation message handler 

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