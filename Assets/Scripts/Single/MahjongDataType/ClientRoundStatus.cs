using System.Collections.Generic;
using System.Linq;
using Multi;
using Multi.ServerData;
using Single.MahjongDataType.Interfaces;
using UnityEngine;

namespace Single.MahjongDataType
{
    [System.Serializable]
    public class ClientRoundStatus : ISubject<ClientRoundStatus>
    {
        public int TotalPlayers { get; }
        public int[] Places { get; }
        public string[] PlayerNames { get; }
        public int[] TileCounts { get; }
        public int[] Points { get; }
        public bool[] RichiStatus { get; private set; }
        public List<Tile> LocalPlayerHandTiles { get; private set; }
        public Tile?[] LastDraws { get; private set; }
        public RiverData[] Rivers { get; private set; }
        public int OyaPlayerIndex { get; private set; }
        public int Field { get; private set; }
        public int Dice { get; private set; }
        public int Extra { get; private set; }
        public int RichiSticks { get; private set; }
        public bool IsRichiing { get; private set; }
        public bool IsZhenting { get; private set; }
        public MahjongSetData MahjongSetData { get; private set; }
        public Player LocalPlayer { get; }
        public IList<Tile> ForbiddenTiles { get; private set; }
        public GameSetting GameSetting { get; private set; }
        public YakuSetting YakuSetting { get; private set; }
        public IDictionary<Tile, IList<Tile>> PossibleWaitingTiles { get; private set; }
        public IList<Tile> WaitingTiles { get; private set; }
        public ClientLocalSettings LocalSettings { get; private set; }
        public int LocalPlayerIndex => LocalPlayer.PlayerIndex;

        public ClientRoundStatus(int totalPlayers, int playerIndex, string gameSetting, string yakuSetting)
        {
            TotalPlayers = totalPlayers;
            // Settings = settings;
            Places = new int[4];
            PlayerNames = new string[4];
            TileCounts = new int[4];
            Points = new int[4];
            RichiStatus = new bool[4];
            LastDraws = new Tile?[4];
            Rivers = new RiverData[4];
            LocalPlayerHandTiles = new List<Tile>();
            LocalPlayer = Lobby.LobbyManager.Instance.LocalPlayer;
            LocalSettings = new ClientLocalSettings();
            AssignSettings(gameSetting, yakuSetting);
            AssignPlaces(playerIndex);
        }

        public void NewRound(int oyaPlayerIndex, int dice, int field, int extra, int richiSticks)
        {
            OyaPlayerIndex = oyaPlayerIndex;
            Dice = dice;
            Field = field;
            Extra = extra;
            RichiSticks = richiSticks;
            RichiStatus = new bool[4];
            LastDraws = new Tile?[4];
            Rivers = new RiverData[4];
            WaitingTiles = null;
            PossibleWaitingTiles = null;
            LocalSettings.Reset();
            NotifyObservers();
        }

        public void UpdatePoints(int[] points)
        {
            for (int placeIndex = 0; placeIndex < Points.Length; placeIndex++)
            {
                int playerIndex = GetPlayerIndex(placeIndex);
                if (playerIndex < points.Length)
                    Points[placeIndex] = points[playerIndex];
            }
            NotifyObservers();
        }

        public void UpdateNames(string[] names)
        {
            for (int placeIndex = 0; placeIndex < PlayerNames.Length; placeIndex++)
            {
                int playerIndex = GetPlayerIndex(placeIndex);
                if (playerIndex < names.Length)
                    PlayerNames[placeIndex] = names[playerIndex];
                else
                    PlayerNames[placeIndex] = null;
            }
            NotifyObservers();
        }

        public void UpdateRichiStatus(bool[] status)
        {
            for (int placeIndex = 0; placeIndex < RichiStatus.Length; placeIndex++)
            {
                int playerIndex = GetPlayerIndex(placeIndex);
                if (playerIndex < status.Length)
                    RichiStatus[placeIndex] = status[playerIndex];
                else
                    RichiStatus[placeIndex] = false;
            }
            NotifyObservers();
        }

        public void SetHandTiles(IList<Tile> handTiles)
        {
            LocalPlayerHandTiles = new List<Tile>(handTiles);
            SetHandTiles(0, handTiles.Count);
            NotifyObservers();
        }

        public void SetHandTiles(int placeIndex, int count)
        {
            TileCounts[placeIndex] = count;
            NotifyObservers();
        }

        public void SetLastDraw(int placeIndex, Tile lastDraw = default(Tile))
        {
            for (int i = 0; i < LastDraws.Length; i++)
            {
                if (i == placeIndex) LastDraws[i] = lastDraw;
                else LastDraws[i] = null;
            }
            NotifyObservers();
        }

        public void ClearLastDraws()
        {
            if (LastDraws == null) return;
            for (int i = 0; i < LastDraws.Length; i++)
                LastDraws[i] = null;
            NotifyObservers();
        }

        public void SetRiverData(int placeIndex, RiverData data)
        {
            Rivers[placeIndex] = data;
            NotifyObservers();
        }

        public void SetRichiing(bool isRichiing)
        {
            IsRichiing = isRichiing;
            NotifyObservers();
        }

        public void SetZhenting(bool zhenting)
        {
            IsZhenting = zhenting;
            NotifyObservers();
        }

        public void SetRichiSticks(int sticks)
        {
            RichiSticks = sticks;
            NotifyObservers();
        }

        public void SetMahjongSetData(MahjongSetData data)
        {
            MahjongSetData = data;
            NotifyObservers();
        }

        public void SetForbiddenTiles(IList<Tile> forbiddenTiles)
        {
            ForbiddenTiles = forbiddenTiles;
            NotifyObservers();
        }

        public void CalculatePossibleWaitingTiles()
        {
            if (LocalPlayerHandTiles == null) return;
            if (!GameSetting.AllowHint)
            {
                PossibleWaitingTiles = null;
                return;
            }
            var handTiles = new List<Tile>(LocalPlayerHandTiles);
            var lastDraw = GetLastDraw(0);
            if (lastDraw == null)
            {
                PossibleWaitingTiles = null;
                NotifyObservers();
                return;
            }
            PossibleWaitingTiles = MahjongLogic.DiscardForReady(LocalPlayerHandTiles, (Tile)lastDraw);
            if (PossibleWaitingTiles == null)
                Debug.Log("WaitingTiles: null");
            else
                Debug.Log($"WaitingTiles: {string.Join(";", PossibleWaitingTiles.Select(x => x.Key + ": " + string.Join(",", x.Value)))}");
            NotifyObservers();
        }

        public void ClearPossibleWaitingTiles()
        {
            PossibleWaitingTiles = null;
            NotifyObservers();
        }

        public void CalculateWaitingTiles()
        {
            if (LocalPlayerHandTiles == null) return;
            if (!GameSetting.AllowHint)
            {
                WaitingTiles = null;
                return;
            }
            WaitingTiles = MahjongLogic.WinningTiles(LocalPlayerHandTiles, null);
            NotifyObservers();
        }

        public void ClearWaitingTiles()
        {
            WaitingTiles = null;
            NotifyObservers();
        }

        public int GetPlaceIndex(int playerIndex)
        {
            return System.Array.FindIndex(Places, i => i == playerIndex);
        }

        public int GetPlayerIndex(int placeIndex)
        {
            if (Places == null) return -1;
            return Places[placeIndex];
        }

        public int GetTileCount(int placeIndex)
        {
            if (TileCounts == null) return 0;
            return TileCounts[placeIndex];
        }

        public string GetPlayerName(int placeIndex)
        {
            if (PlayerNames == null) return "";
            return PlayerNames[placeIndex];
        }

        public Tile? GetLastDraw(int placeIndex)
        {
            if (LastDraws == null) return null;
            return LastDraws[placeIndex];
        }

        public RiverTile[] GetRiverTiles(int placeIndex)
        {
            if (Rivers == null) return null;
            return Rivers[placeIndex].River;
        }

        private void AssignSettings(string gameSetting, string yakuSetting)
        {
            GameSetting = JsonUtility.FromJson<GameSetting>(gameSetting);
            YakuSetting = JsonUtility.FromJson<YakuSetting>(yakuSetting);
        }

        private void AssignPlaces(int playerIndex)
        {
            Places[0] = playerIndex;
            for (int i = 1; i < Places.Length; i++)
            {
                var next = Places[0] + i;
                if (next >= Places.Length) next -= Places.Length;
                Places[i] = next;
            }
        }

        private IList<IObserver<ClientRoundStatus>> observers = new List<IObserver<ClientRoundStatus>>();

        public void AddObserver(IObserver<ClientRoundStatus> observer)
        {
            if (observer == null)
            {
                Debug.Log("Parameter [observer] cannot be null");
                return;
            }
            observers.Add(observer);
        }

        public void RemoveObserver(IObserver<ClientRoundStatus> observer)
        {
            observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.UpdateStatus(this);
            }
        }
    }
}