using StateMachine.Interfaces;
using UnityEngine;

namespace Multi.GameState
{
    public abstract class AbstractMahjongState : IState
    {
        public virtual void OnStateEnter()
        {
            Debug.Log($"[StateMachine] Enter {GetType().Name}");
        }

        public virtual void OnStateUpdate()
        {
            Debug.Log($"[StateMachine] Execute {GetType().Name}");
        }

        public virtual void OnStateExit()
        {
            Debug.Log($"[StateMachine] Exit {GetType().Name}");
        }
    }
}