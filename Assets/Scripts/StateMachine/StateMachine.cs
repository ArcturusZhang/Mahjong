using System;
using StateMachine.Interfaces;

namespace StateMachine
{
    [Serializable]
    public abstract class StateMachine
    {
        private IState currentState;
        private IState previousState;
        public virtual void ChangeState(IState newState)
        {
            if (newState == null) throw new ArgumentException("New state cannot be null!");
            previousState = currentState;
            previousState?.OnStateExit();
            currentState = newState;
            currentState.OnStateEnter();
        }

        public virtual void UpdateState()
        {
            currentState?.OnStateUpdate();
        }

        public virtual void RollbackToPreviousState()
        {
            currentState.OnStateExit();
            currentState = previousState;
            currentState.OnStateEnter();
        }

        public Type CurrentState => currentState.GetType();
    }
}