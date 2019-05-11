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
        public OutTurnOperation[] Operations;
        public Tile[] HandTiles;
        public RiverData[] Rivers;
        public override void OnClientStateEnter()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(CurrentPlayerIndex);
            // update hand tiles
            CurrentRoundStatus.SetHandTiles(HandTiles);
            controller.StartCoroutine(UpdateHandData(CurrentPlayerIndex, DiscardingLastDraw, Tile, Rivers));
            if (IsRichiing)
                controller.StartCoroutine(controller.ShowEffect(placeIndex, UI.PlayerEffectManager.Type.Richi));
            if (Operations == null || Operations.Length == 0)
            {
                Debug.LogError("Received with no operations, this should not happen");
                CurrentRoundStatus.LocalPlayer.SkipOutTurnOperation(BonusTurnTime);
                return;
            }
            // if all the operations are skip, automatically skip this turn.
            if (Operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                Debug.Log("Only operation is skip, skipping turn");
                CurrentRoundStatus.LocalPlayer.SkipOutTurnOperation(BonusTurnTime);
                return;
            }
            controller.OutTurnPanelManager.SetOperations(Operations);
            controller.TurnTimeController.StartCountDown(CurrentRoundStatus.Settings.BaseTurnTime, BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically skip this turn");
                CurrentRoundStatus.LocalPlayer.SkipOutTurnOperation(0);
            });
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
        }

        public override void OnClientStateExit()
        {
            Debug.Log($"Client exits {GetType().Name}");
            controller.OutTurnPanelManager.Close();
        }

        public override void OnStateUpdate()
        {
        }
    }
}