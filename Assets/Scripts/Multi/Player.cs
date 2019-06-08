using Lobby;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Networking;


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
            var message = new ClientDiscardTileMessage
            {
                PlayerIndex = PlayerIndex,
                IsRichiing = isRichiing,
                DiscardingLastDraw = isLastDraw,
                Tile = tile,
                BonusTurnTime = bonusTurnTime
            };
            connectionToServer.Send(MessageIds.ClientDiscardTileMessage, message);
        }

        public void InTurnOperationTaken(InTurnOperation operation, int bonusTurnTime)
        {
            var message = new ClientInTurnOperationMessage
            {
                PlayerIndex = PlayerIndex,
                Operation = operation,
                BonusTurnTime = bonusTurnTime
            };
            connectionToServer.Send(MessageIds.ClientInTurnOperationMessage, message);
        }

        public void SkipOutTurnOperation(int bonusTurnTime)
        {
            OutTurnOperationTaken(new OutTurnOperation { Type = OutTurnOperationType.Skip }, bonusTurnTime);
        }

        public void OutTurnOperationTaken(OutTurnOperation operation, int bonusTurnTime)
        {
            var message = new ClientOutTurnOperationMessage
            {
                PlayerIndex = PlayerIndex,
                Operation = operation,
                BonusTurnTime = bonusTurnTime
            };
            connectionToServer.Send(MessageIds.ClientOutTurnOperationMessage, message);
        }

        public void ClientReady(int code)
        {
            var message = new ClientReadinessMessage
            {
                PlayerIndex = PlayerIndex,
                Content = code
            };
            connectionToServer.Send(MessageIds.ClientReadinessMessage, message);
        }

        public void RequestNewRound()
        {
            var message = new ClientNextRoundMessage
            {
                PlayerIndex = PlayerIndex
            };
            connectionToServer.Send(MessageIds.ClientNextRoundMessage, message);
        }

        private void RegisterHandlers()
        {
            RegisterHandler(MessageIds.ServerGamePrepareMessage, OnGamePrepareMessageReceived);
            RegisterHandler(MessageIds.ServerRoundStartMessage, OnRoundStartMessageReceived);
            RegisterHandler(MessageIds.ServerDrawTileMessage, OnPlayerDrawTileMessageReceived);
            RegisterHandler(MessageIds.ServerDiscardOperationMessage, OnDiscardOperationMessageReceived);
            RegisterHandler(MessageIds.ServerKongMessage, OnKongMessageReceived);
            RegisterHandler(MessageIds.ServerTurnEndMessage, OnTurnEndMessageReceived);
            RegisterHandler(MessageIds.ServerOperationPerformMessage, OnOperationPerformedMessageReceived);
            RegisterHandler(MessageIds.ServerRoundDrawMessage, OnRoundDrawMessageReceived);
            RegisterHandler(MessageIds.ServerTsumoMessage, OnPlayerTsumoMessageReceived);
            RegisterHandler(MessageIds.ServerRongMessage, OnPlayerRongMessageReceived);
            RegisterHandler(MessageIds.ServerPointTransferMessage, OnPointTransferMessageReceived);
            RegisterHandler(MessageIds.ServerGameEndMessage, OnGameEndMessageReceived);
        }

        private void RegisterHandler(short messageId, NetworkMessageDelegate handler)
        {
            LobbyManager.Instance.client.RegisterHandler(messageId, handler);
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
            ClientBehaviour.Instance.GamePrepare(content);
        }

        private void OnRoundStartMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerRoundStartMessage>();
            Debug.Log($"ServerRoundStartMessage received: {content}");
            // invoke client round start logic with the content received.
            ClientBehaviour.Instance.StartRound(content);
        }

        private void OnPlayerDrawTileMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerDrawTileMessage>();
            Debug.Log($"ServerDrawTileMessage received: {content}");
            // invoke client draw tile method
            ClientBehaviour.Instance.PlayerDrawTurn(content);
        }

        private void OnDiscardOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerDiscardOperationMessage>();
            Debug.Log($"ServerDiscardOperationMessage received: {content}");
            // invoke client method for discarding operations
            ClientBehaviour.Instance.PlayerDiscardOperation(content);
        }

        private void OnKongMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerKongMessage>();
            Debug.Log($"ServerKongMessage: {content}");
            // invoke client method for kong
            ClientBehaviour.Instance.PlayerKong(content);
        }

        private void OnTurnEndMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerTurnEndMessage>();
            Debug.Log($"ServerTurnEndMessage received: {content}");
            // invoke client method for turn end operations
            ClientBehaviour.Instance.PlayerTurnEnd(content);
        }

        private void OnOperationPerformedMessageReceived(NetworkMessage message) {
            var content  = message.ReadMessage<ServerOperationPerformMessage>();
            Debug.Log($"ServerOperationPerformMessage received: {content}");
            // invoke client method for operation perform
            ClientBehaviour.Instance.OperationPerform(content);
        }

        private void OnPlayerTsumoMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerPlayerTsumoMessage>();
            Debug.Log($"ServerPlayerTsumoMessage received: {content}");
            // invoke client method for tsumo operation
            ClientBehaviour.Instance.PlayerTsumo(content);
        }

        private void OnPlayerRongMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerPlayerRongMessage>();
            Debug.Log($"ServerPlayerRongMessage received: {content}");
            // invoke client method for rong operations
            ClientBehaviour.Instance.PlayerRong(content);
        }

        private void OnRoundDrawMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerRoundDrawMessage>();
            Debug.Log($"ServerRoundDrawMessage received: {content}");
            // invoke client method for round draw operations
            ClientBehaviour.Instance.RoundDraw(content);
        }

        private void OnPointTransferMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerPointTransferMessage>();
            Debug.Log($"ServerPointTransferMessage received: {content}");
            // invoke client method for point transfer
            ClientBehaviour.Instance.PointTransfer(content);
        }

        private void OnGameEndMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ServerGameEndMessage>();
            Debug.Log($"ServerGameEndMessage received: {content}");
            // invoke client method for game end summary
            ClientBehaviour.Instance.GameEnd(content);
        }
    }
}