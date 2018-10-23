using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lobby;
using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utils;

namespace Multi
{
    public class Player : NetworkBehaviour
    {
        [Header("UI Elements")] public PlayerHandPanel PlayerHandPanel;

        [Header("Game Status")] [SyncVar] public int TotalPlayers;
        [SyncVar] public RoundStatus RoundStatus;

        [SyncVar]
        public int PlayerIndex = -1; // Round order index -- 0: East, 1: South, 2: West, 3: North (This does not change)

        [SyncVar] public int BonusTurnTime;

        [Header("Player Public Data")] [SyncVar]
        public string PlayerName = "";

        [SyncVar(hook = nameof(OnPoints))] public int Points;
        [SyncVar] public bool Richi = false;
        [SyncVar] public bool WRichi = false;
        [SyncVar] public bool OneShot = false;
        [SyncVar] public bool FirstTurn = false;
        [SyncVar] public int HandTilesCount = 0;
        [SerializeField] internal List<Meld> OpenMelds;

        [Header("Player Private Data")] [SerializeField]
        internal List<Tile> HandTiles;

        public Tile LastDraw;

        private bool isRichiing = false;
        private bool discardMessageSent = false;
        private Coroutine outTurnOperationWaiting;

        public override void OnStartClient()
        {
            Debug.Log($"Player [{netId}] [name: {PlayerName}] OnStartClient is called");
            LobbyManager.Instance.AddPlayer(this);
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log($"Player [{netId}] [name: {PlayerName}] OnStartLocalPlayer is called");
            LobbyManager.Instance.LocalPlayer = this;
            RegisterHandlers();
        }

        public override void OnNetworkDestroy()
        {
            LobbyManager.Instance.RemovePlayer(this);
        }

        [Client]
        private void RegisterHandlers()
        {
            LobbyManager.Instance.client.RegisterHandler(MessageConstants.DrawTileMessageId, OnDrawTileMessageReceived);
            LobbyManager.Instance.client.RegisterHandler(MessageConstants.InitialDrawingMessageId,
                OnInitialDrawingMessageReceived);
            LobbyManager.Instance.client.RegisterHandler(MessageConstants.DiscardAfterOpenMessageId,
                OnDiscardAfterOpenMessageReceived);
        }

        [Client]
        private void OnInitialDrawingMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<InitialDrawingMessage>();
            HandTiles = new List<Tile>(content.Tiles);
            // todo -- throwing dice visual effect
            MahjongManager.Instance.InGameInfoText.Print(
                $"Dice rolls to {content.Dice} with open index of {content.MountainOpenIndex}. Total players: {content.TotalPlayers}");
            ClientTurnDoraTiles(content.DoraIndicators, content.DoraIndicatorIndices);
            ClientDrawInitialTiles(content.MountainOpenIndex, content.TotalPlayers);
        }

        private void OnPoints(int amount)
        {
            Debug.Log("Player OnPoints is called");
            Points = amount;
            // todo -- change ui text
        }

        [ClientRpc]
        internal void RpcYourTurnToDraw(int nextIndex)
        {
            ClientYourTurnToDraw(nextIndex);
        }

        [ClientRpc]
        internal void RpcDiscardTile(Tile tile, bool discardLastDraw, InTurnOperation operation)
        {
            ClientYourTurnToDiscard(tile, discardLastDraw, operation);
        }

        [Client]
        private void ClientYourTurnToDiscard(Tile tile, bool discardLastDraw, InTurnOperation operation)
        {
            // todo -- add effect for richi
            Debug.Log($"Player {PlayerIndex}'s turn, tile {tile} is discarded. DiscardLastDraw: {discardLastDraw}");
            MahjongManager.Instance.MahjongSelector.DiscardTile(tile, discardLastDraw, PlayerIndex,
                operation.HasFlag(InTurnOperation.Richi));
            StartCoroutine(ClientUpdateTilesForSeconds(MahjongManager.Instance.GameSettings.PlayerHandTilesSortDelay));
            if (!isLocalPlayer) return;
            if (discardLastDraw)
            {
                PlayerHandPanel.DiscardTile();
            }
            else
            {
                int index = HandTiles.FindIndex(tile, Tile.TileConsiderColorEqualityComparer);
                Assert.IsTrue(index >= 0, "There must be this tile");
                HandTiles.RemoveAt(index);
                HandTiles.Add(LastDraw);
                HandTiles.Sort();
                PlayerHandPanel.DiscardTile(index);
                ClientUpdateTiles();
            }

            DisableInTurnPanel();
        }

        [ClientRpc]
        internal void RpcOutTurnOperation(Tile discardTile, int currentPlayerIndex)
        {
            ClientHandleOutTurnOperation(discardTile, currentPlayerIndex);
        }

        [Client]
        private void ClientHandleOutTurnOperation(Tile discardTile, int currentPlayerIndex, bool lingshang = false)
        {
            // todo -- more game setting variations
            if (!isLocalPlayer) return;
            var can = PlayerIndex == MahjongConstants.RepeatIndex(currentPlayerIndex + 1, TotalPlayers);
            var chows = can ? MahjongLogic.GetChows(HandTiles, discardTile) : new HashSet<Meld>();
            var pongs = MahjongLogic.GetPongs(HandTiles, discardTile);
            var kongs = MahjongLogic.GetOutTurnKongs(HandTiles, discardTile);
            var handStatus = GetCurrentHandStatus(lingshang);
            var roundStatus = GetCurrentRoundState();
            var pointInfo = MahjongLogic.GetPointInfo(HandTiles, OpenMelds, discardTile, handStatus, roundStatus,
                MahjongManager.Instance.YakuSettings);
            Debug.Log($"Client is handling out turn operation with tile {discardTile}, chows: {chows.Count}, "
                      + $"pongs: {pongs.Count}, kongs: {kongs.Count}, point info: {pointInfo}");
            // after richi, only response to RONG
            if (Richi)
            {
                chows.Clear();
                pongs.Clear();
                kongs.Clear();
            }

            // dora will be counted when this round ends
            if (chows.Count == 0 && pongs.Count == 0 && kongs.Count == 0 && pointInfo.BasePoint == 0)
            {
                SendOutTurnOperationMessage(new OutTurnOperationMessage
                {
                    PlayerIndex = PlayerIndex,
                    Operation = OutTurnOperation.Skip,
                    BonusTurnTime = BonusTurnTime
                });
                return;
            }

            EnableOutTurnPanel(chows, pongs, kongs, pointInfo.BasePoint > 0, discardTile);
            // wait for operation or time expires
            MahjongManager.Instance.TimerController.StartCountDown(MahjongManager.Instance.GameSettings.BaseTurnTime,
                BonusTurnTime, () =>
                {
                    DisableOutTurnPanel();
                    SendOutTurnOperationMessage(new OutTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = OutTurnOperation.Skip,
                        BonusTurnTime = 0
                    });
                });
        }

        [Client]
        private HandStatus GetCurrentHandStatus(bool lingshang = false)
        {
            var handStatus = HandStatus.Nothing;
            if (MahjongLogic.TestMenqing(OpenMelds)) handStatus |= HandStatus.Menqing;
            if (FirstTurn) handStatus |= HandStatus.FirstTurn;
            if (RoundStatus.TilesLeft == MahjongManager.Instance.GameSettings.MountainReservedTiles)
                handStatus |= HandStatus.LastDraw;
            if (Richi) handStatus |= HandStatus.Richi;
            if (WRichi) handStatus |= HandStatus.WRichi;
            if (OneShot) handStatus |= HandStatus.OneShot;
            if (lingshang) handStatus |= HandStatus.Lingshang;
            return handStatus;
        }

        [Client]
        private RoundStatus GetCurrentRoundState()
        {
            Debug.Log($"GetCurrentRoundState: RoundStatus: {RoundStatus}");
            return new RoundStatus
            {
                TotalPlayer = TotalPlayers,
                PlayerIndex = PlayerIndex,
                RoundCount = RoundStatus.RoundCount,
                FieldCount = RoundStatus.FieldCount,
                CurrentExtraRound = RoundStatus.CurrentExtraRound
            };
        }

        [Client]
        internal void SendOutTurnOperationMessage(OutTurnOperationMessage message)
        {
            connectionToServer.Send(MessageConstants.OutTurnOperationMessageId, message);
        }

        [Client]
        private void ClientDrawInitialTiles(int openIndex, int totalPlayers)
        {
            Debug.Log($"Player {PlayerIndex} ClientDrawInitialTiles is called");
            ClearHandTiles();
            StartCoroutine(MahjongManager.Instance.MahjongSelector.DrawInitialCoroutine(this, openIndex, totalPlayers));
        }

        [Client] // todo -- maybe need to move this method to MahjongSelector
        private void ClientTurnDoraTiles(Tile[] tiles, int[] indices)
        {
            if (!isLocalPlayer) return;
            Debug.Log($"Player {PlayerIndex} ClientTurnDoraTiles is called");
            Assert.AreEqual(tiles.Length, indices.Length, "Something is very wrong in method ClientTurnDoraTiles");
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            for (int i = 0; i < tiles.Length; i++)
            {
                mahjongSelector.RevealTileAt(indices[i], tiles[i]);
            }
        }

        [Client]
        internal void ClientYourTurnToDraw(int nextIndex)
        {
            Debug.Log("Draw a tile from the mahjong set.");
            discardMessageSent = false;
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.DrawToPlayer(nextIndex, PlayerIndex);
        }

        [Client]
        private void OnDrawTileMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<DrawTileMessage>();
            if (PlayerIndex != content.PlayerIndex)
            {
                Debug.LogError(
                    $"Player {PlayerIndex} should not receive a message sending to player {content.PlayerIndex}");
                return;
            }

            var lastDraw = content.Tile;
            Debug.Log($"Tile {lastDraw} is drawn");
            LastDraw = lastDraw;
            // handle tsumo
            var handStatus = GetCurrentHandStatus(content.Lingshang) | HandStatus.Tsumo;
            var roundStatus = GetCurrentRoundState();
            var pointInfo = MahjongLogic.GetPointInfo(HandTiles, OpenMelds, lastDraw, handStatus, roundStatus,
                MahjongManager.Instance.YakuSettings);
            // handle kongs
            var kongs = MahjongLogic.GetInTurnKongs(HandTiles, OpenMelds, lastDraw);
            var richiKongs = MahjongLogic.GetRichiKongs(HandTiles, OpenMelds, lastDraw);
            // handle auto discard after richi
            if (Richi && pointInfo.BasePoint == 0 && richiKongs.Count == 0)
            {
                StartCoroutine(RichiAutoDiscard(lastDraw));
                return;
            }

            isRichiing = false;
            var menqing = MahjongLogic.TestMenqing(OpenMelds);
            EnableInTurnPanel(menqing, kongs, richiKongs, pointInfo, lastDraw);
            PlayerHandPanel.Refresh(this, HandTiles, Richi);
            PlayerHandPanel.DrawTile(this, lastDraw);

            MahjongManager.Instance.TimerController.StartCountDown(MahjongManager.Instance.GameSettings.BaseTurnTime,
                BonusTurnTime, () =>
                {
                    connectionToServer.Send(MessageConstants.DiscardTileMessageId, new DiscardTileMessage
                    {
                        DiscardTile = lastDraw,
                        Operation = InTurnOperation.Discard,
                        PlayerIndex = PlayerIndex,
                        DiscardLastDraw = true
                    });
                });
        }

        [Client]
        private IEnumerator RichiAutoDiscard(Tile tile)
        {
            yield return new WaitForSeconds(MahjongManager.Instance.GameSettings.AutoDiscardDelayAfterRichi);
            connectionToServer.Send(MessageConstants.DiscardTileMessageId, new DiscardTileMessage
            {
                DiscardTile = tile,
                Operation = InTurnOperation.Discard,
                PlayerIndex = PlayerIndex,
                DiscardLastDraw = true
            });
        }

        [Client]
        internal void ClientAddTiles(List<Tile> tiles)
        {
            PlayerHandPanel.AddTiles(this, tiles);
        }

        [Client]
        private void ClearHandTiles()
        {
            PlayerHandPanel.Clear();
        }

        [Client]
        internal void ClientUpdateTiles()
        {
            if (!isLocalPlayer)
            {
                Debug.LogError("This method can only be called on local player!");
                return;
            }

            PlayerHandPanel.Refresh(this, HandTiles);
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.Refresh(HandTiles, PlayerIndex);
        }

        [Client]
        private IEnumerator ClientUpdateTilesForSeconds(float delay)
        {
            yield return new WaitForSeconds(delay);
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.Refresh(HandTilesCount, PlayerIndex);
        }

        [Client]
        internal void InitialDrawComplete(bool complete)
        {
            Debug.Log($"Player {PlayerIndex} drawing complete, sending message!");
            connectionToServer.Send(MessageConstants.ReadinessMessageId, new ReadinessMessage
            {
                PlayerIndex = PlayerIndex
            });
        }

        [Client]
        internal void ClientDiscardTile(Tile tile, bool discardLastDraw)
        {
            if (discardMessageSent) return;
            discardMessageSent = true;
            Debug.Log($"Client attempts to discard tile {tile}");
            var operation = InTurnOperation.Discard;
            if (isRichiing) operation |= InTurnOperation.Richi;
            int bonusTimeLeft = MahjongManager.Instance.TimerController.StopCountDown();
            connectionToServer.Send(MessageConstants.DiscardTileMessageId, new DiscardTileMessage
            {
                DiscardTile = tile,
                Operation = operation,
                PlayerIndex = PlayerIndex,
                DiscardLastDraw = discardLastDraw,
                BonusTurnTime = bonusTimeLeft
            });
        }

        [Client]
        private void EnableInTurnPanel(bool menqing, ISet<Meld> kongs, ISet<Meld> richiKongs, PointInfo pointInfo,
            Tile lastDraw)
        {
            if (!menqing && pointInfo.BasePoint == 0 && kongs.Count == 0) return;
            if (menqing && !Richi)
            {
                MahjongManager.Instance.RichiButton.gameObject.SetActive(true);
                ReplaceListener(MahjongManager.Instance.RichiButton, () => { isRichiing = !isRichiing; });
            }

            if (pointInfo.BasePoint > 0)
            {
                MahjongManager.Instance.TsumoButton.gameObject.SetActive(true);
                ReplaceListener(MahjongManager.Instance.TsumoButton, () =>
                {
                    DisableInTurnPanel();
                    int bonusTime = MahjongManager.Instance.TimerController.StopCountDown();
                    connectionToServer.Send(MessageConstants.InTurnOperationMessageId, new InTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = InTurnOperation.Tsumo,
                        LastDraw = lastDraw,
                        BonusTurnTime = bonusTime,
                        PointInfo = pointInfo
                    });
                });
            }

            HandleInTurnKongs(MahjongManager.Instance.InTurnKongButton, Richi ? richiKongs : kongs, lastDraw);

            MahjongManager.Instance.InTurnOperationPanel.SetActive(true);
        }

        [Client]
        private void HandleInTurnKongs(Button button, ISet<Meld> kongs, Tile lastDraw)
        {
            button.gameObject.SetActive(kongs.Count > 0);
            if (kongs.Count == 0) return;
            ReplaceListener(button, () =>
            {
                if (kongs.Count == 1)
                {
                    DisableInTurnPanel();
                    int bonusTime = MahjongManager.Instance.TimerController.StopCountDown();
                    connectionToServer.Send(MessageConstants.InTurnOperationMessageId, new InTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = InTurnOperation.Kong,
                        LastDraw = lastDraw,
                        Meld = kongs.First(),
                        BonusTurnTime = bonusTime
                    });
                    return;
                }

                MahjongManager.Instance.MeldSelector.gameObject.SetActive(true);
                MahjongManager.Instance.MeldSelector.ResetMelds();
                MahjongManager.Instance.MeldSelector.AddMelds(kongs, meld =>
                {
                    DisableInTurnPanel();
                    int bonusTime = MahjongManager.Instance.TimerController.StopCountDown();
                    connectionToServer.Send(MessageConstants.InTurnOperationMessageId, new InTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = InTurnOperation.Kong,
                        LastDraw = lastDraw,
                        Meld = meld,
                        BonusTurnTime = bonusTime,
                    });
                });
            });
        }

        [Client]
        private void DisableInTurnPanel()
        {
            MahjongManager.Instance.InTurnOperationPanel.SetActive(false);
        }

        [Client]
        private void EnableOutTurnPanel(ISet<Meld> chows, ISet<Meld> pongs, ISet<Meld> kongs, bool rong,
            Tile discardTile)
        {
            MahjongManager.Instance.SkipButton.gameObject.SetActive(true);
            ReplaceListener(MahjongManager.Instance.SkipButton,
                () =>
                {
                    DisableOutTurnPanel();
                    int bonusTime = MahjongManager.Instance.TimerController.StopCountDown();
                    SendOutTurnOperationMessage(new OutTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = OutTurnOperation.Skip,
                        BonusTurnTime = bonusTime
                    });
                });
            if (rong)
            {
                MahjongManager.Instance.RongButton.gameObject.SetActive(true);
                ReplaceListener(MahjongManager.Instance.RongButton, () =>
                {
                    DisableOutTurnPanel();
                    int bonusTime = MahjongManager.Instance.TimerController.StopCountDown();
                    SendOutTurnOperationMessage(new OutTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = OutTurnOperation.Rong,
                        BonusTurnTime = bonusTime,
                        DiscardedTile = discardTile,
                        Meld = new Meld(true, discardTile)
                        // todo -- rong operation needs a pointInfo field
                    });
                });
            }

            HandleOpenMeldOperations(MahjongManager.Instance.ChowButton, chows, OutTurnOperation.Chow, discardTile);

            HandleOpenMeldOperations(MahjongManager.Instance.PongButton, pongs, OutTurnOperation.Pong, discardTile);

            HandleOpenMeldOperations(MahjongManager.Instance.OutTurnKongButton, kongs, OutTurnOperation.Kong,
                discardTile);

            MahjongManager.Instance.OutTurnOperationPanel.SetActive(true);
        }

        [Client]
        private void HandleOpenMeldOperations(Button button, ISet<Meld> melds, OutTurnOperation operation,
            Tile discardTile)
        {
            button.gameObject.SetActive(melds.Count > 0);
            if (melds.Count == 0) return;
            ReplaceListener(button, () =>
            {
                if (melds.Count == 1)
                {
                    DisableOutTurnPanel();
                    int bonusTime = MahjongManager.Instance.TimerController.StopCountDown();
                    SendOutTurnOperationMessage(new OutTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = operation,
                        BonusTurnTime = bonusTime,
                        DiscardedTile = discardTile,
                        Meld = melds.First()
                    });
                    return;
                }

                MahjongManager.Instance.MeldSelector.gameObject.SetActive(true);
                MahjongManager.Instance.MeldSelector.ResetMelds();
                MahjongManager.Instance.MeldSelector.AddMelds(melds, meld =>
                {
                    DisableOutTurnPanel();
                    int bonusTime = MahjongManager.Instance.TimerController.StopCountDown();
                    SendOutTurnOperationMessage(new OutTurnOperationMessage
                    {
                        PlayerIndex = PlayerIndex,
                        Operation = operation,
                        BonusTurnTime = bonusTime,
                        DiscardedTile = discardTile,
                        Meld = meld
                    });
                });
            });
        }

        [Client]
        private void DisableOutTurnPanel()
        {
            MahjongManager.Instance.OutTurnOperationPanel.SetActive(false);
            MahjongManager.Instance.MeldSelector.gameObject.SetActive(false);
        }

        [Client]
        private void ReplaceListener(Button button, UnityAction call)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(call);
        }

        [ClientRpc]
        internal void RpcPerformChow(Meld meld, Tile discardTile)
        {
            ClientPerformOpenMeld(meld, discardTile, MeldInstanceType.Left);
        }

        [ClientRpc]
        internal void RpcPerformPong(int playerIndex, Meld meld, Tile discardTile, int discardPlayerIndex)
        {
            ClientPerformOpenMeld(meld, discardTile,
                MahjongConstants.GetMeldDirection(playerIndex, discardPlayerIndex));
        }

        [ClientRpc]
        internal void RpcPerformKong(int playerIndex, Meld meld, Tile discardTile, int discardPlayerIndex)
        {
            ClientPerformKong(meld, discardTile,
                MahjongConstants.GetMeldDirection(playerIndex, discardPlayerIndex, true));
        }

        [ClientRpc]
        internal void RpcPerformInTurnKong(int playerIndex, Meld meld, Tile lastDraw)
        {
            ClientPerformInTurnKong(meld, lastDraw);
        }

        [Client]
        private void ClientPerformInTurnKong(Meld meld, Tile lastDraw)
        {
            // determine whether this is an added kong
            var added = meld.Revealed && meld.Tiles.Contains(lastDraw, Tile.TileConsiderColorEqualityComparer);
            // added kong
            if (added)
            {
                var openMeldIndex = OpenMelds.FindIndex(openMeld =>
                    openMeld.Type == meld.Type && openMeld.First.EqualsIgnoreColor(meld.First));
                Assert.IsTrue(openMeldIndex >= 0, "There must be such a meld to add");
                MahjongManager.Instance.MahjongSelector.AddedKongToPlayer(PlayerIndex, meld, lastDraw);
                MahjongManager.Instance.MahjongSelector.Refresh(HandTilesCount, PlayerIndex);
                OpenMelds[openMeldIndex] = meld;
            }
            else // concealed kong
            {
                MahjongManager.Instance.MahjongSelector.ConcealedKongToPlayer(PlayerIndex, meld);
                MahjongManager.Instance.MahjongSelector.Refresh(HandTilesCount, PlayerIndex);
                OpenMelds.Add(meld);
            }

            // handle local player
            if (!isLocalPlayer) return;
            DisableInTurnPanel();
            HandTiles.Subtract(meld.Tiles, lastDraw);
            MahjongManager.Instance.MahjongSelector.Refresh(HandTiles, PlayerIndex);
        }

        [Client]
        private void ClientPerformKong(Meld meld, Tile discardTile, MeldInstanceType direction)
        {
            Debug.Log(
                $"Player {PlayerIndex} is opening meld {meld} on discardTile {discardTile}");
            MahjongManager.Instance.MahjongSelector.OpenToPlayer(PlayerIndex, meld, discardTile, direction);
            MahjongManager.Instance.MahjongSelector.Refresh(HandTilesCount, PlayerIndex);
            OpenMelds.Add(meld);
            // handle local player
            if (!isLocalPlayer) return;
            DisableOutTurnPanel();
            Debug.Log($"Player's hand: {string.Join(", ", HandTiles)}");
            HandTiles.Subtract(meld.Tiles, discardTile);
            Debug.Log($"Player's hand after subtraction: {string.Join(", ", HandTiles)}");
            MahjongManager.Instance.MahjongSelector.Refresh(HandTiles, PlayerIndex);
        }

        [Client]
        private void ClientPerformOpenMeld(Meld meld, Tile discardTile, MeldInstanceType direction)
        {
            Debug.Log(
                $"Player {PlayerIndex} is opening meld {meld} on discardTile {discardTile}");
            MahjongManager.Instance.MahjongSelector.OpenToPlayer(PlayerIndex, meld, discardTile, direction);
            MahjongManager.Instance.MahjongSelector.Refresh(HandTilesCount, PlayerIndex);
            OpenMelds.Add(meld);
            // handle local player
            if (!isLocalPlayer) return;
            DisableOutTurnPanel();
            HandTiles.Subtract(meld.Tiles, discardTile);
            MahjongManager.Instance.MahjongSelector.Refresh(HandTiles, PlayerIndex);
        }

        [Client]
        private void OnDiscardAfterOpenMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<DiscardAfterOpenMessage>();
            if (content.PlayerIndex != PlayerIndex)
            {
                Debug.LogError(
                    $"Player {PlayerIndex} should not receive a message intended to player {content.PlayerIndex}");
            }

            discardMessageSent = false;
            var defaultTile = content.DefaultTile;
            Debug.Log($"Player {content.PlayerIndex} has received discard request, default is {defaultTile}");
            DisableInTurnPanel();
            HandTiles.Remove(defaultTile);
            LastDraw = defaultTile;
            PlayerHandPanel.Refresh(this, HandTiles);
            PlayerHandPanel.DrawTile(this, defaultTile);
            MahjongManager.Instance.TimerController.StartCountDown(MahjongManager.Instance.GameSettings.BaseTurnTime,
                BonusTurnTime, () =>
                {
                    connectionToServer.Send(MessageConstants.DiscardTileMessageId, new DiscardTileMessage
                    {
                        PlayerIndex = PlayerIndex,
                        DiscardTile = defaultTile,
                        Operation = InTurnOperation.Discard,
                        DiscardLastDraw = true
                    });
                });
        }
    }
}