using System.Collections;
using System.Linq;
using Multi;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class PlayerDrawState : ClientState
    {
        public int PlayerIndex;
        public Tile Tile;
        public int BonusTurnTime;
        public bool Richied;
        public MahjongSetData MahjongSetData;
        public InTurnOperation[] Operations;
        private WaitForSeconds waitAutoDiscardAfterRichi = new WaitForSeconds(MahjongConstants.AutoDiscardDelayAfterRichi);

        public override void OnClientStateEnter()
        {
            if (CurrentRoundStatus.LocalPlayerIndex == PlayerIndex)
                HandleLocalPlayerDraw();
            else
                HandleOtherPlayerDraw();
        }

        private void HandleLocalPlayerDraw()
        {
            CurrentRoundStatus.SetLastDraw(0, Tile);
            CurrentRoundStatus.MahjongSetData = MahjongSetData;
            // auto discard when richied
            if (Richied && Operations.All(op => op.Type == InTurnOperationType.Discard))
            {
                controller.HandPanelManager.LockTiles();
                controller.StartCoroutine(AutoDiscard(Tile, BonusTurnTime));
                return;
            }
            // not richied, show timer and panels
            controller.TurnTimeController.StartCountDown(CurrentRoundStatus.Settings.BaseTurnTime, BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically discarding last drawn tile");
                CurrentRoundStatus.LocalPlayer.DiscardTile(Tile, false, true, 0);
            });
            controller.InTurnPanelManager.SetOperations(Operations);
        }

        private void HandleOtherPlayerDraw()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(PlayerIndex);
            CurrentRoundStatus.SetLastDraw(placeIndex);
            CurrentRoundStatus.MahjongSetData = MahjongSetData;
        }

        private IEnumerator AutoDiscard(Tile tile, int bonusTimeLeft)
        {
            yield return waitAutoDiscardAfterRichi;
            CurrentRoundStatus.LocalPlayer.DiscardTile(tile, false, true, bonusTimeLeft);
        }

        public override void OnClientStateExit()
        {
            controller.InTurnPanelManager.Close();
        }

        public override void OnStateUpdate()
        {
        }
    }
}