using StateMachine.Interfaces;
using UnityEngine;

namespace Multi.GameState
{
    public abstract class AbstractMahjongState : IState
    {
        public virtual void OnStateEntered()
        {
            Debug.Log($"[StateMachine] Enter {GetType().Name}");
        }

        public virtual void OnStateUpdated()
        {
            Debug.Log($"[StateMachine] Execute {GetType().Name}");
        }

        public virtual void OnStateExited()
        {
            Debug.Log($"[StateMachine] Exit {GetType().Name}");
        }
    }
}