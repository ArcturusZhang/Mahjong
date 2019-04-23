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
            DoraPanelManager.SetDoraIndicators(data.HandInfo.DoraIndicators, data.HandInfo.UraDoraIndicators);
            // yaku list, total point and yaku rank
            StartCoroutine(YakuListCoroutine(data.PointInfo, data.Multiplier, callback));
        }

        private IEnumerator YakuListCoroutine(PointInfo point, int multiplier, UnityAction callback)
        {
            yield return PointInfoManager.SetPointInfo(point, multiplier);
            yield return waiting;
            YakuRankManager.SetYakuRank(point);
            ConfirmCountDownController.StartCountDown(MahjongConstants.SummaryPanelWaitingTime, callback);
        }

        public void Close() {
            gameObject.SetActive(false);
        }
    }

    public struct PlayerHandInfo
    {
        public IList<Tile> HandTiles;
        public IList<Meld> OpenMelds;
        public Tile WinningTile;
        public IList<Tile> DoraIndicators;
        public IList<Tile> UraDoraIndicators;
        public bool IsTsumo;
    }

    public struct SummaryPanelData
    {
        public PlayerHandInfo HandInfo;
        public PointInfo PointInfo;
        public int Multiplier;
        public string PlayerName;
    }
}
