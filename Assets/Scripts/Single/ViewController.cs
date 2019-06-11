using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Multi;
using Single.MahjongDataType;
using Single.Managers;
using Single.UI;
using Single.UI.Controller;
using UnityEngine;

namespace Single
{
    public class ViewController : MonoBehaviour
    {
        public static ViewController Instance { get; private set; }

        public BoardInfoManager BoardInfoManager;
        public YamaManager YamaManager;
        public TableTilesManager TableTilesManager;
        public PlayerInfoManager PlayerInfoManager;
        public HandPanelManager HandPanelManager;
        public TimerController TurnTimeController;
        public PlayerEffectManager PlayerEffectManager;
        public InTurnPanelManager InTurnPanelManager;
        public OutTurnPanelManager OutTurnPanelManager;
        public MeldSelectionManager MeldSelectionManager;
        public WaitingPanelManager[] WaitingPanelManagers;
        public ReadyHintManager ReadyHintManager;
        public RoundDrawManager RoundDrawManager;
        public PointSummaryPanelManager PointSummaryPanelManager;
        public PointTransferManager PointTransferManager;
        public GameEndPanelManager GameEndPanelManager;
        public LocalSettingManager LocalSettingManager;
        private ClientRoundStatus CurrentRoundStatus;
        private WaitForSeconds waitAutoDiscardAfterRichi = new WaitForSeconds(MahjongConstants.AutoDiscardDelayAfterRichi);

        private void OnEnable()
        {
            Instance = this;
        }

        public void AssignRoundStatus(ClientRoundStatus status)
        {
            CurrentRoundStatus = status;
            status.AddObserver(BoardInfoManager);
            status.AddObserver(YamaManager);
            status.AddObserver(TableTilesManager);
            status.AddObserver(PlayerInfoManager);
            status.AddObserver(HandPanelManager);
            status.AddObserver(PointTransferManager);
            status.AddObserver(ReadyHintManager);
            // add tiles as observer
            foreach (var tile in HandPanelManager.HandTiles)
            {
                status.AddObserver(tile);
            }
            status.AddObserver(HandPanelManager.LastDrawTile);
            status.LocalSettings.AddObserver(LocalSettingManager);
        }

        public void ShowInTurnPanels(InTurnOperation[] operations, int bonusTurnTime)
        {
            var localPlayer = CurrentRoundStatus.LocalPlayer;
            var settings = CurrentRoundStatus.LocalSettings;
            var richied = CurrentRoundStatus.GetRichiStatus(0);
            var lastDraw = (Tile)CurrentRoundStatus.GetLastDraw(0);
            // auto discard when richied or set to qie
            if ((settings.Qie || richied) && operations.All(op => op.Type == InTurnOperationType.Discard))
            {
                if (richied) HandPanelManager.LockTiles();
                StartCoroutine(AutoDiscard(lastDraw, bonusTurnTime));
                InTurnPanelManager.Close();
                return;
            }
            // check settings
            if (settings.He)
            {
                // handle auto-win
                int index = System.Array.FindIndex(operations, op => op.Type == InTurnOperationType.Tsumo);
                if (index >= 0)
                {
                    ClientBehaviour.Instance.OnTsumoButtonClicked(operations[index]);
                    return;
                }
            }
            // not richied, show timer and panels
            CurrentRoundStatus.CalculatePossibleWaitingTiles();
            CurrentRoundStatus.ClearWaitingTiles();
            InTurnPanelManager.SetOperations(operations);
            TurnTimeController.StartCountDown(CurrentRoundStatus.GameSetting.BaseTurnTime, bonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically discarding last drawn tile");
                localPlayer.DiscardTile(lastDraw, false, true, 0);
                InTurnPanelManager.Close();
            });
        }

        private IEnumerator AutoDiscard(Tile tile, int bonusTimeLeft)
        {
            yield return waitAutoDiscardAfterRichi;
            CurrentRoundStatus.LocalPlayer.DiscardTile(tile, false, true, bonusTimeLeft);
        }

        public bool ShowOutTurnPanels(OutTurnOperation[] operations, int bonusTurnTime)
        {
            var localPlayer = CurrentRoundStatus.LocalPlayer;
            if (operations == null || operations.Length == 0)
            {
                Debug.LogError("Received with no operations, this should not happen");
                localPlayer.SkipOutTurnOperation(bonusTurnTime);
                return false;
            }
            var settings = CurrentRoundStatus.LocalSettings;
            if (settings.He)
            {
                // handle auto-win
                int index = System.Array.FindIndex(operations, op => op.Type == OutTurnOperationType.Rong);
                if (index >= 0)
                {
                    ClientBehaviour.Instance.OnOutTurnButtonClicked(operations[index]);
                    return false;
                }
            }
            if (settings.Ming)
            {
                // handle dont-open
                for (int i = 0; i < operations.Length; i++)
                {
                    var operation = operations[i];
                    if (operation.Type == OutTurnOperationType.Chow
                        || operation.Type == OutTurnOperationType.Pong
                        || operation.Type == OutTurnOperationType.Kong)
                        operations[i] = new OutTurnOperation { Type = OutTurnOperationType.Skip };
                }
            }
            // if all the operations are skip, automatically skip this turn.
            if (operations.All(op => op.Type == OutTurnOperationType.Skip))
            {
                Debug.Log("Only operation is skip, skipping turn");
                localPlayer.SkipOutTurnOperation(bonusTurnTime);
                OutTurnPanelManager.Close();
                return false;
            }
            OutTurnPanelManager.SetOperations(operations);
            TurnTimeController.StartCountDown(CurrentRoundStatus.GameSetting.BaseTurnTime, bonusTurnTime, () =>
            {
                Debug.Log("Time out! Automatically skip this turn");
                localPlayer.SkipOutTurnOperation(0);
                OutTurnPanelManager.Close();
            });
            return true;
        }

        public float ShowEffect(int placeIndex, PlayerEffectManager.Type type)
        {
            return PlayerEffectManager.ShowEffect(placeIndex, type);
        }

        public IEnumerator RevealHandTiles(int placeIndex, PlayerHandData handData)
        {
            yield return new WaitForSeconds(MahjongConstants.HandTilesRevealDelay);
            TableTilesManager.OpenUp(placeIndex);
            TableTilesManager.SetHandTiles(placeIndex, handData.HandTiles);
        }
    }
}
