using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Multi.GameState
{
    public class RoundPrepareState : AbstractMahjongState
    {
        public GameSettings GameSettings;
        public NetworkRoundStatus NetworkRoundStatus;
        public GameStatus GameStatus;
        private List<Player> players;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            players = GameStatus.Players;
            players.Shuffle();
            Debug.Log($"[RoundPrepareState] This game has total {players.Count} players");
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerIndex = i;
                players[i].TotalPlayers = players.Count;
            }
            GameStatus.Reset();
            NetworkRoundStatus.Initialize(players.Count);
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            Debug.Log($"[RoundPrepareState] Prepare finished");
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Points = GameSettings.InitialPoints;
            }
        }
    }
}