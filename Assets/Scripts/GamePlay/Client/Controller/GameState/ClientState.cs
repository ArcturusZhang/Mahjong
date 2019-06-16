using Common.StateMachine.Interfaces;
using UnityEngine;
using GamePlay.Client.Model;

namespace GamePlay.Client.Controller.GameState
{
    public abstract class ClientState : IState
    {
        public ClientRoundStatus CurrentRoundStatus;
        protected ViewController controller;
        public void OnStateEnter()
        {
            Debug.Log($"Client enters {GetType().Name}");
            controller = ViewController.Instance;
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