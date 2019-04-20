using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lobby;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using Single.UI;
using UI;
using UnityEngine;
using Debug = Single.Debug;

namespace Multi
{
    public class ClientBehaviour : MonoBehaviour
    {
        public static ClientBehaviour Instance { get; private set; }

        [Header("Game Managers")]
        // public RoundInfoManager RoundInfoManager;
        // public PointsManager PointsManager;
        // public PositionManager PositionManager;
        // public RichiStatusManager RichiStatusManager;
        public BoardInfoManager BoardInfoManager;
        public YamaManager YamaManager;
        public PlayerHandManager[] HandManagers;
        public PlayerRiverManager[] RiverManagers;
        public PlayerInfoManager PlayerInfoManager;
        public HandPanelManager HandPanelManager;
        public TimerController TurnTimeController;
        public OutTurnPanelManager OutTurnPanelManager;
        public RoundDrawPanelManager[] DrawPanelManagers;

        [Header("Settings")]
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;

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
        }

        private void UpdateYamaManager()
        {
            YamaManager.Places = Places;
            YamaManager.OyaPlayerIndex = OyaPlayerIndex;
            YamaManager.Dice = Dice;
            YamaManager.MahjongSetData = MahjongSetData;
            YamaManager.GameSettings = GameSettings;
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
            GameSettings = message.GameSettings;
            YakuSettings = message.YakuSettings;
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
        }

        public void PlayerDrawTurn(ServerDrawTileMessage message)
        {
            var index = message.PlayerIndex;
            var tile = message.Tile;
            for (int i = 0; i < LastDraws.Length; i++)
            {
                LastDraws[i] = null;
            }
            LastDraws[0] = tile;
            BonusTurnTime = message.BonusTurnTime;
            MahjongSetData = message.MahjongSetData;
            // todo -- UI events
            TurnTimeController.StartCountDown(GameSettings.BaseTurnTime, BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically discarding last drawn tile");
                LocalPlayer.DiscardTile(tile, false, true, 0);
            });
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

        // todo -- if player is richiing, show richi related animation.
        public void PlayerOutTurnOperation(ServerDiscardOperationMessage message)
        {
            Debug.Log($"Player {message.PlayerIndex} just discarded {message.Tile}, valid operation: {string.Join(", ", message.Operations)}");
            // update hand tiles
            LocalPlayerHandTiles = new List<Tile>(message.HandTiles);
            StartCoroutine(UpdateHandData(message.CurrentTurnPlayerIndex, message.DiscardingLastDraw, message.Tile, message.Rivers));
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
            TurnTimeController.StartCountDown(GameSettings.BaseTurnTime, message.BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically skip this turn");
                LocalPlayer.SkipOutTurnOperation(0);
                OutTurnPanelManager.Disable();
            });
        }

        private IEnumerator UpdateHandData(int currentIndex, bool discardingLastDraw, Tile tile, RiverData[] rivers)
        {
            for (int i = 0; i < LastDraws.Length; i++) LastDraws[i] = null;
            int currentPlaceIndex = GetPlaceIndexByPlayerIndex(currentIndex);
            HandManagers[currentPlaceIndex].DiscardTile(discardingLastDraw);
            Debug.Log($"Playing player {currentIndex} (place: {currentPlaceIndex}) discarding animation");
            yield return new WaitForEndOfFrame();
            for (int playerIndex = 0; playerIndex < rivers.Length; playerIndex++)
            {
                int placeIndex = GetPlaceIndexByPlayerIndex(playerIndex);
                Rivers[placeIndex] = rivers[playerIndex];
            }
        }

        public void PlayerTurnEnd(ServerTurnEndMessage message)
        {
            // show operation related animation, etc.
            Debug.Log($"Turn ends, operation {message.Operations} is taking.");
            // todo -- do some cleaning, etc
        }

        public void RoundDraw(ServerRoundDrawMessage message)
        {
            // checking
            if (!LastDraws.All(l => l == null))
                Debug.LogError("Someone still holding a lastDraw, this should not happen!");
            // hide hand panel
            HideHandPanel();
            // reveal hand tiles on table
            Debug.Log("Revealing hand tiles");
            var waitingDataArray = message.WaitingData;
            for (int playerIndex = 0; playerIndex < waitingDataArray.Length; playerIndex++)
            {
                int placeIndex = GetPlaceIndexByPlayerIndex(playerIndex);
                CheckReadyOrNot(placeIndex, waitingDataArray[playerIndex]);
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
                HandManagers[placeIndex].Reveal();
                HandManagers[placeIndex].Tiles = data.HandTiles.ToList();
                DrawPanelManagers[placeIndex].Ready(data.WaitingTiles);
            }
        }

        public void OnDiscardTile(Tile tile, bool isLastDraw)
        {
            Debug.Log($"Sending request of discarding tile {tile}");
            int bonusTimeLeft = TurnTimeController.StopCountDown();
            LocalPlayer.DiscardTile(tile, IsRichiing, isLastDraw, bonusTimeLeft);
        }

        public void OnSkipButtonClicked()
        {
            int bonusTimeLeft = TurnTimeController.StopCountDown();
            Debug.Log($"Sending request of skipping out turn operation with bonus turn time {bonusTimeLeft}");
            LocalPlayer.SkipOutTurnOperation(bonusTimeLeft);
            OutTurnPanelManager.Disable();
        }

        public void OnRongButtonClicked(OutTurnOperation operation)
        {
            if (operation.Type != OutTurnOperationType.Rong)
            {
                Debug.LogError($"Cannot send a operation with type {operation.Type} within OnRongButtonClicked method");
                return;
            }
            int bonusTimeLeft = TurnTimeController.StopCountDown();
            Debug.Log($"Sending request of rong operation with bonus turn time {bonusTimeLeft}");
            LocalPlayer.OperationTaken(operation, bonusTimeLeft);
            OutTurnPanelManager.Disable();
        }

        public void OnKongButtonClicked(OutTurnOperation operation)
        {
            if (operation.Type != OutTurnOperationType.Kong)
            {
                Debug.LogError($"Cannot send a operation with type {operation.Type} within OnKongButtonClicked method");
                return;
            }
            int bonusTimeLeft = TurnTimeController.StopCountDown();
            Debug.Log($"Sending request of kong operation with bonus turn time {bonusTimeLeft}");
            LocalPlayer.OperationTaken(operation, bonusTimeLeft);
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
                LocalPlayer.OperationTaken(operationOptions[0], bonusTimeLeft);
                OutTurnPanelManager.Disable();
                return;
            }
            // todo -- chow selection logic here
        }

        public void OnPongButtonClicked(OutTurnOperation[] operationOptions, OutTurnOperation[] originalOperations) {
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
            if (operationOptions.Length == 1) {
                int bonusTimeLeft = TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of kong operation with bonus turn time {bonusTimeLeft}");
                LocalPlayer.OperationTaken(operationOptions[0], bonusTimeLeft);
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

        private void HideHandPanel()
        {
            HandPanelManager.gameObject.SetActive(false);
        }

        private void ShowHandPanel()
        {
            HandPanelManager.gameObject.SetActive(true);
        }

        private int GetPlaceIndexByPlayerIndex(int playerIndex)
        {
            return System.Array.FindIndex(Places, i => i == playerIndex);
        }
    }
}