using System.Collections;
using System.Linq;
using Multi;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace Single.GameState
{
    public class PlayerDrawState : ClientState
    {
        public int PlayerIndex;
        public Tile Tile;
        public int BonusTurnTime;
        public bool Zhenting;
        public MahjongSetData MahjongSetData;
        public InTurnOperation[] Operations;

        public override void OnClientStateEnter()
        {
            if (CurrentRoundStatus.LocalPlayerIndex == PlayerIndex)
                HandleLocalPlayerDraw();
            else
                HandleOtherPlayerDraw();
        }

        private void HandleLocalPlayerDraw()
        {
            CurrentRoundStatus.SetCurrentPlaceIndex(PlayerIndex);
            var placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            Assert.IsTrue(placeIndex == 0);
            CurrentRoundStatus.SetLastDraw(placeIndex, Tile);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            CurrentRoundStatus.SetZhenting(Zhenting);
            controller.ShowInTurnPanels(Operations, BonusTurnTime);
        }

        private void HandleOtherPlayerDraw()
        {
            CurrentRoundStatus.SetCurrentPlaceIndex(PlayerIndex);
            int placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            CurrentRoundStatus.SetLastDraw(placeIndex);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            Debug.Log($"LastDraws: {string.Join(",", CurrentRoundStatus.LastDraws)}");
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