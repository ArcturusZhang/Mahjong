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
        private ClientLocalSettings settings;
        public override void OnClientStateEnter()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(CurrentPlayerIndex);
            // update hand tiles
            CurrentRoundStatus.SetHandTiles(HandTiles);
            CurrentRoundStatus.SetZhenting(Zhenting);
            if (CurrentRoundStatus.IsLocalPlayerTurn(CurrentPlayerIndex))
                CurrentRoundStatus.CalculateWaitingTiles();
            controller.StartCoroutine(UpdateHandData(CurrentPlayerIndex, DiscardingLastDraw, Tile, Rivers));
            if (IsRichiing)
                controller.ShowEffect(placeIndex, UI.PlayerEffectManager.Type.Richi);
            if (Operations == null || Operations.Length == 0)
            {
                Debug.LogError("Received with no operations, this should not happen");
                localPlayer.SkipOutTurnOperation(BonusTurnTime);
                return;
            }
            settings = CurrentRoundStatus.LocalSettings;
            if (settings.He)
            {
                // handle auto-win
                int index = System.Array.FindIndex(Operations, op => op.Type == OutTurnOperationType.Rong);
                if (index >= 0)
                {
                    ClientBehaviour.Instance.OnOutTurnButtonClicked(Operations[index]);
                    return;
                }
            }
            if (settings.Ming)
            {
                // handle dont-open
                for (int i = 0; i < Operations.Length; i++)
                {
                    var operation = Operations[i];
                    if (operation.Type == OutTurnOperationType.Chow
                        || operation.Type == OutTurnOperationType.Pong
                        || operation.Type == OutTurnOperationType.Kong)
                        Operations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
            }
            // if all the operations are skip, automatically skip this turn.
            if (Operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                Debug.Log("Only operation is skip, skipping turn");
                localPlayer.SkipOutTurnOperation(BonusTurnTime);
                return;
            }
            controller.OutTurnPanelManager.SetOperations(Operations);
            controller.TurnTimeController.StartCountDown(CurrentRoundStatus.GameSetting.BaseTurnTime, BonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically skip this turn");
                localPlayer.SkipOutTurnOperation(0);
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
            controller.OutTurnPanelManager.Close();
        }

        public override void OnStateUpdate()
        {
        }
    }
}