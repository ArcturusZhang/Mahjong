using System;
using System.Collections.Generic;
using System.Linq;
using Mahjong.Logic;
using Mahjong.Model;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utils;

namespace GamePlay.Server.Model
{
    [Serializable]
    public class ServerRoundStatus
    {
        private int currentPlayerIndex = -1;
        private int oya = -1;
        private int field = 0;
        private int dice;
        private int extra;
        private int richiSticks;
        private int kongClaimed;
        private int[] bonusTurnTime;
        private List<Tile>[] handTiles;
        private List<OpenMeld>[] openMelds;
        private int[] points;
        private Tile? lastDraw = null;
        private bool[] richiStatus;
        private bool[] oneShotStatus;
        private bool firstTurn;
        private int turnCount;
        private List<RiverTile>[] rivers;
        private bool[] tempZhenting;
        private bool[] discardZhenting;
        private bool[] richiZhenting;
        private int[] beiDoras;
        private IList<int> playerActorNumbers;
        private IDictionary<int, string> playerNames;

        public ServerRoundStatus(GameSetting gameSettings, IList<Player> players)
        {
            GameSettings = gameSettings;
            points = new int[TotalPlayers];
            playerActorNumbers = players.Select(p => p.ActorNumber).ToList();
            playerNames = players.ToDictionary(p => p.ActorNumber, p => p.NickName);
        }

        public GameSetting GameSettings { get; }

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
        public Player GetPlayer(int playerIndex)
        {
            var room = PhotonNetwork.CurrentRoom;
            if (room == null) throw new ArgumentException("This should not happen");
            int actorNumber = playerActorNumbers[playerIndex];
            return room.Players[actorNumber];
        }
        public IList<int> PlayerActorNumbers => playerActorNumbers;
        public int OyaPlayerIndex => oya;
        public int Field => field;
        public int Dice => dice;
        public int Extra => extra;
        public int ExtraPoints => Extra * GameSettings.ExtraRoundBonusPerPlayer;
        public int RichiSticks => richiSticks;
        public int RichiSticksPoints => RichiSticks * GameSettings.RichiMortgagePoints;
        public Tile? LastDraw
        {
            get { return lastDraw; }
            set
            {
                if (default(Tile).Equals(value)) lastDraw = null;
                else lastDraw = value;
            }
        }
        public bool RichiStatus(int playerIndex)
        {
            return richiStatus[playerIndex];
        }
        public bool[] RichiStatusArray
        {
            get
            {
                var array = new bool[TotalPlayers];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = richiStatus[i];
                }
                return array;
            }
        }
        public bool OneShotStatus(int playerIndex)
        {
            return oneShotStatus[playerIndex];
        }
        public bool IsZhenting(int playerIndex)
        {
            return tempZhenting[playerIndex] || discardZhenting[playerIndex] || richiZhenting[playerIndex];
        }
        public bool FirstTurn => firstTurn;
        public int TotalPlayers => GameSettings.MaxPlayer;
        public string[] PlayerNames => playerActorNumbers.Select(id => playerNames[id]).ToArray();
        public int KongClaimed => kongClaimed;
        public int MaxBonusTurnTime => bonusTurnTime.Max();

        public string GetPlayerName(int index)
        {
            return playerNames[playerActorNumbers[index]];
        }

        public void ClaimKong()
        {
            kongClaimed++;
        }

        public void ShufflePlayers()
        {
            playerActorNumbers.Shuffle();
        }

        public int GetBonusTurnTime(int index)
        {
            CheckRange(index);
            return bonusTurnTime[index];
        }

        public void SetBonusTurnTime(int value)
        {
            for (int i = 0; i < bonusTurnTime.Length; i++)
                SetBonusTurnTime(i, value);
        }

        public void SetBonusTurnTime(int index, int value)
        {
            CheckRange(index);
            bonusTurnTime[index] = value;
        }

        public Tile[] HandTiles(int index)
        {
            CheckRange(index);
            return handTiles[index].ToArray();
        }

        public OpenMeld[] OpenMelds(int index)
        {
            CheckRange(index);
            return openMelds[index].ToArray();
        }

        public Meld[] Melds(int index)
        {
            CheckRange(index);
            return openMelds[index].Select(open => open.Meld).ToArray();
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
                var rivers = new RiverData[TotalPlayers];
                for (int i = 0; i < rivers.Length; i++)
                {
                    rivers[i] = new RiverData
                    {
                        River = this.rivers[i].ToArray()
                    };
                }
                return rivers;
            }
        }

        public int[] Points => points;

        public int GetPoints(int index)
        {
            return points[index];
        }

        public void SetPoints(int index, int point)
        {
            points[index] = point;
        }

        public void ChangePoints(int index, int amount)
        {
            points[index] += amount;
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
            if (i < 0) return;
            handTiles[index].RemoveAt(i);
        }

        public void RemoveTile(int index, OpenMeld meld)
        {
            CheckRange(index);
            var tiles = handTiles[index];
            int p = System.Array.FindIndex(meld.Tiles, tile => tile.EqualsConsiderColor(meld.Tile));
            for (int i = 0; i < meld.Tiles.Length; i++)
            {
                if (i == p) continue;
                RemoveTile(index, meld.Tiles[i]);
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

        public void AddMeld(int index, OpenMeld meld)
        {
            CheckRange(index);
            openMelds[index].Add(meld);
        }

        public void AddKong(int index, OpenMeld kong)
        {
            CheckRange(index);
            int i = openMelds[index].FindIndex(meld => meld.Type == MeldType.Triplet && meld.First.EqualsIgnoreColor(kong.First));
            if (i < 0)
            {
                Debug.LogError($"Pong of {kong} not found, this should not happen");
                return;
            }
            openMelds[index][i] = kong;
        }

        public void TryRichi(int index, bool isRichiing)
        {
            CheckRange(index);
            if (!isRichiing) return;
            if (richiStatus[index])
            {
                Debug.LogError($"Player {index} has already richied, therefore he cannot richi again.");
                return;
            }
            richiStatus[index] = true;
            oneShotStatus[index] = true;
            richiSticks++;
            points[index] -= GameSettings.RichiMortgagePoints;
        }

        public void BreakTempZhenting(int playerIndex)
        {
            CheckRange(playerIndex);
            tempZhenting[playerIndex] = false;
        }

        public void UpdateTempZhenting(int discardPlayerIndex, Tile discardTile)
        {
            // test temp zhenting
            for (int playerIndex = 0; playerIndex < GameSettings.MaxPlayer; playerIndex++)
            {
                if (playerIndex == discardPlayerIndex) continue;
                if (MahjongLogic.HasWin(handTiles[playerIndex], null, discardTile))
                    tempZhenting[playerIndex] = true;
            }
        }

        public void UpdateDiscardZhenting()
        {
            for (int i = 0; i < GameSettings.MaxPlayer; i++)
                UpdateDiscardZhenting(i);
        }

        public void UpdateDiscardZhenting(int playerIndex)
        {
            discardZhenting[playerIndex] = MahjongLogic.TestDiscardZhenting(handTiles[playerIndex], rivers[playerIndex]);
        }

        public void UpdateRichiZhenting(Tile discardTile)
        {
            for (int i = 0; i < GameSettings.MaxPlayer; i++)
                UpdateRichiZhenting(i, discardTile);
        }

        public void UpdateRichiZhenting(int playerIndex, Tile discardTile)
        {
            if (!RichiStatus(playerIndex) || richiZhenting[playerIndex]) return;
            if (MahjongLogic.HasWin(handTiles[playerIndex], null, discardTile))
                richiZhenting[playerIndex] = true;
        }

        public void CheckOneShot(int index)
        {
            if (oneShotStatus[index]) oneShotStatus[index] = false;
        }

        public void BreakOneShotsAndFirstTurn()
        {
            for (int i = 0; i < oneShotStatus.Length; i++)
            {
                oneShotStatus[i] = false;
                firstTurn = false;
            }
        }

        public void CheckFirstTurn(int playerIndex)
        {
            CheckRange(playerIndex);
            if (playerIndex == OyaPlayerIndex)
            {
                if (turnCount > 0)
                {
                    firstTurn = false;
                    return;
                }
                turnCount++;
            }
        }

        public bool CheckFourWinds()
        {
            if (!GameSettings.Allow4WindDraw) return false;
            if (!FirstTurn) return false;
            if (TotalPlayers < 4) return false;
            var first = rivers[0][0].Tile;
            for (int i = 1; i < TotalPlayers; i++)
            {
                var current = rivers[i][0].Tile;
                if (!current.EqualsIgnoreColor(first)) return false;
            }
            return true;
        }

        public bool CheckFourRichis()
        {
            if (!GameSettings.Allow4RichiDraw) return false;
            if (TotalPlayers < 4) return false;
            return richiStatus.All(r => r);
        }

        public bool CheckFourKongs()
        {
            if (!GameSettings.Allow4KongDraw) return false;
            if (KongClaimed < 4) return false;
            // check if 4 kongs are in same person
            for (int i = 0; i < TotalPlayers; i++)
            {
                var melds = openMelds[i];
                var kongCount = melds.Count(meld => meld.IsKong);
                if (kongCount >= 4) return false;
            }
            return true;
        }

        public bool CheckThreeRongs(OutTurnOperation[] operations)
        {
            if (!GameSettings.Allow3RongDraw) return false;
            if (TotalPlayers < 4) return false;
            var count = operations.Count(op => op.Type == OutTurnOperationType.Rong);
            return count == 3;
        }

        public int GetBeiDora(int index)
        {
            CheckRange(index);
            return beiDoras[index];
        }

        public int[] GetBeiDoras()
        {
            return beiDoras;
        }

        public void ClearBeiDoras()
        {
            for (int index = 0; index < beiDoras.Length; index++)
            {
                beiDoras[index] = 0;
            }
        }

        public void AddBeiDoras(int index)
        {
            CheckRange(index);
            beiDoras[index]++;
        }

        public bool IsAllLast => GameSettings.IsAllLast(OyaPlayerIndex, Field, TotalPlayers);

        public bool GameForceEnd => GameSettings.GameForceEnd(OyaPlayerIndex, Field, TotalPlayers);

        public void SortHandTiles(int index)
        {
            CheckRange(index);
            handTiles[index].Sort();
        }

        public void SortHandTiles()
        {
            for (int i = 0; i < GameSettings.MaxPlayer; i++)
            {
                SortHandTiles(i);
            }
        }

        public bool IsDealer(int index)
        {
            return index == OyaPlayerIndex;
        }

        public void NextRound(int dice, bool next = true, bool extra = false, bool keepSticks = false)
        {
            this.dice = dice;
            if (next)
            {
                oya++;
                if (oya >= GameSettings.MaxPlayer)
                {
                    oya -= GameSettings.MaxPlayer;
                    field++;
                }
            }
            if (extra) this.extra++;
            if (!keepSticks) richiSticks = 0;
            Debug.Log($"Entering next round, total players: {GameSettings.MaxPlayer}, current player index: {oya}, dice: {dice}, extra: {this.extra}");
            kongClaimed = 0;
            bonusTurnTime = new int[GameSettings.MaxPlayer];
            handTiles = new List<Tile>[GameSettings.MaxPlayer];
            openMelds = new List<OpenMeld>[GameSettings.MaxPlayer];
            rivers = new List<RiverTile>[GameSettings.MaxPlayer];
            richiStatus = new bool[GameSettings.MaxPlayer];
            oneShotStatus = new bool[GameSettings.MaxPlayer];
            tempZhenting = new bool[GameSettings.MaxPlayer];
            discardZhenting = new bool[GameSettings.MaxPlayer];
            richiZhenting = new bool[GameSettings.MaxPlayer];
            beiDoras = new int[GameSettings.MaxPlayer];
            firstTurn = true;
            turnCount = 0;
            for (int i = 0; i < GameSettings.MaxPlayer; i++)
            {
                handTiles[i] = new List<Tile>();
                openMelds[i] = new List<OpenMeld>();
                rivers[i] = new List<RiverTile>();
            }
        }

        private void CheckRange(int index)
        {
            if (index < 0 || index >= GameSettings.MaxPlayer)
                throw new IndexOutOfRangeException($"Player index out of range, should be within {0} to {GameSettings.MaxPlayer - 1}");
        }

        public override string ToString()
        {
            var list = new List<string>();
            for (int i = 0; i < handTiles.Length; i++)
            {
                list.Add($"Hand: {string.Join("", handTiles[i])}, "
                    + $"Open: {string.Join(",", openMelds[i])}, "
                    + $"River: {string.Join("", rivers[i])}");
            }
            return string.Join("\n", list);
        }
    }
}