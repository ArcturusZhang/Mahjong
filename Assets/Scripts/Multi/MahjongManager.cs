using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lobby;
using Multi.GameState;
using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utils;

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

        public PlayerHandPanel PlayerHandPanel;
        public MahjongSetManager MahjongSetManager;

        [Header("Game Status Info")] public GameStatus GameStatus;
        public GameSettings GameSettings;

        private Coroutine waitForClientCoroutine = null;

        private MahjongStateMachine stateMachine;

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
            stateMachine = new MahjongStateMachine();
            GameStatus.Players = LobbyManager.Instance.Players;
            StartCoroutine(StartServerGameLoop());
        }

        [Server]
        private IEnumerator StartServerGameLoop()
        {
            yield return new WaitForSeconds(1f);
            stateMachine.ChangeState(new RoundPrepareState
            {
                MahjongManager = this,
                GameSettings = GameSettings,
                GameStatus = GameStatus
            });
            yield return new WaitForSeconds(1f);
            // Start first round
            stateMachine.ChangeState(new RoundStartState
            {
                MahjongManager = this,
                NewRound = true,
                GameSettings = GameSettings,
                GameStatus = GameStatus,
                MahjongSetManager = MahjongSetManager,
                ServerCallback = ServerNextTurn
            });
            Debug.Log("[Server] Waiting for clients' readiness message");
        }

        [Server]
        internal void ServerNextTurn()
        {
            stateMachine.ChangeState(new PlayerDrawTileState
            {
                MahjongSetManager = MahjongSetManager,
                GameStatus = GameStatus,
                Lingshang = false,
                ServerCallback = (tile, discardLastDraw, operation) =>
                {
                    StopWaitingCoroutine();
                    ServerDiscardTile(tile, discardLastDraw, operation);
                },
            });
            waitForClientCoroutine =
                StartCoroutine(ServerWaitForClientDiscard());
        }

        [Server]
        private IEnumerator ServerWaitForClientDiscard()
        {
            yield return new WaitForSeconds(GameSettings.BaseTurnTime + GameStatus.CurrentTurnPlayer.BonusTurnTime +
                                            GameSettings.ServerBufferTime);
            Debug.Log($"[Server] Time out for player {GameStatus.CurrentPlayerIndex}, "
                      + $"automatically discard {GameStatus.CurrentTurnPlayer.LastDraw}");
            ServerDiscardTile(GameStatus.CurrentTurnPlayer.LastDraw, true, InTurnOperation.Discard);
        }

        [Server]
        private void StopWaitingCoroutine()
        {
            if (waitForClientCoroutine == null) return;
            StopCoroutine(waitForClientCoroutine);
            waitForClientCoroutine = null;
        }

        [Server]
        private void ServerDiscardTile(Tile discardTile, bool discardLastDraw, InTurnOperation operation)
        {
            var outTurnOperationMessages = new OutTurnOperationMessage[GameStatus.Players.Count];
            var playerDiscardTileState = new PlayerDiscardTileState
            {
                GameStatus = GameStatus,
                DiscardTile = discardTile,
                DiscardLastDraw = discardLastDraw,
                InTurnOperation = operation,
                OutTurnOperationMessages = outTurnOperationMessages,
                ServerCallback = messages =>
                {
                    StopWaitingCoroutine();
                    ServerHandleOutTurnOperations(messages);
                }
            };
            stateMachine.ChangeState(playerDiscardTileState);
            waitForClientCoroutine = StartCoroutine(ServerWaitForOutTurnOperations(outTurnOperationMessages));
        }

        [Server]
        private IEnumerator ServerWaitForOutTurnOperations(OutTurnOperationMessage[] outTurnOperationMessages)
        {
            var maxBonusTime = GameStatus.Players.Max(player => player.BonusTurnTime);
            var serverWaitTime = GameSettings.BaseTurnTime + maxBonusTime + GameSettings.ServerBufferTime;
            Debug.Log($"[Server] Server will wait for {serverWaitTime} seconds for out turn operations");
            yield return new WaitForSeconds(serverWaitTime);
            Debug.Log("[Server] Time out when waiting out turn operations.");
            // todo -- disable out turn ui panel for all clients
            ServerHandleOutTurnOperations(outTurnOperationMessages);
        }

        [Server]
        private void ServerHandleOutTurnOperations(OutTurnOperationMessage[] outTurnOperationMessages)
        {
            Debug.Log($"[Server] Handling out turn operations");
            var rongMessages = Array.FindAll(outTurnOperationMessages,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Rong));
            if (rongMessages.Length > 0)
            {
                Debug.Log($"[Server] {rongMessages.Length} players claimed RONG");
                // todo -- rpc call to handle this operation
                return;
            }

            var kongMessageIndex = Array.FindIndex(outTurnOperationMessages,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Kong));
            if (kongMessageIndex >= 0)
            {
                var message = outTurnOperationMessages[kongMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed KONG of meld {message.Meld}");
                BreakFirstTurn();
                BreakOneShot();
                int discardPlayerIndex = GameStatus.CurrentPlayerIndex;
                GameStatus.SetCurrentPlayerIndex(message.PlayerIndex);
                var currentPlayerIndex = GameStatus.CurrentPlayerIndex;
                var currentTurnPlayer = GameStatus.CurrentTurnPlayer;
                currentTurnPlayer.RpcPerformKong(message.PlayerIndex, message.Meld, message.DiscardedTile,
                    discardPlayerIndex);
                // server side data update
                currentTurnPlayer.HandTilesCount -= message.Meld.EffectiveTileCount;
                GameStatus.PlayerHandTiles[currentPlayerIndex].Remove(message.Meld, message.DiscardedTile);
                GameStatus.PlayerOpenMelds[currentPlayerIndex].Add(message.Meld);
                Assert.AreEqual(GameStatus.PlayerHandTiles[currentPlayerIndex].Count, currentTurnPlayer.HandTilesCount,
                    "Hand tile count should equal to data on server");
                stateMachine.ChangeState(new PlayerDrawTileState
                {
                    MahjongSetManager = MahjongSetManager,
                    GameStatus = GameStatus,
                    Lingshang = true,
                    ServerCallback = (tile, discardLastDraw, operation) =>
                    {
                        StopWaitingCoroutine();
                        ServerDiscardTile(tile, discardLastDraw, operation);
                    }
                });
                waitForClientCoroutine = StartCoroutine(ServerWaitForClientDiscard());
                return;
            }

            var pongMessageIndex = Array.FindIndex(outTurnOperationMessages,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Pong));
            if (pongMessageIndex >= 0)
            {
                var message = outTurnOperationMessages[pongMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed PONG of meld {message.Meld}");
                BreakFirstTurn();
                BreakOneShot();
                int discardPlayerIndex = GameStatus.CurrentPlayerIndex;
                GameStatus.SetCurrentPlayerIndex(message.PlayerIndex);
                var currentPlayerIndex = GameStatus.CurrentPlayerIndex;
                var currentTurnPlayer = GameStatus.CurrentTurnPlayer;
                currentTurnPlayer.RpcPerformPong(message.PlayerIndex, message.Meld, message.DiscardedTile,
                    discardPlayerIndex);
                // server side data update
                currentTurnPlayer.HandTilesCount -= message.Meld.EffectiveTileCount;
                GameStatus.PlayerHandTiles[currentPlayerIndex].Remove(message.Meld, message.DiscardedTile);
                GameStatus.PlayerOpenMelds[currentPlayerIndex].Add(message.Meld);
                var defaultTile = GameStatus.PlayerHandTiles[currentPlayerIndex].RemoveLast();
                Assert.AreEqual(GameStatus.PlayerHandTiles[currentPlayerIndex].Count, currentTurnPlayer.HandTilesCount,
                    "Hand tile count should equal to data on server");
                stateMachine.ChangeState(new PlayerOpenMeldState
                {
                    DefaultTile = defaultTile,
                    GameStatus = GameStatus,
                    ServerCallback = (tile, discardLastDraw, operation) =>
                    {
                        StopWaitingCoroutine();
                        ServerDiscardTile(tile, discardLastDraw, operation);
                    }
                });
                waitForClientCoroutine = StartCoroutine(ServerWaitForClientDiscard());
                return;
            }

            var chowMessageIndex = Array.FindIndex(outTurnOperationMessages,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Chow));
            if (chowMessageIndex >= 0)
            {
                var message = outTurnOperationMessages[chowMessageIndex];
                Debug.Log($"[Server] Player {message.PlayerIndex} has claimed CHOW of meld {message.Meld}");
                BreakFirstTurn();
                BreakOneShot();
                GameStatus.SetCurrentPlayerIndex(message.PlayerIndex);
                var currentPlayerIndex = GameStatus.CurrentPlayerIndex;
                var currentTurnPlayer = GameStatus.CurrentTurnPlayer;
                currentTurnPlayer.RpcPerformChow(message.Meld, message.DiscardedTile);
                // server side data update
                currentTurnPlayer.HandTilesCount -= message.Meld.EffectiveTileCount;
                GameStatus.PlayerHandTiles[currentPlayerIndex].Remove(message.Meld, message.DiscardedTile);
                GameStatus.PlayerOpenMelds[currentPlayerIndex].Add(message.Meld);
                var defaultTile = GameStatus.PlayerHandTiles[currentPlayerIndex].RemoveLast();
                Assert.AreEqual(GameStatus.PlayerHandTiles[currentPlayerIndex].Count, currentTurnPlayer.HandTilesCount,
                    "Hand tile count should equal to data on server");
                stateMachine.ChangeState(new PlayerOpenMeldState
                {
                    DefaultTile = defaultTile,
                    GameStatus = GameStatus,
                    ServerCallback = (tile, discardLastDraw, operation) =>
                    {
                        StopWaitingCoroutine();
                        ServerDiscardTile(tile, discardLastDraw, operation);
                    }
                });
                waitForClientCoroutine = StartCoroutine(ServerWaitForClientDiscard());
                return;
            }

            Debug.Log($"[Server] No one claims out turn operations, entering next player's turn");
            GameStatus.SetCurrentPlayerIndex(GameStatus.NextPlayerIndex);
            ServerNextTurn();
        }

        [Server]
        private void BreakOneShot()
        {
            foreach (var player in GameStatus.Players)
            {
                player.OneShot = false;
            }
        }

        [Server]
        private void BreakFirstTurn()
        {
            foreach (var player in GameStatus.Players)
            {
                player.FirstTurn = false;
            }
        }

        [ClientRpc]
        internal void RpcClientPrepare()
        {
            LobbyManager.Instance.LocalPlayer.PlayerHandPanel = PlayerHandPanel;
        }

        [ClientRpc]
        internal void RpcClientRoundStart()
        {
            Debug.Log("ClientState_RoundStart is called");
            InGameCanvas.SetActive(true);
            InGameInfoText.Print($"Current player's index: {LobbyManager.Instance.LocalPlayer.PlayerIndex}");
            // Read player order and put tiles to right direction
            int wind = LobbyManager.Instance.LocalPlayer.PlayerIndex;
            MahjongSelector.transform.Rotate(Vector3.up, 90 * wind, Space.World);
            // todo -- ui elements (dealer display, points, etc)
        }
    }
}