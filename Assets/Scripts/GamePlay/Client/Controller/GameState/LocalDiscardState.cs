using GamePlay.Client.View;
using Mahjong.Model;

namespace GamePlay.Client.Controller.GameState
{
    public class LocalDiscardState : ClientState
    {
        public int CurrentPlayerIndex;
        public bool IsRichiing;
        public bool DiscardingLastDraw;
        public Tile Tile;
        public override void OnClientStateEnter()
        {
            controller.InTurnPanelManager.Close();
            CurrentRoundStatus.DiscardTile(Tile, DiscardingLastDraw, IsRichiing);
            CurrentRoundStatus.CalculateWaitingTiles();
            if (IsRichiing)
                controller.ShowEffect(0, PlayerEffectManager.Type.Richi);
            controller.HandPanelManager.RemoveCandidates();
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}