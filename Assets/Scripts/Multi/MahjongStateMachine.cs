using System;
using System.Collections.Generic;
using Single;
using Single.MahjongDataType;

namespace Multi
{
    [Serializable]
    public class MahjongStateMachine : StateMachine.StateMachine
    {
    }

    [Serializable]
    public class GameStatus
    {
        public int CurrentPlayerIndex;
        public Player CurrentTurnPlayer;
        public List<Player> Players;
        public RoundStatus RoundStatus;
        public List<Tile>[] PlayerHandTiles;
        public List<Meld>[] PlayerOpenMelds;
        public int Dice;

        public void Reset()
        {
            CurrentPlayerIndex = 0;
            CurrentTurnPlayer = Players[CurrentPlayerIndex];
            RoundStatus = new RoundStatus
            {
                RoundCount = 0,
                FieldCount = 1,
                CurrentExtraRound = 0,
                TotalPlayer = Players.Count,
                TilesLeft = MahjongConstants.TotalTilesCount,
            };
            PlayerHandTiles = new List<Tile>[Players.Count];
            PlayerOpenMelds = new List<Meld>[Players.Count];
        }

        public void SetCurrentPlayerIndex(int index)
        {
            CurrentPlayerIndex = index;
            CurrentTurnPlayer = Players[index];
        }

        public int NextPlayerIndex
        {
            get
            {
                int nextPlayerIndex = CurrentPlayerIndex + 1;
                if (nextPlayerIndex >= Players.Count) nextPlayerIndex -= Players.Count;
                return nextPlayerIndex;
            }
        }
    }
}