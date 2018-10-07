using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multi.Messages;
using Single;
using Single.MahjongDataType;
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
        [Header("In Game UI Elements")] public GameObject InGameCanvas;
        public Text InGameInfoText;
        public GameObject InTurnOperationPanel;
        public Button TsumoButton;
        public Toggle RichiButton;

        public Button InTurnKongButton;

//        public Button CancelButton;
        public GameObject OutTurnOperationPanel;
        public Button RongButton;
        public Button ChowButton;
        public Button PongButton;
        public Button OutTurnKongButton;

        [Header("Object Reference Registrations")]
        public GameObject MahjongTable;

        public GameObject PlayerHandPanel;
        public GameObject PlayerOpenPanel;
        public MahjongSetManager MahjongSetManager;

        [Header("Prefab Registrations")] public GameObject MahjongTilesPrefab;

        [Header("Game Status Info")] [SyncVar] public GameTurnState TurnState;
        public Player CurrentTurnPlayer;
        [SyncVar] public int CurrentPlayerIndex;
        [SyncVar] public int OpenIndex;
        [SyncVar] public int RoundCount = 0; // Represent [East # Round]
        [SyncVar] public int CurrentExtraRound = 0;
        [SyncVar] public int FieldCount = 0;

        private GameObject mahjongTiles;
        internal MahjongSelector MahjongSelector;
        private Coroutine waitForClientCoroutine = null;

        private List<Player> players;
        private Player localPlayer;
        private bool[] responseReceived;
        private OutTurnOperationMessage[] outTurnOperations;

        public static MahjongManager Instance { get; private set; }

        private void Awake()
        {
            Debug.Log("[Server] MahjongManager Awake is called");
            Instance = this;
        }

        public override void OnStartClient()
        {
            Debug.Log("MahjongManager OnStartClient is called");
        }

        public override void OnStartServer()
        {
            Debug.Log("MahjongManager OnStartServer is called");
            ServerRegisterHandlers();
            StartCoroutine(ServerState_RoundPrepare());
        }

        [Server]
        private void ServerRegisterHandlers()
        {
            NetworkServer.RegisterHandler(MessageConstants.ReadinessMessageId, OnReadinessMessageReceived);
            NetworkServer.RegisterHandler(MessageConstants.DiscardTileMessageId, OnDiscardTileMessageReceived);
            NetworkServer.RegisterHandler(MessageConstants.OutTurnOperationMessageId,
                OnOutTurnOperationMessageReceived);
        }

        [Server]
        internal void ServerEnterGameState(GameTurnState newState, string args)
        {
            Debug.Log($"Server enters state {newState} with args {args}");
            TurnState = newState;
            RpcGameState(newState, args);
        }

        [ClientRpc]
        private void RpcGameState(GameTurnState newState, string args)
        {
            ClientHandleState(newState, args);
        }

        [Client]
        private void ClientHandleState(GameTurnState newState, string args)
        {
            Debug.Log($"Client enters state: {newState} with args {args}");
            TurnState = newState;
            // todo -- client side MahjongManager logic
            switch (TurnState)
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
            localPlayer = PlayerManager.Instance.LocalPlayer;
            localPlayer.PlayerHandPanel = PlayerHandPanel.GetComponent<PlayerHandPanel>();
        }

        [Client]
        private void ClientState_RoundStart()
        {
            Debug.Log("ClientState_RoundStart is called");
            InGameCanvas.SetActive(true);
            InGameInfoText.Print($"Current player's index: {localPlayer.PlayerIndex}");
            // Instantiate tiles and reset tiles
            if (mahjongTiles != null) Destroy(mahjongTiles);
            mahjongTiles = Instantiate(MahjongTilesPrefab, MahjongTable.transform);
            // Read player order and put tiles to right direction
            int wind = localPlayer.PlayerIndex;
            mahjongTiles.transform.Rotate(Vector3.up, 90 * wind, Space.World);
            MahjongSelector = mahjongTiles.GetComponent<MahjongSelector>();
            // todo -- ui elements (dealer display, points, etc)
        }

        [Server]
        private IEnumerator ServerState_RoundPrepare()
        {
            yield return new WaitForSeconds(1f);
            // Important: this is not redundant, time is needed for other object to initialize
            ServerEnterGameState(GameTurnState.RoundPrepare, "Determine initial dealer");
            // determine player turn order
            players = new List<Player>(PlayerManager.Instance.Players);
            localPlayer = PlayerManager.Instance.LocalPlayer;
            players.Shuffle();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerIndex = i;
                players[i].TotalPlayers = players.Count;
                players[i].Points = GameSettings.PlayerInitialPoints;
            }

            FieldCount = 1;

            yield return null;
            ServerState_RoundStart(true);
        }

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

            foreach (var player in players)
            {
                player.HandTiles = new List<Tile>();
                player.OpenMelds = new List<Meld>();
                player.HandTilesCount = 0;
                player.Richi = false;
            }

            // Throw dice
            // todo -- visual effect for throwing dice
            int dice = Random.Range(GameSettings.DiceMin, GameSettings.DiceMax + 1);
            Debug.Log($"Dice rolls {dice}");
            OpenIndex = MahjongSetManager.Open(dice);
            // Draw tiles in turn
            ServerDrawInitialTiles();
            // Get dora tiles and their indices
            var doraTiles = MahjongSetManager.DoraIndicators.ToArray();
            var doraIndices = MahjongSetManager.DoraIndicatorIndices.ToArray();
            var playerIndices = new int[players.Count];
            for (int i = 0; i < playerIndices.Length; i++) playerIndices[i] = i;
            RpcClient_RoundStart(dice, OpenIndex, playerIndices, doraTiles, doraIndices);
            // wait for all the client has done drawing initial tiles
            responseReceived = new bool[players.Count];
            Debug.Log("[Server] Waiting for clients' readiness message");
        }

        [Server]
        private int ServerDrawInitialTiles()
        {
            int count = 0;
            CurrentPlayerIndex = 0;
            for (int current = 0; current < GameSettings.InitialDrawRound * players.Count; current++)
            {
                CurrentTurnPlayer = players[CurrentPlayerIndex];
                int index = MahjongSetManager.NextIndex;
                var tiles = MahjongSetManager.DrawTiles(GameSettings.TilesEveryRound);
                count += tiles.Count;
                CurrentTurnPlayer.ServerDrawTiles(index, tiles.ToArray());
                CurrentPlayerIndex = MahjongConstants.RepeatIndex(CurrentPlayerIndex + 1, players.Count);
            }

            Assert.AreEqual(CurrentPlayerIndex, 0, "Something has went wrong in method ServerDrawInitialTiles");
            for (CurrentPlayerIndex = 0; CurrentPlayerIndex < players.Count; CurrentPlayerIndex++)
            {
                CurrentTurnPlayer = players[CurrentPlayerIndex];
                int index = MahjongSetManager.NextIndex;
                var tile = MahjongSetManager.DrawTile();
                count++;
                CurrentTurnPlayer.ServerDrawTiles(index, tile);
            }

            return count;
        }

        [Server]
        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            if (TurnState != GameTurnState.RoundStart)
            {
                Debug.LogError($"Should not receive a ReadinessMessage in {TurnState} state, ignoring this message");
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
            ServerState_PlayerInTurn();
        }

        [Server]
        private void ServerState_PlayerInTurn()
        {
            ServerEnterGameState(GameTurnState.PlayerInTurn, $"Player {CurrentTurnPlayer.PlayerIndex}'s turn");
            Debug.Log(
                "[Server] Now waiting for turn time to expire or receive a message that this player has discarded a tile");
            waitForClientCoroutine = StartCoroutine(ServerWaitForClientDiscard(CurrentTurnPlayer.LastDraw, true));
        }

        [Server]
        private IEnumerator ServerWaitForClientDiscard(Tile defaultTile, bool discardLastDraw)
        {
            yield return new WaitForSeconds(GameSettings.BaseTurnTime + CurrentTurnPlayer.BonusTurnTime +
                                            GameSettings.ServerBufferTime);
            Debug.Log($"[Server] Time out for player {CurrentTurnPlayer.PlayerIndex}, "
                      + $"automatically discard a tile {defaultTile}");
            ServerState_PlayerDiscardTile(defaultTile, discardLastDraw, InTurnOperation.Discard);
        }

        [Server]
        private void OnDiscardTileMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<DiscardTileMessage>();
            if (content.PlayerIndex != CurrentPlayerIndex) return;
            Debug.Log("[Server] Discard tile message received. " +
                      $"Player {content.PlayerIndex} is discarding tile {content.DiscardTile}, " +
                      $"discardLastDraw: {content.DiscardLastDraw}");
            if (waitForClientCoroutine != null)
            {
                StopCoroutine(waitForClientCoroutine);
                waitForClientCoroutine = null;
            }

            // todo -- handle richi, self kong
            ServerState_PlayerDiscardTile(content.DiscardTile, content.DiscardLastDraw, content.Operation);
        }

        [Server]
        private void ServerState_PlayerDiscardTile(Tile discardTile, bool discardLastDraw, InTurnOperation operation)
        {
            ServerEnterGameState(GameTurnState.PlayerDiscardTile,
                $"Player {CurrentPlayerIndex} has discard a tile {discardTile}");
            CurrentTurnPlayer.RpcDiscardTile(discardTile, discardLastDraw, operation);
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
            var serverWaitTime = GameSettings.BaseTurnTime + maxBonusTime + GameSettings.ServerBufferTime;
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
            if (TurnState != GameTurnState.PlayerDiscardTile)
            {
                Debug.LogError(
                    $"[Server] Should not receive a ReadinessMessage in {TurnState} state, ignoring this message");
                return;
            }

            var content = message.ReadMessage<OutTurnOperationMessage>();
            if (responseReceived[content.PlayerIndex]) return; // already get message from this player, ignore.
            Debug.Log($"[Server] Received out turn message from player {content.PlayerIndex}");
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
            // todo -- find which operation is to be executed
            var rongMessageIndex = Array.FindIndex(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Kong));
            if (rongMessageIndex >= 0)
            {
                var message = outTurnOperations[rongMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed a RONG of tile {message.Meld}");
                ServerState_RoundEnd(true);
                return;
            }

            var kongMessageIndex = Array.FindIndex(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Kong));
            if (kongMessageIndex >= 0)
            {
                var message = outTurnOperations[kongMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed a KONG of meld {message.Meld}");
                // todo -- rpc call to perform this operation
                return;
            }

            var pongMessageIndex = Array.FindIndex(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Pong));
            if (pongMessageIndex >= 0)
            {
                var message = outTurnOperations[pongMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed PONG of meld {message.Meld}");
                // todo -- rpc call to perform this operation
                return;
            }

            var chowMessageIndex = Array.FindIndex(outTurnOperations,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Chow));
            if (chowMessageIndex >= 0)
            {
                var message = outTurnOperations[chowMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed CHOW of meld {message.Meld}");
                // todo -- rpc call to perform this operation
                return;
            }
            // Nothing is claimed by any client, enter next player's turn
            CurrentPlayerIndex = MahjongConstants.RepeatIndex(CurrentPlayerIndex + 1, players.Count);
            ServerState_PlayerDrawTile();
        }

        [Server] // todo -- refine this 
        private void ServerState_RoundEnd(bool rong)
        {
            ServerEnterGameState(GameTurnState.RoundEnd, "A player claimed rong"); // todo -- add client round end logic
            Debug.Log("A player claimed rong");
            // todo -- add other end game operations, possibly start another round
        }

        [ClientRpc]
        private void RpcClient_RoundStart(int dice, int openIndex, int[] playerIndices, Tile[] doraTiles,
            int[] doraIndices)
        {
            InGameInfoText.Print($"Dice rolls to {dice} with open index of {openIndex}");
            localPlayer.ClientTurnDoraTiles(doraTiles, doraIndices);
            localPlayer.ClientDrawInitialTiles(openIndex, playerIndices);
        }
    }
}