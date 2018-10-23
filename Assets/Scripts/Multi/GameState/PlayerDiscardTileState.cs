using System.Linq;
using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class PlayerDiscardTileState : AbstractMahjongState
    {
        public Tile DiscardTile;
        public bool DiscardLastDraw;
        public InTurnOperation InTurnOperation;
        public GameStatus GameStatus;
        public OutTurnOperationMessage[] OutTurnOperationMessages;
        public UnityAction<OutTurnOperationMessage[]> ServerCallback;
        private int currentPlayerIndex;
        private Player currentTurnPlayer;
        private bool[] responseReceived;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            NetworkServer.RegisterHandler(MessageConstants.OutTurnOperationMessageId, OnOutTurnOperationMessageReceived);
            currentPlayerIndex = GameStatus.CurrentPlayerIndex;
            currentTurnPlayer = GameStatus.CurrentTurnPlayer;
            // player visual effect on clients
            currentTurnPlayer.RpcDiscardTile(DiscardTile, DiscardLastDraw, InTurnOperation);
            // handle richi
            if (currentTurnPlayer.OneShot) currentTurnPlayer.OneShot = false;
            if (InTurnOperation.HasFlag(InTurnOperation.Richi))
            {
                currentTurnPlayer.Richi = true;
                currentTurnPlayer.OneShot = true;
                if (currentTurnPlayer.FirstTurn) currentTurnPlayer.WRichi = true;
            }

            foreach (var player in GameStatus.Players)
            {
                if (player == currentTurnPlayer) continue;
                player.RpcOutTurnOperation(DiscardTile, currentPlayerIndex);
            }
            
            Debug.Log("[Server] Now waiting for turn time to expire or receive out turn operation message.");
            responseReceived = new bool[GameStatus.Players.Count];
            responseReceived[currentPlayerIndex] = true; // current player do not need to send this message
        }

        private void OnOutTurnOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<OutTurnOperationMessage>();
            if (responseReceived[content.PlayerIndex]) return; // already get message from this player
            responseReceived[content.PlayerIndex] = true;
            OutTurnOperationMessages[content.PlayerIndex] = content;
            GameStatus.Players[content.PlayerIndex].BonusTurnTime = content.BonusTurnTime;
            if (!responseReceived.All(received => received)) return;
            Debug.Log("[Server] All players have sent out turn operation message, dealing with out turn operation");
            ServerCallback.Invoke(OutTurnOperationMessages);
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            NetworkServer.UnregisterHandler(MessageConstants.OutTurnOperationMessageId);
        }
    }
}