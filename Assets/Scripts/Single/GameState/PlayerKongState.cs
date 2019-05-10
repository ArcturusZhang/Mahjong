using System.Linq;
using Multi;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace Single.GameState
{
    public class PlayerKongState : ClientState
    {
        public int KongPlayerIndex;
        public PlayerHandData HandData;
        public int BonusTurnTime;
        public OutTurnOperation[] Operations;
        public MahjongSetData MahjongSetData;

        public override void OnClientStateEnter()
        {
            if (CurrentRoundStatus.LocalPlayerIndex == KongPlayerIndex)
                HandleLocalPlayerKong();
            else
                HandleOtherPlayerKong();
        }

        private void HandleLocalPlayerKong()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(KongPlayerIndex);
            Assert.IsTrue(placeIndex == 0);
            CurrentRoundStatus.SetHandTiles(HandData.HandTiles);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            CurrentRoundStatus.MahjongSetData = MahjongSetData;
            CurrentRoundStatus.LocalPlayer.SkipOutTurnOperation(BonusTurnTime);
        }
        private void HandleOtherPlayerKong()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(KongPlayerIndex);
            Debug.Log($"Hand tile count of player {KongPlayerIndex}: {HandData.HandTiles.Length}");
            CurrentRoundStatus.SetHandTiles(placeIndex, HandData.HandTiles.Length);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            CurrentRoundStatus.MahjongSetData = MahjongSetData;
            if (Operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                Debug.Log("Only operation is skip, skipping turn");
                CurrentRoundStatus.LocalPlayer.SkipOutTurnOperation(BonusTurnTime);
                controller.OutTurnPanelManager.Close();
                return;
            }
            // if there are valid operations, assign operations and start count down
            controller.OutTurnPanelManager.SetOperations(Operations);
            controller.TurnTimeController.StartCountDown(CurrentRoundStatus.Settings.BaseTurnTime, BonusTurnTime, () =>
            {
                Debug.Log("Time out, automatically skip");
                CurrentRoundStatus.LocalPlayer.SkipOutTurnOperation(0);
                controller.OutTurnPanelManager.Close();
            });
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}