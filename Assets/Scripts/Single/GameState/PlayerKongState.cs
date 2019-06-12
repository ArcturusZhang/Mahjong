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
            CurrentRoundStatus.SetCurrentPlaceIndex(KongPlayerIndex);
            int placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            Assert.IsTrue(placeIndex == 0);
            CurrentRoundStatus.SetHandTiles(HandData.HandTiles);
            CurrentRoundStatus.ClearLastDraws();
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            localPlayer.SkipOutTurnOperation(BonusTurnTime);
            controller.ShowEffect(placeIndex, UI.PlayerEffectManager.Type.Kong);
        }
        private void HandleOtherPlayerKong()
        {
            CurrentRoundStatus.SetCurrentPlaceIndex(KongPlayerIndex);
            int placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            Debug.Log($"Hand tile count of player {KongPlayerIndex}: {HandData.HandTiles.Length}");
            CurrentRoundStatus.SetHandTiles(placeIndex, HandData.HandTiles.Length);
            CurrentRoundStatus.ClearLastDraws();
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            controller.ShowEffect(placeIndex, UI.PlayerEffectManager.Type.Kong);
            controller.ShowOutTurnPanels(Operations, BonusTurnTime);
        }

        public override void OnClientStateExit()
        {
            // controller.TableTilesManager.ShineOff();
        }

        public override void OnStateUpdate()
        {
        }
    }
}