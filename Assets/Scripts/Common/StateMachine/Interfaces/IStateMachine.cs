using System;

namespace Common.StateMachine.Interfaces
{
    public interface IStateMachine
    {
         void ChangeState(IState newState);
         void UpdateState();
    }
}