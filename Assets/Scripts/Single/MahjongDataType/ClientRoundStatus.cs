using System;
using System.Collections.Generic;
using Multi;
using Multi.ServerData;

namespace Single.MahjongDataType
{
    [Serializable]
    public class ClientRoundStatus
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
        public int RichiSticks { get; set; }
        public bool IsRichiing { get; set; }
        public MahjongSetData MahjongSetData { get; set; }
        public NetworkSettings Settings { get; }
        public Player LocalPlayer { get; }
        public int LocalPlayerIndex => LocalPlayer.PlayerIndex;

        public ClientRoundStatus(int totalPlayers, int playerIndex, NetworkSettings settings)
        {
            TotalPlayers = totalPlayers;
            Settings = settings;
            Places = new int[4];
            PlayerNames = new string[4];
            TileCounts = new int[4];
            Points = new int[4];
            RichiStatus = new bool[4];
            LastDraws = new Tile?[4];
            Rivers = new RiverData[4];
            LocalPlayerHandTiles = new List<Tile>();
            LocalPlayer = Lobby.LobbyManager.Instance.LocalPlayer;
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
        }

        public void UpdatePoints(int[] points)
        {
            for (int placeIndex = 0; placeIndex < Points.Length; placeIndex++)
            {
                int playerIndex = GetPlayerIndex(placeIndex);
                if (playerIndex < points.Length)
                    Points[placeIndex] = points[playerIndex];
            }
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
        }

        public void SetHandTiles(IList<Tile> handTiles)
        {
            LocalPlayerHandTiles = new List<Tile>(handTiles);
        }

        public void SetHandTiles(int placeIndex, int count)
        {
            TileCounts[placeIndex] = count;
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

        public void SetLastDraw(int placeIndex, Tile lastDraw = default(Tile))
        {
            for (int i = 0; i < LastDraws.Length; i++)
            {
                if (i == placeIndex) LastDraws[i] = lastDraw;
                else LastDraws[i] = null;
            }
        }

        public string GetPlayerName(int placeIndex)
        {
            if (PlayerNames == null) return "";
            return PlayerNames[placeIndex];
        }

        public void ClearLastDraws()
        {
            if (LastDraws == null) return;
            for (int i = 0; i < LastDraws.Length; i++)
                LastDraws[i] = null;
        }

        public Tile? GetLastDraw(int placeIndex)
        {
            if (LastDraws == null) return null;
            return LastDraws[placeIndex];
        }

        public void SetRiverData(int placeIndex, RiverData data)
        {
            Rivers[placeIndex] = data;
        }

        public RiverTile[] GetRiverTiles(int placeIndex)
        {
            if (Rivers == null) return null;
            return Rivers[placeIndex].River;
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
    }
}