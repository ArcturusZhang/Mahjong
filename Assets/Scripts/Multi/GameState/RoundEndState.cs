using System.Linq;
using Multi.Messages;
using UI.PointSummaryPanel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Multi.GameState
{
    // todo -- more detail round end, need more summary info
    // todo -- 1. A player has won.
    // todo -- 2. No players won -- need to test whether players' hand is ready
    // todo -- 3. Points summary
    public class RoundEndState : AbstractMahjongState
    {
        public MahjongManager MahjongManager;
        public int WinPlayerIndex;
        public GameStatus GameStatus;
        public UnityAction<bool> ServerCallback;
        private bool[] responseReceived;
        
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            Debug.Log($"Round ends!");
            NetworkServer.RegisterHandler(MessageConstants.ReadinessMessageId, OnReadinessMessageReceived);
            responseReceived = new bool[GameStatus.Players.Count];
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ReadinessMessage>();
            responseReceived[content.PlayerIndex] = true;
            if (!responseReceived.All(received => received)) return;
            // all clients are ready
            var newRound = WinPlayerIndex == GameStatus.RoundStatus.RoundCount - 1;
            ServerCallback.Invoke(newRound);
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            NetworkServer.UnregisterHandler(MessageConstants.ReadinessMessageId);
        }
    }
}