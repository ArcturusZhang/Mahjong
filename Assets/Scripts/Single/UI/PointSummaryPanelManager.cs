using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using Single.UI.Controller;
using Single.UI.SubManagers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Single.UI
{
    public class PointSummaryPanelManager : MonoBehaviour
    {
        public Text PlayerNameText;
        public SummaryHandTileManager HandTileManager;
        public WinTypeManager WinTypeManager;
        public DoraPanelManager DoraPanelManager;
        public PointInfoManager PointInfoManager;
        public YakuRankManager YakuRankManager;
        public Button ConfirmButton;
        public CountDownController ConfirmCountDownController;
        private WaitForSeconds waiting = new WaitForSeconds(MahjongConstants.SummaryPanelDelayTime);

        public void ShowPanel(SummaryPanelData data, UnityAction callback)
        {
            ConfirmButton.onClick.RemoveAllListeners();
            ConfirmButton.onClick.AddListener(callback);
            gameObject.SetActive(true);
            PlayerNameText.text = data.PlayerName;
            HandTileManager.SetHandTiles(data.HandInfo.HandTiles, data.HandInfo.OpenMelds, data.HandInfo.WinningTile);
            WinTypeManager.SetType(data.HandInfo.IsTsumo);
            var uraDora = data.HandInfo.IsRichi ? data.HandInfo.UraDoraIndicators : null;
            DoraPanelManager.SetDoraIndicators(data.HandInfo.DoraIndicators, uraDora);
            // yaku list, total point and yaku rank
            StartCoroutine(YakuListCoroutine(data.PointInfo, data.TotalPoints, callback));
        }

        private IEnumerator YakuListCoroutine(PointInfo point, int totalPoints, UnityAction callback)
        {
            yield return PointInfoManager.SetPointInfo(point, totalPoints);
            yield return waiting;
            YakuRankManager.SetYakuRank(point);
            ConfirmCountDownController.StartCountDown(MahjongConstants.SummaryPanelWaitingTime, callback);
        }

        public int StopCountDown()
        {
            return ConfirmCountDownController.StopCountDown();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }

    public struct PlayerHandInfo
    {
        public IList<Tile> HandTiles;
        public IList<OpenMeld> OpenMelds;
        public Tile WinningTile;
        public IList<Tile> DoraIndicators;
        public IList<Tile> UraDoraIndicators;
        public bool IsTsumo;
        public bool IsRichi;
    }

    public struct SummaryPanelData
    {
        public PlayerHandInfo HandInfo;
        public PointInfo PointInfo;
        public int TotalPoints;
        public string PlayerName;
    }
}
