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
            CurrentRoundStatus.SetLastDraw(0, Tile);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            CurrentRoundStatus.SetZhenting(Zhenting);
            controller.ShowInTurnPanels(Operations, BonusTurnTime);
        }

        private void HandleOtherPlayerDraw()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(PlayerIndex);
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