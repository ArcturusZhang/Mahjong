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
        public bool Zhenting;
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
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            CurrentRoundStatus.SetZhenting(Zhenting);
            // auto discard when richied
            if (Richied && Operations.All(op => op.Type == InTurnOperationType.Discard))
            {
                // CurrentRoundStatus.CalculateWaitingTiles();
                controller.HandPanelManager.LockTiles();
                controller.StartCoroutine(AutoDiscard(Tile, BonusTurnTime));
                return;
            }
            // not richied, show timer and panels
            CurrentRoundStatus.CalculatePossibleWaitingTiles();
            CurrentRoundStatus.ClearWaitingTiles();
            controller.TurnTimeController.StartCountDown(CurrentRoundStatus.GameSetting.BaseTurnTime, BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically discarding last drawn tile");
                localPlayer.DiscardTile(Tile, false, true, 0);
            });
            controller.InTurnPanelManager.SetOperations(Operations);
        }

        private void HandleOtherPlayerDraw()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(PlayerIndex);
            CurrentRoundStatus.SetLastDraw(placeIndex);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            Debug.Log($"LastDraws: {string.Join(",", CurrentRoundStatus.LastDraws)}");
        }

        private IEnumerator AutoDiscard(Tile tile, int bonusTimeLeft)
        {
            yield return waitAutoDiscardAfterRichi;
            localPlayer.DiscardTile(tile, false, true, bonusTimeLeft);
        }

        public override void OnClientStateExit()
        {
            CurrentRoundStatus.ClearPossibleWaitingTiles();
            controller.InTurnPanelManager.Close();
        }

        public override void OnStateUpdate()
        {
        }
    }
}