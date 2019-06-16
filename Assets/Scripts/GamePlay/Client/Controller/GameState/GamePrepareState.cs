namespace GamePlay.Client.Controller.GameState
{
    public class GamePrepareState : ClientState
    {
        public int[] Points;
        public string[] Names;
        public override void OnClientStateEnter()
        {
            // assign round status
            controller.AssignRoundStatus(CurrentRoundStatus);
            // update data
            CurrentRoundStatus.UpdatePoints(Points);
            CurrentRoundStatus.UpdateNames(Names);
            // send ready message
            ClientBehaviour.Instance.ClientReady();
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}
