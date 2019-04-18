using System.Collections.Generic;
using Multi.MahjongMessages;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;
using Debug = Single.Debug;

namespace Multi.GameState
{
    /// <summary>
    /// When server is in this state, the server waits for ReadinessMessage from every player.
    /// When the server gets enough ReadinessMessages, the server transfers to GamePrepareState.
    /// Otherwise the server will resend the messages to not-responding clients until get enough responds or time out.
    /// When time out, the server transfers to GameAbortState.
    /// </summary>
    public class WaitForLoadingState : IState
    {
        public int TotalPlayers;
        public float TimeOut;
        private ISet<uint> responds;
        private float lastTime;

        public void OnStateEnter()
        {
            Debug.Log("Server enters WaitForLoadingState");
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            responds = new HashSet<uint>();
            lastTime = Time.time;
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            var netId = (uint)content.PlayerIndex;
            Debug.Log($"[Server] Received ClientReadinessMessage: {content}");
            if (!responds.Contains(netId)) responds.Add(netId);
        }

        public void OnStateUpdate()
        {
            if (responds.Count == TotalPlayers)
            {
                Debug.Log("All set, game start");
                ServerBehaviour.Instance.GamePrepare();
            }
            else if (Time.time - lastTime > TimeOut)
            {
                Debug.Log("Time out");
                ServerBehaviour.Instance.GameAbort();
            }
        }

        public void OnStateExit()
        {
            Debug.Log("Server exits WaitForLoadingState");
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
        }
    }
}