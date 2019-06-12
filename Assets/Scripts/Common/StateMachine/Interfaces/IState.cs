namespace Common.StateMachine.Interfaces
{
	public interface IState
	{
		void OnStateEnter();
		void OnStateUpdate();
		void OnStateExit();
	}
}
