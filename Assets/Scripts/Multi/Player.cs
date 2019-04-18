using Lobby;
using Multi.MahjongMessages;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Networking;
using Debug = Single.Debug;

namespace Multi
{
    public class Player : NetworkBehaviour
    {
        private const float ReadinessMessageSendInterval = 0.5f;
        [Header("Game Status")]
        [SyncVar]
        public int PlayerIndex = -1; // Round order index -- 0: East, 1: South, 2: West, 3: North (This does not change)

        [SyncVar] public int BonusTurnTime;

        [Header("Player Public Data")]
        [SyncVar]
        public string PlayerName = "";
        private bool gameStarted = false;
        private float lastSendTime;

        public override void OnStartClient()
        {
            Debug.Log($"Player [netId: {netId}] [name: {PlayerName}] OnStartClient is called");
            LobbyManager.Instance.AddPlayer(this);
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log($"Player [netId: {netId}] [name: {PlayerName}] OnStartLocalPlayer is called");
            LobbyManager.Instance.LocalPlayer = this;
            lastSendTime = Time.time;
            RegisterHandlers();
        }

        private void Update()
        {
            if (!gameStarted && isLocalPlayer)
            {
                // send ready message constantly to the server
                if (Time.time - lastSendTime > ReadinessMessageSendInterval)
                {
                    connectionToServer.Send(MessageIds.ClientReadinessMessage, new ClientReadinessMessage
                    {
                        PlayerIndex = (int)this.netId.Value
                    });
                    lastSendTime = Time.time;
                }
            }
        }

        public override void OnNetworkDestroy()
        {
            LobbyManager.Instance.RemovePlayer(this);
        }

        public void DiscardTile(Tile tile, bool isRichiing, bool isLastDraw, int bonusTurnTime)
        {
            var message = new ClientDiscardRequestMessage
            {
                PlayerIndex = PlayerIndex,
                IsRichiing = isRichiing,
                DiscardingLastDraw = isLastDraw,
                Tile = tile,
                BonusTurnTime = bonusTurnTime
            };
            connectionToServer.Send(MessageIds.ClientDiscardTileMessage, message);
        }

        public void SkipOutTurnOperation(int bonusTurnTime)
        {
            var message = new ClientOperationMessage {
                PlayerIndex = PlayerIndex,
                Operation = new OutTurnOperation {Type = OutTurnOperationType.Skip},
                BonusTurnTime = bonusTurnTime
            };
            connectionToServer.Send(MessageIds.ClientOperationMessage, message);
        }

        private void RegisterHandlers()
        {
            LobbyManager.Instance.client.RegisterHandler(MessageIds.ServerPrepareMessage, OnGamePrepareMessageReceived);
            LobbyManager.Instance.client.RegisterHandler(MessageIds.ServerRoundStartMessage, OnRoundStartMessageReceived);
            LobbyManager.Instance.client.RegisterHandler(MessageIds.ServerDrawTileMessage, OnPlayerDrawTileMessageReceived);
            LobbyManager.Instance.client.RegisterHandler(MessageIds.ServerOtherDrawTileMessage, OnOtherPlayerDrawTileMessageReceived);
            LobbyManager.Instance.client.RegisterHandler(MessageIds.ServerDiscardOperationMessage, OnDiscardOperationMessageReceived);
            LobbyManager.Instance.client.RegisterHandler(MessageIds.ServerTurnEndMessage, OnTurnEndMessageReceived);
        }

        private void OnGamePrepareMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerGamePrepareMessage>();
            Debug.Log($"ServerGamePrepareMessage received: {content}");
            if (PlayerIndex != content.PlayerIndex)
            {
                Debug.Log($"Setting player index locally to {content.PlayerIndex}");
                PlayerIndex = content.PlayerIndex;
            }
            gameStarted = true;
            connectionToServer.Send(MessageIds.ClientReadinessMessage, new ClientReadinessMessage
            {
                PlayerIndex = PlayerIndex,
                Content = PlayerIndex
            });
            // invoke client game prepare logic with the content received.
            ClientBehaviour.Instance.GamePrepare(content);
        }

        private void OnRoundStartMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerRoundStartMessage>();
            if (PlayerIndex != content.PlayerIndex)
            {
                Debug.LogError($"Player {PlayerIndex} should not receive a message that is send to player {content.PlayerIndex}");
                return;
            }
            Debug.Log($"ServerRoundStartMessage received: {content}");
            // confirm message received
            connectionToServer.Send(MessageIds.ClientReadinessMessage, new ClientReadinessMessage
            {
                PlayerIndex = PlayerIndex,
                Content = content.InitialHandTiles.Length
            });
            // invoke client round start logic with the content received.
            ClientBehaviour.Instance.StartRound(content);
        }

        private void OnPlayerDrawTileMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerDrawTileMessage>();
            Debug.Log($"ServerDrawTileMessage received: {content}");
            // confirm message received
            connectionToServer.Send(MessageIds.ClientReadinessMessage, new ClientReadinessMessage
            {
                PlayerIndex = PlayerIndex,
                Content = MessageIds.ServerDrawTileMessage
            });
            Debug.Log($"Player bonusTurnTime is {BonusTurnTime}, received bonusTurnTime is {content.BonusTurnTime}");
            // invoke client draw tile method
            ClientBehaviour.Instance.PlayerDrawTurn(content);
        }

        private void OnOtherPlayerDrawTileMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerOtherDrawTileMessage>();
            Debug.Log($"ServerOtherDrawTileMessage received: {content}");
            // confirm message received
            connectionToServer.Send(MessageIds.ClientReadinessMessage, new ClientReadinessMessage
            {
                PlayerIndex = PlayerIndex,
                Content = MessageIds.ServerDrawTileMessage
            });
            // invoke client method for other player draw tile
            ClientBehaviour.Instance.OtherPlayerDrawTurn(content);
        }

        private void OnDiscardOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerDiscardOperationMessage>();
            Debug.Log($"ServerDiscardOperationMessage received: {content}");
            // confirm message received
            connectionToServer.Send(MessageIds.ClientReadinessMessage, new ClientReadinessMessage
            {
                PlayerIndex = PlayerIndex,
                Content = MessageIds.ServerDiscardOperationMessage
            });
            // invoke client method for discarding operations
            ClientBehaviour.Instance.PlayerOutTurnOperation(content);
        }

        private void OnTurnEndMessageReceived(NetworkMessage message) {
            var content = message.ReadMessage<ServerTurnEndMessage>();
            Debug.Log($"ServerTurnEndMessage received: {content}");
            // this message do not require confirm
            // invoke client method for turn end operations
            ClientBehaviour.Instance.PlayerTurnEnd(content);
        }
    }
}