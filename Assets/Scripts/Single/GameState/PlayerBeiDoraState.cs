using System.Linq;
using Multi;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;

namespace Single.GameState
{
    public class PlayerBeiDoraState : ClientState
    {
        public int BeiDoraPlayerIndex;
        public int[] BeiDoras;
        public PlayerHandData HandData;
        public int BonusTurnTime;
        public OutTurnOperation[] Operations;
        public MahjongSetData MahjongSetData;
        public override void OnClientStateEnter()
        {
            if (CurrentRoundStatus.LocalPlayerIndex == BeiDoraPlayerIndex)
                HandleLocalPlayerBeiDora();
            else
                HandleOtherPlayerBeiDora();
        }

        private void HandleLocalPlayerBeiDora()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(BeiDoraPlayerIndex);
            Assert.IsTrue(placeIndex == 0);
            CurrentRoundStatus.SetHandTiles(HandData.HandTiles);
            CurrentRoundStatus.ClearLastDraws();
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            CurrentRoundStatus.UpdateBeiDoras(BeiDoras);
            localPlayer.SkipOutTurnOperation(BonusTurnTime);
        }

        private void HandleOtherPlayerBeiDora()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(BeiDoraPlayerIndex);
            Debug.Log($"Hand tile count of player {BeiDoraPlayerIndex}: {HandData.HandTiles.Length}");
            CurrentRoundStatus.SetHandTiles(placeIndex, HandData.HandTiles.Length);
            CurrentRoundStatus.ClearLastDraws();
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            CurrentRoundStatus.UpdateBeiDoras(BeiDoras);
            controller.ShowOutTurnPanels(Operations, BonusTurnTime);
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}