using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lobby;
using Multi;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single.MahjongDataType;
using Single.UI;
using Single.UI.Controller;
using UnityEngine;


namespace Single
{
    public class ClientBehaviour : MonoBehaviour
    {
        public static ClientBehaviour Instance { get; private set; }

        [Header("Game Managers")]
        public BoardInfoManager BoardInfoManager;
        public YamaManager YamaManager;
        public PlayerHandManager[] HandManagers;
        public PlayerRiverManager[] RiverManagers;
        public PlayerInfoManager PlayerInfoManager;
        public HandPanelManager HandPanelManager;
        public TimerController TurnTimeController;
        public PlayerEffectManager PlayerEffectManager;
        public InTurnPanelManager InTurnPanelManager;
        public OutTurnPanelManager OutTurnPanelManager;
        public WaitingPanelManager[] DrawPanelManagers;
        public RoundDrawManager RoundDrawManager;
        public PointSummaryPanelManager PointSummaryPanelManager;
        public PointTransferManager PointTransferManager;
        public GameEndPanelManager GameEndPanelManager;

        [Header("Data")]
        public int TotalPlayers;
        public int[] Places = new int[4];
        public string[] PlayerNames = new string[4];
        public int[] TileCounts = new int[4];
        public int[] Points = new int[4];
        public bool[] RichiStatus = new bool[4];
        public bool IsRichiing = false;
        public int BonusTurnTime;
        public List<Tile> LocalPlayerHandTiles;
        public Tile?[] LastDraws = new Tile?[4];
        public RiverData[] Rivers = new RiverData[4];
        public Player LocalPlayer = null;
        public int OyaPlayerIndex;
        public int Field;
        public int Dice;
        public int Extra;
        public int RichiSticks;
        public MahjongSetData MahjongSetData;
        private WaitForSeconds waitAutoDiscardAfterRichi = new WaitForSeconds(MahjongConstants.AutoDiscardDelayAfterRichi);
        private NetworkSettings settings;

        private void OnEnable()
        {
            Debug.Log("ClientBehaviour.OnEnable() is called");
            Instance = this;
        }

        private void Update()
        {
            AssignData();
        }

        private void AssignData()
        {
            if (LocalPlayer == null) LocalPlayer = LobbyManager.Instance?.LocalPlayer;
            UpdateYamaManager();
            UpdateHandManager();
            UpdateRiverManager();
            UpdatePlayerInfoManager();
            UpdateBoardInfoManager();
            UpdateHandPanelManager();
            UpdatePointTransferManager();
        }

        private void UpdateYamaManager()
        {
            YamaManager.Places = Places;
            YamaManager.OyaPlayerIndex = OyaPlayerIndex;
            YamaManager.Dice = Dice;
            YamaManager.MahjongSetData = MahjongSetData;
            YamaManager.LingshangTilesCount = settings.LingShangTilesCount;
        }

        private void UpdateHandManager()
        {
            for (int i = 0; i < HandManagers.Length; i++)
            {
                var hand = HandManagers[i];
                // update hand tile count
                hand.Count = TileCounts[i];
                // update hand tiles if local
                if (i == 0) hand.Tiles = LocalPlayerHandTiles;
                // update last draw
                hand.LastDraw = LastDraws[i];
            }
        }

        private void UpdateRiverManager()
        {
            for (int i = 0; i < RiverManagers.Length; i++)
            {
                var manager = RiverManagers[i];
                manager.RiverTiles = Rivers[i].River;
            }
        }

        private void UpdatePlayerInfoManager()
        {
            PlayerInfoManager.TotalPlayers = TotalPlayers;
            PlayerInfoManager.Places = Places;
            PlayerInfoManager.Names = PlayerNames;
        }

        private void UpdateBoardInfoManager()
        {
            BoardInfoManager.UpdateRoundInfo(OyaPlayerIndex, Field, Extra, RichiSticks);
            BoardInfoManager.UpdatePoints(TotalPlayers, Places, Points);
            BoardInfoManager.UpdatePosition(TotalPlayers, OyaPlayerIndex, Places);
            BoardInfoManager.UpdateRichiStatus(TotalPlayers, Places, RichiStatus);
        }

        private void UpdateHandPanelManager()
        {
            HandPanelManager.Tiles = LocalPlayerHandTiles;
            HandPanelManager.LastDraw = LastDraws[0];
        }

        private void UpdatePointTransferManager()
        {
            PointTransferManager.TotalPlayers = TotalPlayers;
            PointTransferManager.Places = Places;
            PointTransferManager.PlayerNames = PlayerNames;
        }

        /// <summary>
        /// This method make the game preparation information takes effect 
        /// on the client. In this method, client sets the proper place to the 
        /// corresponding player, and gathering necessary information.
        /// </summary>
        /// <param name="message">The message received from server</param>
        public void GamePrepare(ServerGamePrepareMessage message)
        {
            TotalPlayers = message.TotalPlayers;
            Places[0] = message.PlayerIndex;
            settings = message.Settings;
            LocalPlayerHandTiles = null;
            MahjongSetData = default(MahjongSetData);
            for (int i = 1; i < Places.Length; i++)
            {
                var next = Places[0] + i;
                if (next >= Places.Length) next -= Places.Length;
                Places[i] = next;
            }
            // Sync points for every player
            UpdatePoints(message.Points);
            // Sync names for ui
            UpdateNames(message.PlayerNames);
        }

        /// <summary>
        /// This method is invoked when the client received a RoundStartMessage 
        /// when every round starts. In this method, client set initial hand tiles
        /// of the local player, and hand tiles count for every remote players.
        /// Dice is thrown on the server, as client receives the dice value then 
        /// applies it to the game. Same as other data (Extra, RichiSticks, etc).
        /// </summary>
        /// <param name="message">The message received from server</param>
        public void StartRound(ServerRoundStartMessage message)
        {
            // todo -- add animation here
            // data update
            var tiles = message.InitialHandTiles;
            for (int i = 0; i < TileCounts.Length; i++)
            {
                if (Places[i] >= TotalPlayers) continue;
                TileCounts[i] = tiles.Length;
            }
            LocalPlayerHandTiles = new List<Tile>(tiles);
            LastDraws = new Tile?[4];
            Rivers = new RiverData[4];
            OyaPlayerIndex = message.OyaPlayerIndex;
            Field = message.Field;
            Dice = message.Dice;
            Extra = message.Extra;
            RichiSticks = message.RichiSticks;
            MahjongSetData = message.MahjongSetData;
            Debug.Log(message.MahjongSetData);
            // Sync points for every player
            UpdatePoints(message.Points);
            // All player should not be in richi status
            RichiStatus = new bool[4];
            // ui elements update
            for (int i = 0; i < HandManagers.Length; i++)
            {
                HandManagers[i].StandUp();
            }
            HandPanelManager.gameObject.SetActive(true);
            PointTransferManager.Close();
        }

        public void PlayerDrawTurn(ServerDrawTileMessage message)
        {
            HandPanelManager.UnlockTiles();
            var index = message.PlayerIndex;
            var tile = message.Tile;
            for (int i = 0; i < LastDraws.Length; i++)
            {
                LastDraws[i] = null;
            }
            LastDraws[0] = tile;
            BonusTurnTime = message.BonusTurnTime;
            MahjongSetData = message.MahjongSetData;
            // auto discard when richied
            if (message.Richied && message.Operations.All(op => op.Type == InTurnOperationType.Discard))
            {
                HandPanelManager.LockTiles();
                StartCoroutine(AutoDiscard(message.Tile, message.BonusTurnTime));
                return;
            }
            // not richied, show timer and panels
            TurnTimeController.StartCountDown(settings.BaseTurnTime, BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically discarding last drawn tile");
                LocalPlayer.DiscardTile(tile, false, true, 0);
            });
            InTurnPanelManager.SetOperations(message.Operations);
        }

        public void OtherPlayerDrawTurn(ServerOtherDrawTileMessage message)
        {
            var index = message.CurrentTurnPlayerIndex;
            for (int i = 0; i < LastDraws.Length; i++)
            {
                LastDraws[i] = null;
            }
            var place = GetPlaceIndexByPlayerIndex(index);
            LastDraws[place] = default(Tile);
            MahjongSetData = message.MahjongSetData;
        }

        private IEnumerator AutoDiscard(Tile tile, int bonusTimeLeft)
        {
            yield return waitAutoDiscardAfterRichi;
            LocalPlayer.DiscardTile(tile, false, true, bonusTimeLeft);
        }

        public void PlayerOutTurnOperation(ServerDiscardOperationMessage message)
        {
            Debug.Log($"Player {message.PlayerIndex} just discarded {message.Tile}, valid operation: {string.Join(", ", message.Operations)}");
            InTurnPanelManager.Close();
            int currentPlaceIndex = GetPlaceIndexByPlayerIndex(message.CurrentTurnPlayerIndex);
            // update hand tiles
            LocalPlayerHandTiles = new List<Tile>(message.HandTiles);
            StartCoroutine(UpdateHandData(currentPlaceIndex, message.DiscardingLastDraw, message.Tile, message.Rivers));
            if (message.IsRichiing)
                StartCoroutine(PlayerEffectManager.ShowEffect(currentPlaceIndex, PlayerEffectManager.Type.Richi));
            BonusTurnTime = message.BonusTurnTime;
            // check if message contains a valid operation
            if (message.Operations == null || message.Operations.Length == 0)
            {
                Debug.LogError("Received with no operations, this should not happen");
                LocalPlayer.SkipOutTurnOperation(message.BonusTurnTime);
                OutTurnPanelManager.Disable();
                return;
            }
            // if all the operations are skip, automatically skip this turn.
            if (message.Operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                Debug.Log("Only operation is skip, skipping turn.");
                LocalPlayer.SkipOutTurnOperation(message.BonusTurnTime);
                OutTurnPanelManager.Disable();
                return;
            }
            OutTurnPanelManager.SetOperations(message.Operations);
            // if there are valid operations, assign operations
            TurnTimeController.StartCountDown(settings.BaseTurnTime, message.BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically skip this turn");
                LocalPlayer.SkipOutTurnOperation(0);
                OutTurnPanelManager.Disable();
            });
        }

        private IEnumerator UpdateHandData(int currentPlaceIndex, bool discardingLastDraw, Tile tile, RiverData[] rivers)
        {
            for (int i = 0; i < LastDraws.Length; i++) LastDraws[i] = null;
            // int currentPlaceIndex = GetPlaceIndexByPlayerIndex(currentIndex);
            HandManagers[currentPlaceIndex].DiscardTile(discardingLastDraw);
            Debug.Log($"Playing player (place: {currentPlaceIndex}) discarding animation");
            yield return new WaitForEndOfFrame();
            for (int playerIndex = 0; playerIndex < rivers.Length; playerIndex++)
            {
                int placeIndex = GetPlaceIndexByPlayerIndex(playerIndex);
                Rivers[placeIndex] = rivers[playerIndex];
            }
        }

        public void PlayerTurnEnd(ServerTurnEndMessage message)
        {
            // show operation related animation
            Debug.Log($"Turn ends, operation {message.Operations} is taking.");
            // update richi status
            for (int playerIndex = 0; playerIndex < message.RichiStatus.Length; playerIndex++)
            {
                int placeIndex = GetPlaceIndexByPlayerIndex(playerIndex);
                RichiStatus[placeIndex] = message.RichiStatus[playerIndex];
            }
            UpdatePoints(message.Points);
            RichiSticks = message.RichiSticks;
            // perform operation effect
            var operations = message.Operations;
            if (operations.All(op => op.Type == OutTurnOperationType.Skip)) return;
            for (int playerIndex = 0; playerIndex < operations.Length; playerIndex++)
            {
                int placeIndex = GetPlaceIndexByPlayerIndex(playerIndex);
                var operation = operations[playerIndex];
                var type = operation.Type;
                switch (type)
                {
                    case OutTurnOperationType.Skip:
                        continue;
                    case OutTurnOperationType.Chow:
                    case OutTurnOperationType.Pong:
                    case OutTurnOperationType.Kong:
                        Debug.LogError("This part is under construction");
                        break;
                    case OutTurnOperationType.Rong:
                        StartCoroutine(PlayerEffectManager.ShowEffect(placeIndex, PlayerEffectManager.GetAnimationType(type)));
                        StartCoroutine(RevealHandTiles(placeIndex, operation.HandData));
                        break;
                    case OutTurnOperationType.RoundDraw:
                        Debug.Log("Round is draw");
                        // RoundDrawManager.SetDrawType(RoundDrawType.RoundDraw);
                        break;
                }
            }
            // todo -- do some cleaning, etc
        }

        private IEnumerator RevealHandTiles(int placeIndex, PlayerHandData handData)
        {
            yield return new WaitForSeconds(MahjongConstants.HandTilesRevealDelay);
            var manager = HandManagers[placeIndex];
            manager.OpenUp();
            manager.Tiles = new List<Tile>(handData.HandTiles);
        }

        public IEnumerator PlayerTsumo(ServerPlayerTsumoMessage message)
        {
            // show tsumo animation
            int playerIndex = message.TsumoPlayerIndex;
            int placeIndex = GetPlaceIndexByPlayerIndex(playerIndex);
            // reveal hand tiles
            StartCoroutine(RevealHandTiles(placeIndex, message.TsumoHandData));
            yield return PlayerEffectManager.ShowEffect(placeIndex, PlayerEffectManager.Type.Tsumo);
            // show summary panel
            var data = new SummaryPanelData
            {
                HandInfo = new PlayerHandInfo
                {
                    HandTiles = message.TsumoHandData.HandTiles,
                    OpenMelds = message.TsumoHandData.OpenMelds,
                    WinningTile = message.WinningTile,
                    DoraIndicators = message.DoraIndicators,
                    UraDoraIndicators = message.UraDoraIndicators,
                    IsRichi = message.IsRichi,
                    IsTsumo = true
                },
                PointInfo = new PointInfo(message.TsumoPointInfo),
                TotalPoints = message.TotalPoints,
                PlayerName = message.TsumoPlayerName
            };
            PointSummaryPanelManager.ShowPanel(data, () =>
            {
                Debug.Log("Sending readiness message");
                LocalPlayer.ClientReady(MessageIds.ServerPointTransferMessage);
            });
        }

        public void PlayerRong(ServerPlayerRongMessage message)
        {
            var indices = message.RongPlayerIndices.Select((playerIndex, index) => index).ToArray();
            var dataArray = indices.Select(index => new SummaryPanelData
            {
                HandInfo = new PlayerHandInfo
                {
                    HandTiles = message.HandData[index].HandTiles,
                    OpenMelds = message.HandData[index].OpenMelds,
                    WinningTile = message.WinningTile,
                    DoraIndicators = message.DoraIndicators,
                    UraDoraIndicators = message.UraDoraIndicators,
                    IsRichi = message.RongPlayerRichiStatus[index],
                    IsTsumo = false
                },
                PointInfo = new PointInfo(message.RongPointInfos[index]),
                TotalPoints = message.TotalPoints[index],
                PlayerName = message.RongPlayerNames[index]
            });
            var dataQueue = new Queue<SummaryPanelData>(dataArray);
            ShowRongPanel(dataQueue);
        }

        private void ShowRongPanel(Queue<SummaryPanelData> queue)
        {
            if (queue.Count > 0)
            {
                // show panel for this data
                var data = queue.Dequeue();
                PointSummaryPanelManager.ShowPanel(data, () => ShowRongPanel(queue));
                // todo wait between two panels
            }
            else
            {
                // no more data to show
                Debug.Log("Sending readiness message");
                PointSummaryPanelManager.StopCountDown();
                LocalPlayer.ClientReady(MessageIds.ServerPointTransferMessage);
            }
        }

        public void RoundDraw(ServerRoundDrawMessage message)
        {
            RoundDrawManager.SetDrawType(message.RoundDrawType);
            if (message.RoundDrawType == RoundDrawType.RoundDraw)
            {
                Debug.Log("Revealing hand tiles");
                HandleRoundDraw(message.WaitingData);
            }
        }

        private void HandleRoundDraw(WaitingData[] data)
        {
            // var waitingDataArray = message.WaitingData;
            for (int playerIndex = 0; playerIndex < data.Length; playerIndex++)
            {
                int placeIndex = GetPlaceIndexByPlayerIndex(playerIndex);
                CheckReadyOrNot(placeIndex, data[playerIndex]);
            }
        }

        private void CheckReadyOrNot(int placeIndex, WaitingData data)
        {
            // Show tiles and corresponding panel
            if (data.WaitingTiles == null || data.WaitingTiles.Length == 0)
            {
                // no-ting
                HandManagers[placeIndex].CloseDown();
                DrawPanelManagers[placeIndex].NotReady();
            }
            else
            {
                // ting
                HandManagers[placeIndex].OpenUp();
                HandManagers[placeIndex].Tiles = data.HandTiles.ToList();
                DrawPanelManagers[placeIndex].Ready(data.WaitingTiles);
            }
        }

        public void PointTransfer(ServerPointTransferMessage message)
        {
            PointSummaryPanelManager.Close();
            var transfers = message.PointTransfers;
            PointTransferManager.SetTransfer(Points, transfers, () =>
            {
                LocalPlayer.RequestNewRound();
            });
            UpdatePoints(message.Points);
        }

        public void GameEnd(ServerGameEndMessage message)
        {
            GameEndPanelManager.SetPoints(message.PlayerNames, message.Points, message.Places, () =>
            {
                Debug.Log("Back to lobby");
            });
            // todo
        }

        public void OnDiscardTile(Tile tile, bool isLastDraw)
        {
            Debug.Log($"Sending request of discarding tile {tile}");
            int bonusTimeLeft = TurnTimeController.StopCountDown();
            LocalPlayer.DiscardTile(tile, IsRichiing, isLastDraw, bonusTimeLeft);
            InTurnPanelManager.Close();
            IsRichiing = false;
            HandPanelManager.RemoveCandidates();
        }

        public void OnInTurnSkipButtonClicked()
        {
            Debug.Log("In turn skip button clicked, hide buttons");
            InTurnPanelManager.Close();
        }

        public void OnTsumoButtonClicked(InTurnOperation operation)
        {
            if (operation.Type != InTurnOperationType.Tsumo)
            {
                Debug.LogError($"Cannot send a operation with type {operation.Type} within OnTsumoButtonClicked method");
                return;
            }
            int bonusTimeLeft = TurnTimeController.StopCountDown();
            Debug.Log($"Sending request of tsumo operation with bonus turn time {bonusTimeLeft}");
            LocalPlayer.InTurnOperationTaken(operation, bonusTimeLeft);
            InTurnPanelManager.Close();
        }

        public void OnRichiButtonClicked(InTurnOperation operation)
        {
            if (operation.Type != InTurnOperationType.Richi)
            {
                Debug.LogError($"Cannot send a operation with type {operation.Type} within OnRichiButtonClicked method");
                return;
            }
            // show richi selection panel
            Debug.Log($"Showing richi selection panel, candidates: {string.Join(",", operation.RichiAvailableTiles)}");
            IsRichiing = true;
            HandPanelManager.SetCandidates(operation.RichiAvailableTiles);
        }

        public void OnInTurnKongButtonClicked(InTurnOperation[] operationOptions)
        {
            if (operationOptions == null || operationOptions.Length == 0)
            {
                Debug.LogError("The operations are null or empty in OnInTurnKongButtonClicked method, this should not happen.");
                return;
            }
            if (!operationOptions.All(op => op.Type == InTurnOperationType.Kong))
            {
                Debug.LogError("There are incompatible type within OnInTurnKongButtonClicked method");
                return;
            }
            if (operationOptions.Length == 1)
            {
                int bonusTimeLeft = TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                LocalPlayer.InTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                InTurnPanelManager.Close();
                return;
            }
            // todo -- show kong selection panel here
        }

        public void OnInTurnBackButtonClicked(InTurnOperation[] operations)
        {
            InTurnPanelManager.SetOperations(operations);
            HandPanelManager.RemoveCandidates();
            IsRichiing = false;
        }

        public void OnInTurnDrawButtonClicked()
        {
            Debug.Log($"Requesting round draw due to 9 kinds of orphans");
            TurnTimeController.StopCountDown();
            LocalPlayer.NineKindsOfOrphans();
            InTurnPanelManager.Close();
        }

        public void OnOutTurnButtonClicked(OutTurnOperation operation)
        {
            int bonusTimeLeft = TurnTimeController.StopCountDown();
            Debug.Log($"Sending request of operation {operation} with bonus turn time {bonusTimeLeft}");
            LocalPlayer.OutTurnOperationTaken(operation, bonusTimeLeft);
            OutTurnPanelManager.Disable();
        }

        public void OnChowButtonClicked(OutTurnOperation[] operationOptions, OutTurnOperation[] originalOperations)
        {
            if (operationOptions == null || operationOptions.Length == 0)
            {
                Debug.LogError("The operations are null or empty in OnChowButtonClicked method, this should not happen.");
                return;
            }
            if (!operationOptions.All(op => op.Type == OutTurnOperationType.Chow))
            {
                Debug.LogError("There are incompatible type within OnChowButtonClicked method");
                return;
            }
            if (operationOptions.Length == 1)
            {
                int bonusTimeLeft = TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of chow operation with bonus turn time {bonusTimeLeft}");
                LocalPlayer.OutTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                OutTurnPanelManager.Disable();
                return;
            }
            // todo -- chow selection logic here
        }

        public void OnPongButtonClicked(OutTurnOperation[] operationOptions, OutTurnOperation[] originalOperations)
        {
            if (operationOptions == null || operationOptions.Length == 0)
            {
                Debug.LogError("The operations are null or empty in OnPongButtonClicked method, this should not happen.");
                return;
            }
            if (!operationOptions.All(op => op.Type == OutTurnOperationType.Chow))
            {
                Debug.LogError("There are incompatible type within OnPongButtonClicked method");
                return;
            }
            if (operationOptions.Length == 1)
            {
                int bonusTimeLeft = TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of kong operation with bonus turn time {bonusTimeLeft}");
                LocalPlayer.OutTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                OutTurnPanelManager.Disable();
                return;
            }
            // todo -- pong selection logic here
        }

        private void UpdatePoints(int[] points)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                if (Places[i] < points.Length)
                    Points[i] = points[Places[i]];
            }
        }

        private void UpdateNames(string[] names)
        {
            for (int i = 0; i < PlayerNames.Length; i++)
            {
                if (Places[i] < names.Length)
                    PlayerNames[i] = names[Places[i]];
                else
                    PlayerNames[i] = null;
            }
        }

        private int GetPlaceIndexByPlayerIndex(int playerIndex)
        {
            return System.Array.FindIndex(Places, i => i == playerIndex);
        }
    }
}