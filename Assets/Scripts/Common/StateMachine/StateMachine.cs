using System;
using Common.StateMachine.Interfaces;

namespace Common.StateMachine
{
    [Serializable]
    public class StateMachine : IStateMachine
    {
        private IState currentState;

        public virtual void ChangeState(IState newState)
        {
            if (newState == null) throw new ArgumentException("New state cannot be null!");
            currentState?.OnStateExit();
            currentState = newState;
            currentState.OnStateEnter();
        }

        public virtual void UpdateState()
        {
            currentState?.OnStateUpdate();
        }

        public Type CurrentStateType => currentState?.GetType();
    }
}