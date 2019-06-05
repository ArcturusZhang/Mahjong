using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

        private void OnEnable()
        {
            Instance = this;
        }

        public void AssignRoundStatus(ClientRoundStatus status)
        {
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
