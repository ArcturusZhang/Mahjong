using GamePlay.Client.View;
using GamePlay.Server.Model;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace GamePlay.Client.Controller.GameState
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
            CurrentRoundStatus.SetCurrentPlaceIndex(BeiDoraPlayerIndex);
            int placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            Assert.IsTrue(placeIndex == 0);
            CurrentRoundStatus.CheckLocalHandTiles(HandData.HandTiles);
            CurrentRoundStatus.ClearLastDraws();
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            CurrentRoundStatus.UpdateBeiDoras(BeiDoras);
            controller.ShowEffect(placeIndex, PlayerEffectManager.Type.Bei);
            ClientBehaviour.Instance.OnSkipOutTurnOperation(BonusTurnTime);
        }

        private void HandleOtherPlayerBeiDora()
        {
            CurrentRoundStatus.SetCurrentPlaceIndex(BeiDoraPlayerIndex);
            int placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            Assert.IsTrue(placeIndex != 0);
            Debug.Log($"Hand tile count of player {BeiDoraPlayerIndex}: {HandData.HandTiles.Length}");
            CurrentRoundStatus.SetHandTiles(placeIndex, HandData.HandTiles.Length);
            CurrentRoundStatus.ClearLastDraws();
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            controller.TableTilesManager.SetMelds(placeIndex, HandData.OpenMelds);
            CurrentRoundStatus.UpdateBeiDoras(BeiDoras);
            controller.ShowOutTurnPanels(Operations, BonusTurnTime);
            controller.ShowEffect(placeIndex, PlayerEffectManager.Type.Bei);
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}