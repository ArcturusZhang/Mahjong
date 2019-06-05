using System.Collections.Generic;
using Multi.ServerData;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Multi.GameState
{
    public abstract class ServerState : IState
    {
        public ServerRoundStatus CurrentRoundStatus;
        protected GameSetting gameSettings;
        protected YakuSetting yakuSettings;
        protected IList<Player> players;
        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            if (CurrentRoundStatus != null)
            {
                gameSettings = CurrentRoundStatus.GameSettings;
                yakuSettings = CurrentRoundStatus.YakuSettings;
                players = CurrentRoundStatus.Players;
            }
            OnServerStateEnter();
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
            OnServerStateExit();
        }

        public abstract void OnServerStateEnter();
        public abstract void OnServerStateExit();

        public abstract void OnStateUpdate();
    }
}