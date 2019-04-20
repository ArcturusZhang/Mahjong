using StateMachine.Interfaces;
using UnityEngine;


namespace Multi.GameState
{
    public class IdleState : IState
    {
        public void OnStateEnter()
        {
            Debug.Log("Enter IdleState");
        }

        public void OnStateExit()
        {
            Debug.Log("IdleState Update");
        }

        public void OnStateUpdate()
        {
            Debug.Log("Exit IdleState");
        }
    }
}