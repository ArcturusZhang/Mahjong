using GamePlay.Server.Model;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace GamePlay.Client.Controller.GameState
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
            CurrentRoundStatus.DrawTile(placeIndex, Tile);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            CurrentRoundStatus.SetZhenting(Zhenting);
            controller.ShowInTurnPanels(Operations, BonusTurnTime);
        }

        private void HandleOtherPlayerDraw()
        {
            CurrentRoundStatus.SetCurrentPlaceIndex(PlayerIndex);
            int placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            CurrentRoundStatus.DrawTile(placeIndex);
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