using System.Collections.Generic;
using Common.StateMachine.Interfaces;
using GamePlay.Server.Model;
using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Server.Controller.GameState
{
    public abstract class ServerState : IState
    {
        public ServerRoundStatus CurrentRoundStatus;
        protected GameSetting gameSettings;
        protected IList<Player> players;
        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            if (CurrentRoundStatus != null)
            {
                gameSettings = CurrentRoundStatus.GameSettings;
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