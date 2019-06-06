using Multi;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;

namespace Single.GameState
{
    public class PlayerOperationPerformState : ClientState
    {
        public int PlayerIndex;
        public int OperationPlayerIndex;
        public OutTurnOperation Operation;
        public PlayerHandData HandData;
        public int BonusTurnTime;
        public RiverData[] Rivers;
        public MahjongSetData MahjongSetData;

        public override void OnClientStateEnter()
        {
            if (CurrentRoundStatus.LocalPlayerIndex == OperationPlayerIndex)
                HandleLocalPlayerOperation();
            else
                HandleOtherPlayerOperation();
        }

        private void HandleLocalPlayerOperation()
        {
            // update local data
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(OperationPlayerIndex);
            Assert.IsTrue(placeIndex == 0);
            SetRoundStatusData();
            // update ui elements
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            // if not kong, start timer
            if (Operation.Type != OutTurnOperationType.Kong)
            {
                controller.TurnTimeController.StartCountDown(CurrentRoundStatus.GameSetting.BaseTurnTime, BonusTurnTime, () =>
                {
                    Debug.Log("Time out, automatically discard rightmost tile");
                    var tile = HandData.HandTiles[HandData.HandTiles.Length - 1];
                    localPlayer.DiscardTile(tile, false, false, 0);
                });
            }
        }

        private void EnableAllTiles()
        {
            CurrentRoundStatus.SetForbiddenTiles(null);
        }

        private void HandleOtherPlayerOperation()
        {
            // update local data
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(OperationPlayerIndex);
            Debug.Log($"Hand tile count of player {OperationPlayerIndex}: {HandData.HandTiles.Length}");
            CurrentRoundStatus.SetHandTiles(placeIndex, HandData.HandTiles.Length);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            SetRiverData();
            // update ui elements
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
        }

        private void SetRoundStatusData()
        {
            CurrentRoundStatus.SetHandTiles(HandData.HandTiles);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            SetRiverData();
            CurrentRoundStatus.CalculatePossibleWaitingTiles();
            CurrentRoundStatus.SetForbiddenTiles(Operation.ForbiddenTiles);
        }

        private void SetRiverData()
        {
            for (int playerIndex = 0; playerIndex < Rivers.Length; playerIndex++)
            {
                int placeIndex = CurrentRoundStatus.GetPlaceIndex(playerIndex);
                CurrentRoundStatus.SetRiverData(placeIndex, Rivers[playerIndex]);
            }
        }

        public override void OnClientStateExit()
        {
            EnableAllTiles();
        }

        public override void OnStateUpdate()
        {
        }
    }
}