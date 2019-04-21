using StateMachine.Interfaces;
using UnityEngine;

namespace Multi.GameState
{
    public class OperationPerformState : IState
    {
        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }

        public void OnStateUpdate()
        {
            // Debug.Log($"Server is in {GetType().Name}");
        }
    }
}