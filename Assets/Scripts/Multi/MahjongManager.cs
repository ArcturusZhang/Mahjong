using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multi.Messages;
using Prototype.NetworkLobby;
using Single;
using Single.MahjongDataType;
using Single.Yakus;
using UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Multi
{
    public class MahjongManager : NetworkBehaviour
    {
        [Header("In Turn UI Elements")] public GameObject InGameCanvas;
        public Text InGameInfoText;
        public GameObject InTurnOperationPanel;
        public Button TsumoButton;
        public Button RichiButton;
        public Button InTurnKongButton;

        [Header("Out Turn UI Elements")] public GameObject OutTurnOperationPanel;
        public Button RongButton;
        public Button ChowButton;
        public Button PongButton;
        public Button OutTurnKongButton;
        public Button SkipButton;
        public MeldSelector MeldSelector;

        [Header("General UI Elements")] public TimerController TimerController;

        [Header("Object Reference Registrations")]
        public MahjongSelector MahjongSelector;
        public GameObject PlayerHandPanel;
        public MahjongSetManager MahjongSetManager;

        [Header("Game Status Info")] [SyncVar] public GameTurnState CurrentState;
        public Player CurrentTurnPlayer;
        [SyncVar] public int CurrentPlayerIndex;
        [SyncVar] public int OpenIndex;
        [SyncVar] public int RoundCount = 0; // Represent [East # Round]
        [SyncVar] public int CurrentExtraRound = 0;
        [SyncVar] public int FieldCount = 0;
        [SyncVar] public bool FirstTurn = false;
        [SyncVar] public bool LastDraw = false;
        [SyncVar] public int SceneLoaded = 0;

        private Coroutine waitForClientCoroutine = null;

        private List<Player> players;
        private bool[] responseReceived;
        private OutTurnOperationMessage[] outTurnOperations;
        private List<Tile>[] playerHandTiles;
        private List<Meld>[] playerOpenMelds;

        public static MahjongManager Instance { get; private set; }

        private void Awake()
        {
            Debug.Log("[Server] MahjongManager Awake is called");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnStartClient()
        {
            Debug.Log("MahjongManager OnStartClient is called");
        }

        public override void OnStartServer()
        {
            Debug.Log("MahjongManager OnStartServer is called");
            SceneLoaded = 0;
            ServerRegisterHandlers();
            StartCoroutine(ServerState_RoundPrepare());
        }

        [Server]
        private void ServerRegisterHandlers()
        {
            NetworkServer.RegisterHandler(MessageConstants.ReadinessMessageId, OnReadinessMessageReceived);
            NetworkServer.RegisterHandler(MessageConstants.DiscardTileMessageId, OnDiscardTileMessageReceived);
            NetworkServer.RegisterHandler(MessageConstants.InTurnOperationMessageId, OnInTurnOperationMessageReceived);
            NetworkServer.RegisterHandler(MessageConstants.OutTurnOperationMessageId,
                OnOutTurnOperationMessageReceived);
        }

        [Server]
        internal void ServerEnterGameState(GameTurnState newState, string message)
        {
            Debug.Log($"Server enters state {newState} with message {message}");
            CurrentState = newState;
            RpcGameState(newState, message);
        }

        [ClientRpc]
        private void RpcGameState(GameTurnState newState, string message)
        {
            ClientHandleState(newState, message);
        }

        [Client]
        private void ClientHandleState(GameTurnState newState, string message)
        {
            Debug.Log($"Client enters state: {newState} with message {message}");
            CurrentState = newState;
            // todo -- client side MahjongManager logic
            switch (CurrentState)
            {
                case GameTurnState.RoundPrepare:
                    ClientState_RoundPrepare();
                    break;
                case GameTurnState.RoundStart:
                    ClientState_RoundStart();
                    break;
            }
        }

        [Client]
        private void ClientState_RoundPrepare()
        {
            // set local player's ui elements
            LobbyManager.Instance.LocalPlayer.PlayerHandPanel = PlayerHandPanel.GetComponent<PlayerHandPanel>();
        }

        [Client]
        private void ClientState_RoundStart()
        {
            Debug.Log("ClientState_RoundStart is called");
            InGameCanvas.SetActive(true);
            InGameInfoText.Print($"Current player's index: {LobbyManager.Instance.LocalPlayer.PlayerIndex}");
            // Read player order and put tiles to right direction
            int wind = LobbyManager.Instance.LocalPlayer.PlayerIndex;
            MahjongSelector.transform.Rotate(Vector3.up, 90 * wind, Space.World);
            // todo -- ui elements (dealer display, points, etc)
        }

        /// <summary>
        /// Prepare a round. Determine first dealer, then start a new round.
        /// Enter RoundStart state afterwards
        /// </summary>
        /// <returns></returns>
        [Server]
        private IEnumerator ServerState_RoundPrepare()
        {
            yield return new WaitForSeconds(1);
            // Important: this is not redundant, time is needed for other object to initialize
            ServerEnterGameState(GameTurnState.RoundPrepare, "Determine initial dealer");
            // determine player turn order
            players = new List<Player>(LobbyManager.Instance.Players);
            players.Shuffle();
            Debug.Log($"[Server] This game has total {players.Count} players");
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerIndex = i;
                players[i].TotalPlayers = players.Count;
                players[i].Points = YakuManager.Instance.YakuData.InitialPoints;
            }

            FieldCount = 1;
            playerHandTiles = new List<Tile>[players.Count];
            playerOpenMelds = new List<Meld>[players.Count];

            yield return new WaitForSeconds(1f);
            ServerState_RoundStart(true);
        }

        /// <summary>
        /// Start a new round or extra round. Throw the dice and draw initial tiles for every player, send message to
        /// all clients about their initial tiles, then wait for every client has done playing their drawing animation.
        /// </summary>
        /// <param name="newRound">whether this round has a new dealer</param>
        [Server]
        private void ServerState_RoundStart(bool newRound) // todo -- convert all the ServerState methods to coroutine
        {
            ServerEnterGameState(GameTurnState.RoundStart, $"New round starts! New round: {newRound}");
            // Round status setting
            if (newRound)
            {
                RoundCount++;
                CurrentExtraRound = 0;
            }
            else
            {
                CurrentExtraRound++;
            }

            if (RoundCount > players.Count)
            {
                RoundCount = 1;
                FieldCount++;
            }

            FirstTurn = true;

            foreach (var player in players)
            {
                player.HandTilesCount = MahjongConstants.CompleteHandTilesCount;
                player.Richi = false;
            }

            // Throwing dice
            int dice = Random.Range(GameSettings.DiceMin, GameSettings.DiceMax + 1);
            Debug.Log($"Dice rolls {dice}");
            OpenIndex = MahjongSetManager.Open(dice);
            // Draw tiles in turn
            ServerDrawInitialTiles();
            // Get dora tiles and their indices
            var doraTiles = MahjongSetManager.DoraIndicators.ToArray();
            var doraIndices = MahjongSetManager.DoraIndicatorIndices.ToArray();
            // Sending initial tiles message
            for (int i = 0; i < players.Count; i++)
            {
                players[i].connectionToClient.Send(MessageConstants.InitialDrawingMessageId, new InitialDrawingMessage
                {
                    Dice = dice,
                    TotalPlayers = players.Count,
                    MountainOpenIndex = OpenIndex,
                    Tiles = playerHandTiles[i].ToArray(),
                    DoraIndicators = doraTiles.ToArray(),
                    DoraIndicatorIndices = doraIndices,
                });
                playerHandTiles[i].Sort();
            }

            // wait for all the client has done drawing initial tiles
            responseReceived = new bool[players.Count];
            Debug.Log("[Server] Waiting for clients' readiness message");
        }

        [Server]
        private int ServerDrawInitialTiles()
        {
            int count = 0;
            for (int current = 0; current < players.Count; current++)
            {
                playerHandTiles[current] = new List<Tile>();
                // initialize open meld list
                playerOpenMelds[current] = new List<Meld>();
            }
            CurrentPlayerIndex = 0;
            for (int current = 0; current < GameSettings.InitialDrawRound * players.Count; current++)
            {
//                CurrentTurnPlayer = players[CurrentPlayerIndex];
                var tiles = MahjongSetManager.DrawTiles(GameSettings.TilesEveryRound);
                count += tiles.Count;
                playerHandTiles[CurrentPlayerIndex].AddRange(tiles.ToArray());
//                CurrentTurnPlayer.ServerDrawTiles(tiles.ToArray());
                CurrentPlayerIndex = NextPlayerIndex;
            }

            Assert.AreEqual(CurrentPlayerIndex, 0, "Something has went wrong in method ServerDrawInitialTiles");
            for (int current = 0; current < players.Count; current++)
            {
//                CurrentTurnPlayer = players[CurrentPlayerIndex];
                var tile = MahjongSetManager.DrawTile();
                count++;
                playerHandTiles[CurrentPlayerIndex].Add(tile);
//                CurrentTurnPlayer.ServerDrawTiles(tile);
                CurrentPlayerIndex = NextPlayerIndex;
            }

            Assert.AreEqual(MahjongSetManager.NextIndex - MahjongSetManager.OpenIndex, count,
                $"MahjongSetManager {MahjongSetManager.NextIndex - MahjongSetManager.OpenIndex}, count {count}");

            return count;
        }

        /// <summary>
        /// This is the callback when the server receives the ReadinessMessage when a client notice the server that
        /// it has done playing initial drawing animation.
        /// Enter PlayerDrawTile state afterwards.
        /// </summary>
        /// <param name="message"></param>
        [Server]
        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            if (CurrentState != GameTurnState.RoundStart)
            {
                Debug.LogError($"Should not receive a ReadinessMessage in {CurrentState} state, ignoring this message");
                return;
            }

            var content = message.ReadMessage<ReadinessMessage>();
            Debug.Log($"Player {content.PlayerIndex} is ready");
            responseReceived[content.PlayerIndex] = true;
            if (!responseReceived.All(received => received)) return;
            Debug.Log("[Server] All players has done initial drawing, entering next state");
            CurrentPlayerIndex = 0;
            ServerState_PlayerDrawTile();
        }

        /// <summary>
        /// In this state, the current turn player draw a new tile. The server send message to current turn client
        /// which tile he is drawing, and uses a rpc call informs all the clients to play the animation of drawing
        /// a new tile.
        /// Enter PlayerInTurn state afterwards
        /// </summary>
        [Server]
        private void ServerState_PlayerDrawTile()
        {
            ServerEnterGameState(GameTurnState.PlayerDrawTile, $"Now is player {CurrentPlayerIndex}'s turn");
            Debug.Log($"Current Player: {CurrentPlayerIndex}");
            CurrentTurnPlayer = players[CurrentPlayerIndex];
            var nextIndex = MahjongSetManager.NextIndex;
            var tile = MahjongSetManager.DrawTile();
            CurrentTurnPlayer.LastDraw = tile;
            // informs this player about this tile and the operations
            CurrentTurnPlayer.RpcYourTurnToDraw(nextIndex);
            CurrentTurnPlayer.connectionToClient.Send(MessageConstants.DrawTileMessageId,
                new DrawTileMessage {Tile = tile});
            // server enters next state
            ServerState_PlayerInTurn(CurrentTurnPlayer.LastDraw);
        }

        /// <summary>
        /// Player handles the new tile just drawn. Server waits until the turn time expires or receives player
        /// discard tile message. 
        /// </summary>
        /// <param name="lastTile"></param>
        [Server]
        private void ServerState_PlayerInTurn(Tile lastTile)
        {
            ServerEnterGameState(GameTurnState.PlayerInTurn, $"Player {CurrentTurnPlayer.PlayerIndex}'s turn");
            Debug.Log(
                "[Server] Now waiting for turn time to expire or receive a message that this player has discarded a tile");
            waitForClientCoroutine = StartCoroutine(ServerWaitForClientDiscard(lastTile, true));
        }

        [Server]
        private void OnInTurnOperationMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<InTurnOperationMessage>();
            if (content.PlayerIndex != CurrentPlayerIndex)
            {
                Debug.Log(
                    $"[Server] Received message from player {content.PlayerIndex} in player {CurrentPlayerIndex}'s turn, ignoring");
                return;
            }

            // todo -- server side handle in turn operation
            Debug.Log($"[Server] Received message from player {content.PlayerIndex}, "
                      + $"requesting operation {content.Operation} with tile {content.Meld}");
            CurrentTurnPlayer.BonusTurnTime = content.BonusTurnTime;
        }

        [Server]
        private IEnumerator ServerWaitForClientDiscard(Tile defaultTile, bool discardLastDraw)
        {
            yield return new WaitForSeconds(YakuManager.Instance.YakuData.BaseTurnTime +
                                            CurrentTurnPlayer.BonusTurnTime + GameSettings.ServerBufferTime);
            Debug.Log($"[Server] Time out for player {CurrentTurnPlayer.PlayerIndex}, "
                      + $"automatically discard a tile {defaultTile}");
            ServerState_PlayerDiscardTile(defaultTile, discardLastDraw, InTurnOperation.Discard);
        }

        [Server]
        private void OnDiscardTileMessageReceived(NetworkMessage message)
        {
            if (CurrentState != GameTurnState.PlayerInTurn)
            {
                Debug.Log(
                    $"[Server] Server received a discard tile message at state {CurrentState}, ignoring this message");
                return;
            }

            var content = message.ReadMessage<DiscardTileMessage>();
            if (content.PlayerIndex != CurrentPlayerIndex) return;
            Debug.Log("[Server] Discard tile message received. " +
                      $"Player {content.PlayerIndex} is discarding tile {content.DiscardTile}, " +
                      $"discardLastDraw: {content.DiscardLastDraw}");
            if (!content.DiscardLastDraw)
            {
                playerHandTiles[CurrentPlayerIndex].Remove(content.DiscardTile);
                playerHandTiles[CurrentPlayerIndex].Add(CurrentTurnPlayer.LastDraw);
                playerHandTiles[CurrentPlayerIndex].Sort();
            }
            CurrentTurnPlayer.BonusTurnTime = content.BonusTurnTime; // update bonus time left
            if (waitForClientCoroutine != null)
            {
                StopCoroutine(waitForClientCoroutine);
                waitForClientCoroutine = null;
            }

            // todo -- self kong and tsumo
            // handle other operations
            ServerState_PlayerDiscardTile(content.DiscardTile, content.DiscardLastDraw, content.Operation);
        }

        [Server]
        private void ServerState_PlayerDiscardTile(Tile discardTile, bool discardLastDraw, InTurnOperation operation)
        {
            ServerEnterGameState(GameTurnState.PlayerDiscardTile,
                $"Player {CurrentPlayerIndex} has discard a tile {discardTile}");
            CurrentTurnPlayer.RpcDiscardTile(discardTile, discardLastDraw, operation);
            // Handle richi
            if (CurrentTurnPlayer.OneShot) CurrentTurnPlayer.OneShot = false;
            if (operation.HasFlag(InTurnOperation.Richi))
            {
                CurrentTurnPlayer.Richi = true;
                CurrentTurnPlayer.OneShot = true;
                if (FirstTurn) CurrentTurnPlayer.WRichi = true;
            }

            foreach (var player in players)
            {
                if (player == CurrentTurnPlayer) continue;
                player.RpcOutTurnOperation(discardTile);
            }

            Debug.Log(
                "[Server] Now waiting for turn time to expire or receive messages that any clients have out turn operation.");
            responseReceived = new bool[players.Count];
            responseReceived[CurrentPlayerIndex] = true; // current player do not need to send this message
            outTurnOperations = new OutTurnOperationMessage[players.Count]; // used for storing received message
            waitForClientCoroutine = StartCoroutine(ServerWaitForOutTurnOperations());
        }

        [Server]
        private IEnumerator ServerWaitForOutTurnOperations()
        {
            var maxBonusTime = players.Max(player => player.BonusTurnTime);
            var serverWaitTime = YakuManager.Instance.YakuData.BaseTurnTime + maxBonusTime +
                                 GameSettings.ServerBufferTime;
            Debug.Log($"[Server] Server will wait for {serverWaitTime} seconds");
            yield return new WaitForSeconds(serverWaitTime);
            Debug.Log("[Server] Time out when waiting out turn operations.");
            Debug.Log(
                $"[Server] response received: {responseReceived.Aggregate(new StringBuilder(), (builder, received) => builder.Append(received).Append(" "))}");
            ServerState_PlayerOutTurnOperation();
        }

        [Server]
        private void OnOutTurnOperationMessageReceived(NetworkMessage message)
        {
            if (CurrentState != GameTurnState.PlayerDiscardTile)
            {
                Debug.LogError(
                    $"[Server] Should not receive a InTurnOperationMessage in {CurrentState} state, ignoring it");
                return;
            }

            var content = message.ReadMessage<OutTurnOperationMessage>();
            if (responseReceived[content.PlayerIndex]) return; // already get message from this player, ignore.
            Debug.Log($"[Server] Received out turn message from player {content.PlayerIndex}");
            players[content.PlayerIndex].BonusTurnTime = content.BonusTurnTime;
            responseReceived[content.PlayerIndex] = true;
            outTurnOperations[content.PlayerIndex] = content;
            if (!responseReceived.All(received => received)) return;
            Debug.Log("[Server] All players have sent out turn operation message, dealing with out turn operation");
            // stop server count down
            if (waitForClientCoroutine != null)
            {
                StopCoroutine(waitForClientCoroutine);
                waitForClientCoroutine = null;
            }

            // Enter playerOutTurnOperation state
            ServerState_PlayerOutTurnOperation();
        }

        [Server]
        private void ServerState_PlayerOutTurnOperation()
        {
            ServerEnterGameState(GameTurnState.PlayerOutTurnOperation, "Enters player out turn operation state");
            // find which operation is to be executed
            var rongMessages = Array.FindAll(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Rong));
            if (rongMessages.Length > 0)
            {
                Debug.Log($"[Server] {rongMessages.Length} players claimed RONG");
                foreach (var message in rongMessages)
                {
                    Debug.Log($"[Server] Player {message.PlayerIndex} has claimed a RONG of tile {message.DiscardedTile}");
                }

                // todo -- rpc call to perform this operation
                ServerState_RoundEnd(true);
                return;
            }

            var kongMessageIndex = Array.FindIndex(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Kong));
            if (kongMessageIndex >= 0)
            {
                var message = outTurnOperations[kongMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed a KONG of meld {message.Meld}");
                OpenMeldBreaksOneShotAndFirstRound();
                // todo -- rpc call to perform this operation
                return;
            }

            var pongMessageIndex = Array.FindIndex(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Pong));
            if (pongMessageIndex >= 0)
            {
                var message = outTurnOperations[pongMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed PONG of meld {message.Meld}");
                OpenMeldBreaksOneShotAndFirstRound();
                // todo -- rpc call to perform this operation
                return;
            }

            var chowMessageIndex = Array.FindIndex(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Chow));
            if (chowMessageIndex >= 0)
            {
                var message = outTurnOperations[chowMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed CHOW of meld {message.Meld}");
                OpenMeldBreaksOneShotAndFirstRound();
                // todo -- rpc call to perform this operation
                CurrentPlayerIndex = message.PlayerIndex;
                CurrentTurnPlayer = players[CurrentPlayerIndex];
                CurrentTurnPlayer.HandTilesCount -= message.Meld.Tiles.Length;
                CurrentTurnPlayer.RpcPerformChow(message.PlayerIndex, message.Meld, message.DiscardedTile);
                var defaultTile = playerHandTiles[CurrentPlayerIndex].RemoveLast();
                CurrentTurnPlayer.LastDraw = defaultTile;
                playerOpenMelds[CurrentPlayerIndex].Add(message.Meld);
                ServerState_PlayerInTurn(defaultTile);
                Debug.Log($"[Server] Sending message to player {message.PlayerIndex}, default discard: {defaultTile}");
                players[message.PlayerIndex].connectionToClient.Send(MessageConstants.DiscardAfterOpenMessageId,
                    new DiscardAfterOpenMessage
                    {
                        PlayerIndex = message.PlayerIndex,
                        DefaultTile = defaultTile
                    });
                return;
            }

            // Nothing is claimed by any client, enter next player's turn
            CurrentPlayerIndex = NextPlayerIndex;
            ServerState_PlayerDrawTile();
        }

        [Server] // todo -- complete this 
        private void ServerState_RoundEnd(bool rong)
        {
            ServerEnterGameState(GameTurnState.RoundEnd, "A player claimed rong"); // todo -- add client round end logic
            Debug.Log("A player claimed rong");
            // todo -- add other end game operations, possibly start another round
        }

        [Server]
        private void OpenMeldBreaksOneShotAndFirstRound()
        {
            FirstTurn = false;
            foreach (var player in players)
            {
                player.OneShot = false;
            }
        }


        public int NextPlayerIndex =>
            MahjongConstants.RepeatIndex(CurrentPlayerIndex + 1, LobbyManager.Instance.LocalPlayer.TotalPlayers);
    }
}