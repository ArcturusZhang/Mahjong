using System.Collections.Generic;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Multi.GameState
{
    public class RoundPrepareState : AbstractMahjongState
    {
        public MahjongManager MahjongManager;
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        public GameStatus GameStatus;
        private List<Player> players;
        
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            players = GameStatus.Players;
            MahjongManager.RpcClientPrepare(GameSettings, YakuSettings);
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