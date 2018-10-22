using System.Collections.Generic;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using Utils;

namespace Multi.GameState
{
    public class RoundPrepareState : AbstractMahjongState
    {
        public MahjongManager MahjongManager;
        public GameSettings GameSettings;
        public GameStatus GameStatus;
        private List<Player> players;
        
        public override void OnStateEntered()
        {
            base.OnStateEntered();
            players = GameStatus.Players;
            MahjongManager.RpcClientPrepare();
            players.Shuffle();
            Debug.Log($"[RoundPrepareState] This game has total {players.Count} players");
            for (int i = 0; i < players.Count; i++)
            {
                players[i].PlayerIndex = i;
                players[i].TotalPlayers = players.Count;
                players[i].Points = GameSettings.InitialPoints;
            }

            GameStatus.Reset();
        }
    }
}