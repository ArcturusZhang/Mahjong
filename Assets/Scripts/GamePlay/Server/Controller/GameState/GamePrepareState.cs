using System.Linq;
using ExitGames.Client.Photon;
using GamePlay.Client.Controller;
using Photon.Pun;
using Photon.Realtime;
using GamePlay.Server.Model.Events;
using UnityEngine;

namespace GamePlay.Server.Controller.GameState
{
    /// <summary>
    /// When the server is in this state, the server make preparation that the game needs.
    /// The playerIndex is arranged in this state, so is the settings. Messages will be sent to clients to inform the information.
    /// Transfers to RoundStartState. The state transfer will be done regardless whether enough client responds received.
    /// </summary>
    public class GamePrepareState : ServerState, IOnEventCallback
    {
        private bool[] responds;
        private float firstTime;
        private float serverTimeOut = 5f;
        public override void OnServerStateEnter()
        {
            Debug.Log($"This game has total {players.Count} players");
            PhotonNetwork.AddCallbackTarget(this);
            responds = new bool[players.Count];
            firstTime = Time.time;
            CurrentRoundStatus.ShufflePlayers();
            AssignInitialPoints();
            ClientRpcCalls();
        }

        private void AssignInitialPoints()
        {
            for (int i = 0; i < players.Count; i++)
            {
                CurrentRoundStatus.SetPoints(i, CurrentRoundStatus.GameSettings.InitialPoints);
            }
        }

        private void ClientRpcCalls()
        {
            var room = PhotonNetwork.CurrentRoom;
            for (int i = 0; i < CurrentRoundStatus.TotalPlayers; i++)
            {
                var player = CurrentRoundStatus.GetPlayer(i);
                ClientBehaviour.Instance.photonView.RPC("RpcGamePrepare", player, new EventMessages.GamePrepareInfo
                {
                    PlayerIndex = i,
                    Points = CurrentRoundStatus.Points,
                    PlayerNames = CurrentRoundStatus.PlayerNames,
                    GameSetting = gameSettings.ToString()
                });
            }
        }

        public override void OnServerStateExit()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnStateUpdate()
        {
            if (responds.All(r => r))
            {
                ServerBehaviour.Instance.RoundStart(true, false, false);
                return;
            }
            if (Time.time - firstTime > serverTimeOut)
            {
                Debug.Log("[Server] Prepare state time out");
                ServerBehaviour.Instance.RoundStart(true, false, false);
                return;
            }
        }

        private void OnClientReadyEvent(int index)
        {
            responds[index] = true;
        }

        public void OnEvent(EventData photonEvent)
        {
            var code = photonEvent.Code;
            var info = photonEvent.CustomData;
            Debug.Log($"{GetType().Name} receives event code: {code} with content {info}");
            switch (code)
            {
                case EventMessages.ClientReadyEvent:
                    OnClientReadyEvent((int)photonEvent.CustomData);
                    break;
            }
        }
    }
}