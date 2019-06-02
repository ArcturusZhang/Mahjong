using Multi;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public abstract class ClientState : IState
    {
        public ClientRoundStatus CurrentRoundStatus;
        protected ViewController controller;
        protected Player localPlayer;
        public void OnStateEnter()
        {
            Debug.Log($"Client enters {GetType().Name}");
            controller = ViewController.Instance;
            localPlayer = CurrentRoundStatus.LocalPlayer;
            OnClientStateEnter();
        }
        public void OnStateExit()
        {
            Debug.Log($"Client exits {GetType().Name}");
            OnClientStateExit();
        }
        public abstract void OnStateUpdate();

        public abstract void OnClientStateEnter();
        public abstract void OnClientStateExit();
    }
}