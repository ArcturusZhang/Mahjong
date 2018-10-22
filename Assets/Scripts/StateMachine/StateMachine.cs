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
            previousState?.OnStateExited();
            currentState = newState;
            currentState.OnStateEntered();
        }

        public virtual void UpdateState()
        {
            currentState?.OnStateUpdated();
        }

        public virtual void RollbackToPreviousState()
        {
            currentState.OnStateExited();
            currentState = previousState;
            currentState.OnStateEntered();
        }

        public Type CurrentState => currentState.GetType();
    }
}