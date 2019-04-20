using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;
using Utils;


namespace Multi.GameState
{
    /// <summary>
    /// When the server is in this state, the server make preparation that the game needs.
    /// The playerIndex is arranged in this state, so is the settings. Messages will be sent to clients to inform the information.
    /// Transfers to RoundStartState. The state transfer will be done regardless whether enough client responds received.
    /// </summary>
    public class GamePrepareState : IState
    {
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        public ServerRoundStatus CurrentRoundStatus;
        public List<Player> Players;
        private ServerGamePrepareMessage[] messages;
        private bool[] responds;
        private float lastSendTime;
        public void OnStateEnter()
        {
            Debug.Log("Server enters GamePrepareState");
            Debug.Log($"This game has total {Players.Count} players");
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            Players.Shuffle();
            messages = new ServerGamePrepareMessage[Players.Count];
            responds = new bool[Players.Count];
            AssignInitialPoints();
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].PlayerIndex = i;
                Players[i].BonusTurnTime = GameSettings.BonusTurnTime;
                messages[i] = new ServerGamePrepareMessage
                {
                    TotalPlayers = Players.Count,
                    PlayerIndex = i,
                    Points = CurrentRoundStatus.Points.ToArray(),
                    PlayerNames = CurrentRoundStatus.PlayerNames,
                    GameSettings = GameSettings,
                    YakuSettings = YakuSettings
                };
                Players[i].connectionToClient.Send(MessageIds.ServerPrepareMessage, messages[i]);
            }
            lastSendTime = Time.time;
            // todo -- Maybe other initialization work needs to be done.
        }

        private void AssignInitialPoints() {
            for (int i = 0; i < Players.Count; i++) {
                CurrentRoundStatus.SetPoints(i, GameSettings.InitialPoints);
            }
        }

        public void OnStateUpdate()
        {
            if (responds.All(r => r))
            {
                ServerBehaviour.Instance.RoundStart();
                return;
            }
            // resend request messsage by some interval
            if (Time.time - lastSendTime >= ServerConstants.MessageResendInterval)
            {
                lastSendTime = Time.time;
                for (int i = 0; i < Players.Count; i++)
                {
                    if (responds[i]) continue;
                    Players[i].connectionToClient.Send(MessageIds.ServerPrepareMessage, messages[i]);
                }
            }
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            Debug.Log($"[Server] Received ClientReadinessMessage: {content}.");
            if (content.Content != content.PlayerIndex)
            {
                Debug.LogError("Something is wrong, the received readiness message contains invalid content.");
                return;
            }
            responds[content.PlayerIndex] = true;
        }

        public void OnStateExit()
        {
            Debug.Log("Server exits GamePrepareState");
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
        }
    }
}