using System;

namespace StateMachine.Interfaces
{
    public interface IStateMachine
    {
         void ChangeState(IState newState);
         void UpdateState();
    }
}