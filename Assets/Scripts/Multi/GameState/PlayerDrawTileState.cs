using Multi.Messages;
using Multi.ServerData;
using Single;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerDrawTileState : AbstractMahjongState
    {
        public MahjongSetManager MahjongSetManager;
        public NetworkRoundStatus NetworkRoundStatus;
        public GameStatus GameStatus;
        public UnityAction<InTurnOperationData> ServerInTurnCallback;
        public UnityAction<DiscardTileData> ServerDiscardCallback;
        public bool Lingshang;
        private int currentPlayerIndex;
        private Player currentTurnPlayer;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            NetworkServer.RegisterHandler(MessageConstants.InTurnOperationMessageId, OnInTurnOperationMessageReceived);
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
            NetworkRoundStatus.RemoveTiles(1);
            currentTurnPlayer.LastDraw = tile;
            currentTurnPlayer.RpcYourTurnToDraw(nextIndex);
            currentTurnPlayer.connectionToClient.Send(MessageConstants.DrawTileMessageId,
                new DrawTileMessage {PlayerIndex = GameStatus.CurrentPlayerIndex, Tile = tile, Lingshang = false});
        }

        private void OnInTurnOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<InTurnOperationMessage>();
            if (content.PlayerIndex != currentPlayerIndex)
            {
                Debug.Log($"[PlayerInTurnState] Received discarding message from player {content.PlayerIndex}"
                          + $" in player {currentPlayerIndex}'s turn, ignoring");
                return;
            }

            // rpc data update
            currentTurnPlayer.BonusTurnTime = content.BonusTurnTime;
            // handle result in callback
            ServerInTurnCallback.Invoke(new InTurnOperationData
            {
                PlayerIndex = content.PlayerIndex,
                LastDraw = content.LastDraw,
                Meld = content.Meld,
                Operation = content.Operation,
                PlayerClientData = content.PlayerClientData
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

            Debug.Log($"[PlayerInTurnState] Player {content.PlayerIndex} has discarded a tile {content.DiscardTile}");
            // first turn is broken by the first discard
            if (currentTurnPlayer.FirstTurn) currentTurnPlayer.FirstTurn = false;
            // server side data update
            if (!content.DiscardLastDraw)
            {
                GameStatus.PlayerHandTiles[currentPlayerIndex].Remove(content.DiscardTile);
                GameStatus.PlayerHandTiles[currentPlayerIndex].Add(currentTurnPlayer.LastDraw);
                GameStatus.PlayerHandTiles[currentPlayerIndex].Sort();
            }

            // rpc data update
            currentTurnPlayer.BonusTurnTime = content.BonusTurnTime;
            ServerDiscardCallback.Invoke(new DiscardTileData
            {
                DiscardTile = content.DiscardTile,
                DiscardLastDraw = content.DiscardLastDraw,
                Operation = content.Operation
            });
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            NetworkServer.UnregisterHandler(MessageConstants.InTurnOperationMessageId);
            NetworkServer.UnregisterHandler(MessageConstants.DiscardTileMessageId);
        }
    }
}