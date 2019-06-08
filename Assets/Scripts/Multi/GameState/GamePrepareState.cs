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
    public class GamePrepareState : ServerState
    {
        private MessageBase[] messages;
        private bool[] responds;
        private float firstSendTime;
        private float lastSendTime;
        private float serverTimeOut = 5f;
        public override void OnServerStateEnter()
        {
            Debug.Log($"This game has total {players.Count} players");
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            CurrentRoundStatus.ShufflePlayers();
            messages = new ServerGamePrepareMessage[players.Count];
            responds = new bool[players.Count];
            AssignInitialPoints();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerIndex = i;
                players[i].BonusTurnTime = CurrentRoundStatus.GameSettings.BonusTurnTime;
                messages[i] = new ServerGamePrepareMessage
                {
                    TotalPlayers = players.Count,
                    PlayerIndex = i,
                    Points = CurrentRoundStatus.Points.ToArray(),
                    PlayerNames = CurrentRoundStatus.PlayerNames,
                    GameSetting = gameSettings.ToJson(false),
                    YakuSetting = yakuSettings.ToJson(false)
                };
                players[i].connectionToClient.Send(MessageIds.ServerGamePrepareMessage, messages[i]);
            }
            firstSendTime = Time.time;
            lastSendTime = Time.time;
        }

        private void AssignInitialPoints()
        {
            for (int i = 0; i < players.Count; i++)
            {
                CurrentRoundStatus.SetPoints(i, CurrentRoundStatus.GameSettings.InitialPoints);
            }
        }

        public override void OnStateUpdate()
        {
            if (responds.All(r => r))
            {
                ServerBehaviour.Instance.RoundStart(true, false, false);
                return;
            }
            if (Time.time - firstSendTime > serverTimeOut)
            {
                Debug.Log("[Server] Prepare state time out");
                ServerBehaviour.Instance.RoundStart(true, false, false);
                return;
            }
            // resend request messsage by some interval
            if (Time.time - lastSendTime >= ServerConstants.MessageResendInterval)
            {
                lastSendTime = Time.time;
                for (int i = 0; i < players.Count; i++)
                {
                    if (responds[i]) continue;
                    players[i].connectionToClient.Send(MessageIds.ServerGamePrepareMessage, messages[i]);
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

        public override void OnServerStateExit()
        {
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
        }
    }
}