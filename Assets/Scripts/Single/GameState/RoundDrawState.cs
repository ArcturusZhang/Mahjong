using Multi.ServerData;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class RoundDrawState : ClientState
    {
        public RoundDrawType RoundDrawType;
        public WaitingData[] WaitingData;

        public override void OnClientStateEnter()
        {
            controller.RoundDrawManager.SetDrawType(RoundDrawType);
            if (RoundDrawType == RoundDrawType.RoundDraw)
            {
                Debug.Log("Revealing hand tiles");
                HandleRoundDraw(WaitingData);
            }
        }

        private void HandleRoundDraw(WaitingData[] data)
        {
            for (int playerIndex = 0; playerIndex < data.Length; playerIndex++)
            {
                int placeIndex = CurrentRoundStatus.GetPlaceIndex(playerIndex);
                CheckReadyOrNot(placeIndex, data[playerIndex]);
            }
        }

        private void CheckReadyOrNot(int placeIndex, WaitingData data)
        {
            // Show tiles and corresponding panel
            if (data.WaitingTiles == null || data.WaitingTiles.Length == 0)
            {
                // no-ting
                controller.TableTilesManager.CloseDown(placeIndex);
                controller.WaitingPanelManagers[placeIndex].NotReady();
            }
            else
            {
                // ting
                controller.TableTilesManager.OpenUp(placeIndex);
                controller.TableTilesManager.SetHandTiles(placeIndex, data.HandTiles);
                controller.WaitingPanelManagers[placeIndex].Ready(data.WaitingTiles);
            }
        }

        public override void OnClientStateExit()
        {
            System.Array.ForEach(controller.WaitingPanelManagers, m => m.Close());
            controller.TableTilesManager.StandUp();
        }

        public override void OnStateUpdate()
        {
        }
    }
}