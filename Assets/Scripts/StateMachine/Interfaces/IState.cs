namespace StateMachine.Interfaces
{
	public interface IState
	{
		void OnStateEntered();
		void OnStateUpdated();
		void OnStateExited();
	}
}
