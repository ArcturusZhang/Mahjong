using System;
using System.Collections.Generic;
using System.Linq;
using Single.Exceptions;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Multi.ServerData
{
    [Serializable]
    public class ServerRoundStatus
    {
        // todo -- game data accumulator on server
        private int currentPlayerIndex = -1;
        private int oya = -1;
        private int field = 0;
        private int dice;
        private int extra;
        private int richiSticks;
        private List<Player> players;
        private List<Tile>[] handTiles;
        private List<Meld>[] openMelds;
        private int[] points;
        private Tile? lastDraw = null;
        private bool[] richiStatus;
        private bool[] oneShotStatus;
        private List<RiverTile>[] rivers;

        public ServerRoundStatus(GameSettings gameSettings, YakuSettings yakuSettings, List<Player> players)
        {
            GameSettings = gameSettings;
            YakuSettings = yakuSettings;
            this.players = players;
            points = new int[players.Count];
        }

        public GameSettings GameSettings { get; }
        public YakuSettings YakuSettings { get; }

        public int CurrentPlayerIndex
        {
            get
            {
                return currentPlayerIndex;
            }
            set
            {
                currentPlayerIndex = value;
            }
        }
        public int OyaPlayerIndex => oya;
        public int Field => field;
        public int Dice => dice;
        public int Extra => extra;
        public int RichiSticks => richiSticks;
        public Tile? LastDraw
        {
            get { return lastDraw; }
            set
            {
                if (default(Tile).Equals(value)) lastDraw = null;
                else lastDraw = value;
            }
        }
        public bool[] RichiStatus => richiStatus;
        public bool[] OneShotStatus => oneShotStatus;
        public int TotalPlayers => players.Count;
        public string[] PlayerNames => players.Select(player => player.PlayerName).ToArray();

        private IList<Player> readOnlyPlayers = null;

        public IList<Player> Players
        {
            get
            {
                if (readOnlyPlayers == null) readOnlyPlayers = players.AsReadOnly();
                return readOnlyPlayers;
            }
        }

        public void ShufflePlayers()
        {
            players.Shuffle();
            readOnlyPlayers = null;
        }

        public Tile[] HandTiles(int index)
        {
            CheckRange(index);
            return handTiles[index].ToArray();
        }

        public Meld[] OpenMelds(int index)
        {
            CheckRange(index);
            return openMelds[index].ToArray();
        }

        public PlayerHandData HandData(int index)
        {
            CheckRange(index);
            return new PlayerHandData
            {
                HandTiles = HandTiles(index),
                OpenMelds = OpenMelds(index)
            };
        }

        public RiverData[] Rivers
        {
            get
            {
                var rivers = new RiverData[players.Count];
                for (int i = 0; i < players.Count; i++)
                {
                    rivers[i] = new RiverData
                    {
                        River = this.rivers[i].ToArray()
                    };
                }
                return rivers;
            }
        }

        public IList<int> Points
        {
            get
            {
                return Array.AsReadOnly(points);
            }
        }

        public int GetPoints(int index)
        {
            return points[index];
        }

        public void SetPoints(int index, int point)
        {
            points[index] = point;
        }

        public void AddTile(int index, Tile tile)
        {
            CheckRange(index);
            handTiles[index].Add(tile);
        }

        public void RemoveTile(int index, Tile tile)
        {
            CheckRange(index);
            var i = handTiles[index].FindIndex(t => t.EqualsConsiderColor(tile));
            if (i < 0) throw new NoMoreTilesException($"The tile {tile} does not found in list");
            handTiles[index].RemoveAt(i);
        }

        public void RemoveTile(int index, Meld meld)
        {
            CheckRange(index);
            var tiles = handTiles[index];
            foreach (var tile in meld.Tiles)
            {
                RemoveTile(index, tile);
            }
        }

        public void AddToRiver(int index, Tile tile, bool richi = false)
        {
            CheckRange(index);
            rivers[index].Add(new RiverTile
            {
                Tile = tile,
                IsRichi = richi,
                IsGone = false
            });
        }

        public void RemoveFromRiver(int index)
        {
            CheckRange(index);
            var river = rivers[index];
            if (river.Count == 0)
            {
                Debug.LogWarning($"There are no tiles in river {index}");
                return;
            }
            var tile = river[river.Count - 1];
            river[river.Count - 1] = new RiverTile
            {
                Tile = tile.Tile,
                IsRichi = tile.IsRichi,
                IsGone = true
            };
        }

        public void AddMeld(int index, Meld meld)
        {
            CheckRange(index);
            openMelds[index].Add(meld);
        }

        public void Richi(int index)
        {
            CheckRange(index);
            if (RichiStatus[index])
            {
                Debug.LogError($"Player {index} has already richied, therefore he cannot richi again.");
                return;
            }
            RichiStatus[index] = true;
        }

        public void SortHandTiles(int index)
        {
            CheckRange(index);
            handTiles[index].Sort();
        }

        public void SortHandTiles()
        {
            for (int i = 0; i < players.Count; i++)
            {
                SortHandTiles(i);
            }
        }

        public bool IsDealer(int index)
        {
            return index == OyaPlayerIndex;
        }

        // todo -- dealing with extra and new
        public void NextRound(int dice, bool next = true, bool extra = false, bool keepSticks = false)
        {
            this.dice = dice;
            if (next)
            {
                oya++;
                if (oya >= players.Count)
                {
                    oya -= players.Count;
                    field++;
                }
            }
            if (extra) this.extra++;
            if (!keepSticks) richiSticks = 0;
            Debug.Log($"Entering next round, total players: {players.Count}, current player index: {oya}, dice: {dice}, extra: {this.extra}");
            handTiles = new List<Tile>[players.Count];
            openMelds = new List<Meld>[players.Count];
            rivers = new List<RiverTile>[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                handTiles[i] = new List<Tile>();
                openMelds[i] = new List<Meld>();
                rivers[i] = new List<RiverTile>();
            }
            richiStatus = new bool[players.Count];
            oneShotStatus = new bool[players.Count];
        }

        private void CheckRange(int index)
        {
            if (index < 0 || index >= players.Count)
                throw new IndexOutOfRangeException($"Player index out of range, should be within {0} to {players.Count - 1}");
        }
    }
}