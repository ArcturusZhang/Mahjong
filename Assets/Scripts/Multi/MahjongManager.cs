using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Lobby;
using Multi.GameState;
using Multi.Messages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using UI;
using UI.PointSummaryPanel;
using UI.RoundEndPanel;
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

        [Header("Round End UI Elements")] public PointSummaryPanelController PointSummaryPanelController;
        public RoundEndPanelController RoundEndPanelController;

        [Header("General UI Elements")] public TimerController TimerController;
        public RoundStatusPanel RoundStatusPanel;

        [Header("Object Reference Registrations")]
        public MahjongSelector MahjongSelector;

        public PlayerHandPanel PlayerHandPanel;
        public MahjongSetManager MahjongSetManager;

        [Header("Game Status Info")] public GameStatus GameStatus;
        public NetworkRoundStatus NetworkRoundStatus;
        [HideInInspector] public GameSettings GameSettings;
        [HideInInspector] public YakuSettings YakuSettings;

        private Coroutine waitForClientCoroutine = null;

        private MahjongStateMachine stateMachine;

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
            GameSettings = LobbyManager.Instance.GameSettings;
            YakuSettings = LobbyManager.Instance.YakuSettings;
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
                NetworkRoundStatus = NetworkRoundStatus,
                GameSettings = GameSettings,
                GameStatus = GameStatus
            });
            yield return new WaitForSeconds(1f);
            RpcClientPrepare(GameSettings, YakuSettings);
            // Start first round
            ServerNextRound(true, false);
            Debug.Log("[Server] Waiting for clients' readiness message");
        }

        [Server]
        private void ServerNextRound(bool newRound, bool extraRound)
        {
            RpcClientRoundStart(newRound, extraRound);
            stateMachine.ChangeState(new RoundStartState
            {
                NewRound = newRound,
                ExtraRound = extraRound,
                NetworkRoundStatus = NetworkRoundStatus,
                GameSettings = GameSettings,
                GameStatus = GameStatus,
                MahjongSetManager = MahjongSetManager,
                ServerCallback = ServerNextTurn
            });
        }

        [Server]
        internal void ServerNextTurn()
        {
            // todo -- check for early ending
            stateMachine.ChangeState(new PlayerDrawTileState
            {
                MahjongSetManager = MahjongSetManager,
                NetworkRoundStatus = NetworkRoundStatus,
                GameStatus = GameStatus,
                Lingshang = false,
                ServerInTurnCallback = data =>
                {
                    StopWaitingCoroutine();
                    ServerInTurnOperation(data);
                },
                ServerDiscardCallback = data =>
                {
                    StopWaitingCoroutine();
                    ServerDiscardTile(data);
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
            ServerDiscardTile(new DiscardTileData
            {
                DiscardTile = GameStatus.CurrentTurnPlayer.LastDraw,
                DiscardLastDraw = true,
                Operation = InTurnOperation.Discard
            });
        }

        [Server]
        private PlayerServerData ValidatePlayerSideData(PlayerClientData playerClientData)
        {
            var data = new PlayerServerData
            {
                HandTiles = playerClientData.HandTiles,
                OpenMelds = playerClientData.OpenMelds,
                WinningTile = playerClientData.WinningTile,
                PlayerIndex = playerClientData.WinPlayerIndex,
                HandStatus = playerClientData.HandStatus,
                RoundStatus = playerClientData.RoundStatus,
                DoraIndicators = MahjongSetManager.DoraIndicators.ToArray(),
                UraDoraIndicators = MahjongSetManager.UraDoraIndicators.ToArray()
            };
            // validate
            var index = data.PlayerIndex;
            Assert.IsTrue(ClientUtil.ArrayEquals(data.HandTiles, GameStatus.PlayerHandTiles[index].ToArray()));
            Assert.IsTrue(ClientUtil.ArrayEquals(data.OpenMelds, GameStatus.PlayerOpenMelds[index].ToArray()));
            // todo -- winning tile also need validation 
            return data;
        }

        [Server]
        private void ServerInTurnOperation(InTurnOperationData inTurnOperationData)
        {
            var operation = inTurnOperationData.Operation;
            if (operation.HasFlag(InTurnOperation.Tsumo))
            {
                Debug.Log($"[Server] Player {inTurnOperationData.PlayerIndex} has claimed tsumo, "
                          + $"player side data: {inTurnOperationData.PlayerClientData}");
                // validate data from client
                var data = ValidatePlayerSideData(inTurnOperationData.PlayerClientData);
                var pointTransfers =
                    MahjongScoring.GetPointsTransfers(RoundEndType.Tsumo, NetworkRoundStatus, GameStatus, data);
                // rpc call to show visual effect and summary panel
                RpcClientRoundEnd(new RoundEndData
                {
                    RoundEndType = RoundEndType.Tsumo,
                    TotalPlayer = GameStatus.TotalPlayer,
                    PlayerServerDataArray = new[] {data},
                    PointsTransfers = pointTransfers
                });
                // todo -- switch to RoundEndState
                stateMachine.ChangeState(new RoundEndState
                {
                    GameStatus = GameStatus,
                    ServerNextRoundCallback = ServerNextRound
                });
                // todo -- rpc call to show round end panel
                return;
            }

            if (operation.HasFlag(InTurnOperation.Kong))
            {
                Debug.Log(
                    $"[Server] Player {inTurnOperationData.PlayerIndex} has claimed kong for {inTurnOperationData.Meld}");
                // rpc call to perform kong on clients
                GameStatus.CurrentTurnPlayer.RpcPerformInTurnKong(inTurnOperationData.PlayerIndex,
                    inTurnOperationData.Meld, inTurnOperationData.LastDraw);
                var meld = inTurnOperationData.Meld;
                var added = meld.Revealed &&
                            meld.Tiles.Contains(inTurnOperationData.LastDraw, Tile.TileConsiderColorEqualityComparer);
                if (!added) GameStatus.CurrentTurnPlayer.HandTilesCount -= meld.EffectiveTileCount;
                stateMachine.ChangeState(new PlayerDrawTileState
                {
                    MahjongSetManager = MahjongSetManager,
                    NetworkRoundStatus = NetworkRoundStatus,
                    GameStatus = GameStatus,
                    Lingshang = true,
                    ServerInTurnCallback = data =>
                    {
                        StopWaitingCoroutine();
                        ServerInTurnOperation(data);
                    },
                    ServerDiscardCallback = data =>
                    {
                        StopWaitingCoroutine();
                        ServerDiscardTile(data);
                    }
                });
                return;
            }
        }

        [Server]
        private void ServerDiscardTile(DiscardTileData data)
        {
            var outTurnOperationMessages = new OutTurnOperationMessage[GameStatus.Players.Count];
            var playerDiscardTileState = new PlayerDiscardTileState
            {
                GameStatus = GameStatus,
                GameSettings = GameSettings,
                DiscardTile = data.DiscardTile,
                DiscardLastDraw = data.DiscardLastDraw,
                InTurnOperation = data.Operation,
                OutTurnOperationMessages = outTurnOperationMessages,
                ServerCallback = messages =>
                {
                    StopWaitingCoroutine();
                    ServerOutTurnOperations(messages);
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
            ServerOutTurnOperations(outTurnOperationMessages);
        }

        [Server]
        private void ServerOutTurnOperations(OutTurnOperationMessage[] outTurnOperationMessages)
        {
            Debug.Log($"[Server] Handling out turn operations");
            var rongMessages = Array.FindAll(outTurnOperationMessages,
                message => message != null && message.Operation.HasFlag(OutTurnOperation.Rong));
            if (rongMessages.Length > 0)
            {
                Debug.Log($"[Server] {rongMessages.Length} players claimed RONG");
                // todo -- validate player side data and sort from current player
                // todo -- rpc call to show visual effect
                // todo -- rpc call to show summary panel
                RpcClientRoundEnd(new RoundEndData
                {
                    // todo -- RoundEndData
                });
                // todo -- switch to RoundEndState
                stateMachine.ChangeState(new RoundEndState
                {
                    GameStatus = GameStatus,
                    ServerNextRoundCallback = ServerNextRound
                });
                // todo -- rpc call to show summary panel
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
                currentTurnPlayer.RpcPerformKong(message.PlayerIndex, message.Meld, message.DiscardTile,
                    discardPlayerIndex);
                // server side data update
                currentTurnPlayer.HandTilesCount -= message.Meld.EffectiveTileCount;
                GameStatus.PlayerHandTiles[currentPlayerIndex].Subtract(message.Meld.Tiles, message.DiscardTile);
                GameStatus.PlayerOpenMelds[currentPlayerIndex].Add(message.Meld);
                Assert.AreEqual(GameStatus.PlayerHandTiles[currentPlayerIndex].Count, currentTurnPlayer.HandTilesCount,
                    "Hand tile count should equal to data on server");
                stateMachine.ChangeState(new PlayerDrawTileState
                {
                    MahjongSetManager = MahjongSetManager,
                    NetworkRoundStatus = NetworkRoundStatus,
                    GameStatus = GameStatus,
                    Lingshang = true,
                    ServerInTurnCallback = data =>
                    {
                        StopWaitingCoroutine();
                        ServerInTurnOperation(data);
                    },
                    ServerDiscardCallback = data =>
                    {
                        StopWaitingCoroutine();
                        ServerDiscardTile(data);
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
                currentTurnPlayer.RpcPerformPong(message.PlayerIndex, message.Meld, message.DiscardTile,
                    discardPlayerIndex);
                // server side data update
                currentTurnPlayer.HandTilesCount -= message.Meld.EffectiveTileCount;
                GameStatus.PlayerHandTiles[currentPlayerIndex].Subtract(message.Meld.Tiles, message.DiscardTile);
                GameStatus.PlayerOpenMelds[currentPlayerIndex].Add(message.Meld);
                var defaultTile = GameStatus.PlayerHandTiles[currentPlayerIndex].RemoveLast();
                Assert.AreEqual(GameStatus.PlayerHandTiles[currentPlayerIndex].Count, currentTurnPlayer.HandTilesCount,
                    "Hand tile count should equal to data on server");
                stateMachine.ChangeState(new PlayerOpenMeldState
                {
                    OpenMeldData = new OpenMeldData
                        {DefaultTile = defaultTile, DiscardTile = message.DiscardTile, OpenMeld = message.Meld},
                    GameStatus = GameStatus,
                    ServerCallback = data =>
                    {
                        StopWaitingCoroutine();
                        ServerDiscardTile(data);
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
                currentTurnPlayer.RpcPerformChow(message.Meld, message.DiscardTile);
                // server side data update
                currentTurnPlayer.HandTilesCount -= message.Meld.EffectiveTileCount;
                GameStatus.PlayerHandTiles[currentPlayerIndex].Subtract(message.Meld.Tiles, message.DiscardTile);
                GameStatus.PlayerOpenMelds[currentPlayerIndex].Add(message.Meld);
                var defaultTile = GameStatus.PlayerHandTiles[currentPlayerIndex].RemoveLast();
                Assert.AreEqual(GameStatus.PlayerHandTiles[currentPlayerIndex].Count, currentTurnPlayer.HandTilesCount,
                    "Hand tile count should equal to data on server");
                stateMachine.ChangeState(new PlayerOpenMeldState
                {
                    OpenMeldData = new OpenMeldData
                        {DefaultTile = defaultTile, DiscardTile = message.DiscardTile, OpenMeld = message.Meld},
                    GameStatus = GameStatus,
                    ServerCallback = data =>
                    {
                        StopWaitingCoroutine();
                        ServerDiscardTile(data);
                    }
                });
                waitForClientCoroutine = StartCoroutine(ServerWaitForClientDiscard());
                return;
            }

            Debug.Log($"[Server] No one claims out turn operations, entering next player's turn");
            GameStatus.SetCurrentPlayerIndex(GameStatus.NextPlayerIndex());
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
        internal void RpcClientPrepare(GameSettings gameSettings, YakuSettings yakuSettings)
        {
            // assign settings
            GameSettings = gameSettings;
            YakuSettings = yakuSettings;
            PointSummaryPanelController.GameSettings = gameSettings;
            PointSummaryPanelController.YakuSettings = yakuSettings;
            // turn mahjong set to the initial player
            var localPlayer = LobbyManager.Instance.LocalPlayer;
            int localPlayerIndex = localPlayer.PlayerIndex;
            MahjongSelector.transform.localRotation = Quaternion.Euler(90, 0, -localPlayerIndex * 90);
            Debug.Log($"Local player index: {localPlayerIndex}");
            // assign UI Elements
            localPlayer.PlayerHandPanel = PlayerHandPanel;
            foreach (var player in LobbyManager.Instance.Players)
            {
                int index = player.PlayerIndex - localPlayerIndex;
                if (index < 0) index += RoundStatusPanel.PointPanels.Length;
                RoundStatusPanel.PointPanels[index].gameObject.SetActive(true);
                player.PlayerPointInfo = RoundStatusPanel.PointPanels[index];
            }
        }

        [ClientRpc]
        internal void RpcClientRoundStart(bool newRound, bool extraRound)
        {
            Debug.Log("ClientState_RoundStart is called");
            InGameCanvas.SetActive(true);
            InGameInfoText.Print($"Current player's index: {LobbyManager.Instance.LocalPlayer.PlayerIndex}");
            // todo -- ui elements (dealer display, points, etc)
        }

        [ClientRpc]
        internal void RpcClientRoundEnd(RoundEndData data)
        {
            StartCoroutine(ClientShowSummaryPanel(data));
        }

        [Server]
        private void StopWaitingCoroutine()
        {
            if (waitForClientCoroutine == null) return;
            StopCoroutine(waitForClientCoroutine);
            waitForClientCoroutine = null;
        }

        [Client]
        private IEnumerator ClientShowSummaryPanel(RoundEndData data)
        {
            // todo -- point transfer
            int panelCount = data.PlayerServerDataArray.Length;
            for (int i = 0; i < panelCount; i++)
            {
                var playerServerData = data.PlayerServerDataArray[i];
                if (i < panelCount - 1)
                    yield return StartCoroutine(
                        PointSummaryPanelController.ShowSummaryPanel(playerServerData));
                else
                    yield return StartCoroutine(
                        PointSummaryPanelController.ShowSummaryPanel(playerServerData,
                            () => { Debug.Log("All panel has been shown"); }));
            }
        }
    }
}