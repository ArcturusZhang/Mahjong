using System.Collections;
using System.Linq;
using Multi;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class PlayerDiscardOperationState : ClientState
    {
        public int CurrentPlayerIndex;
        public bool IsRichiing;
        public bool DiscardingLastDraw;
        public Tile Tile;
        public int BonusTurnTime;
        public bool Zhenting;
        public OutTurnOperation[] Operations;
        public Tile[] HandTiles;
        public RiverData[] Rivers;
        public override void OnClientStateEnter()
        {
            CurrentRoundStatus.SetCurrentPlaceIndex(CurrentPlayerIndex);
            int placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            // update hand tiles
            CurrentRoundStatus.SetHandTiles(HandTiles);
            CurrentRoundStatus.SetZhenting(Zhenting);
            if (CurrentRoundStatus.IsLocalPlayerTurn(CurrentPlayerIndex))
                CurrentRoundStatus.CalculateWaitingTiles();
            if (IsRichiing)
                controller.ShowEffect(placeIndex, UI.PlayerEffectManager.Type.Richi);
            controller.StartCoroutine(UpdateHandData(placeIndex, DiscardingLastDraw, Tile, Rivers));
        }

        private IEnumerator UpdateHandData(int currentPlaceIndex, bool discardingLastDraw, Tile tile, RiverData[] rivers)
        {
            CurrentRoundStatus.ClearLastDraws();
            controller.TableTilesManager.DiscardTile(currentPlaceIndex, discardingLastDraw);
            Debug.Log($"Playing player (place: {currentPlaceIndex}) discarding animation");
            yield return new WaitForEndOfFrame();
            for (int playerIndex = 0; playerIndex < rivers.Length; playerIndex++)
            {
                int placeIndex = CurrentRoundStatus.GetPlaceIndex(playerIndex);
                CurrentRoundStatus.SetRiverData(placeIndex, rivers[playerIndex]);
            }
            var panelShown = controller.ShowOutTurnPanels(Operations, BonusTurnTime);
            if (panelShown)
                controller.TableTilesManager.ShineLastTile(currentPlaceIndex);
        }

        public override void OnClientStateExit()
        {
            controller.OutTurnPanelManager.Close();
            controller.TableTilesManager.ShineOff();
        }

        public override void OnStateUpdate()
        {
        }
    }
}