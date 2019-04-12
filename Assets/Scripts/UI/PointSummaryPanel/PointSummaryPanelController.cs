using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Multi;
using Multi.Messages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PointSummaryPanel
{
    public class PointSummaryPanelController : MonoBehaviour
    {
        public HandPanelController HandPanelController;
        public DoraController DoraController;
        public PointInfoController PointInfoController;
        public YakuPointPanelController YakuPointPanelController;
        public YakuRankController YakuRankController;
        public ConfirmButtonController ConfirmButtonController;
        public GameSettings MGameSettings; // todo -- to be removed, only for testing
        public YakuSettings MYakuSettings; // todo -- to be removed, only for testing

        [Obsolete] public int LosePlayerIndex { private get; set; }
        public GameSettings GameSettings { private get; set; }
        public YakuSettings YakuSettings { private get; set; }
        public Tile[] HandTiles { private get; set; }
        public Meld[] OpenMelds { private get; set; }
        public Tile WinningTile { private get; set; }
        public PointInfo PointInfo { private get; set; }
        public Tile[] DoraIndicators { private get; set; }
        public Tile[] UraDoraIndicators { private get; set; }
        public int Point { private get; set; }

//        // todo -- to be removed, only for testing
//        private void Awake()
//        {
//            GameSettings = MGameSettings;
//            YakuSettings = MYakuSettings;
//        }
//
//        // todo -- for test
//        private void Start()
//        {
//            StartCoroutine(ShowSummaryPanel(new PlayerServerData
//            {
//                WinPlayerIndex = 0,
//                HandTiles = new[]
//                {
//                    new Tile(Suit.M, 7), new Tile(Suit.M, 8),
//                    new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 7), new Tile(Suit.S, 8),
//                    new Tile(Suit.S, 9),
//                    new Tile(Suit.P, 7), new Tile(Suit.P, 8), new Tile(Suit.P, 9),
//                    new Tile(Suit.P, 7), new Tile(Suit.P, 8), new Tile(Suit.P, 9)
//                },
//                OpenMelds = new Meld[] { },
//                WinningTile = new Tile(Suit.M, 9),
//                HandStatus = HandStatus.Menqing | HandStatus.Richi | HandStatus.Tsumo | HandStatus.OneShot,
//                RoundStatus = new RoundStatus
//                {
//                    PlayerIndex = 0, RoundCount = 1, FieldCount = 1, TotalPlayer = 4, CurrentExtraRound = 0,
//                    TilesLeft = 100
//                },
//                DoraIndicators = new Tile[] {new Tile(Suit.S, 9),},
//                UraDoraIndicators = new Tile[] {new Tile(Suit.P, 8),}
//            }, 1));
//        }

        // todo -- complete this class to fully use all the data provided in playerServerData
        // todo -- should the appearance of point panels be handled here?
        public IEnumerator ShowSummaryPanel(PlayerServerData playerServerData, UnityAction callback = null)
        {
            gameObject.SetActive(true);
//            LosePlayerIndex = losePlayerIndex;
            HandTiles = playerServerData.HandTiles;
            OpenMelds = playerServerData.OpenMelds;
            WinningTile = playerServerData.WinningTile;
            DoraIndicators = playerServerData.DoraIndicators;
            UraDoraIndicators = playerServerData.UraDoraIndicators;
            PointInfo = MahjongLogic.GetPointInfo(HandTiles, OpenMelds, WinningTile, playerServerData.HandStatus,
                playerServerData.RoundStatus, YakuSettings, DoraIndicators, UraDoraIndicators);
            Point = MahjongLogic.GetTotalPoint(PointInfo, playerServerData.RoundStatus);
            yield return StartCoroutine(SummaryPanelStart());
            gameObject.SetActive(false);
            callback?.Invoke();
        }

        private void OnDisable()
        {
            HandPanelController.gameObject.SetActive(false);
            DoraController.gameObject.SetActive(false);
            PointInfoController.gameObject.SetActive(false);
            YakuPointPanelController.gameObject.SetActive(false);
            YakuRankController.gameObject.SetActive(false);
            ConfirmButtonController.gameObject.SetActive(false);
        }

        private IEnumerator SummaryPanelStart()
        {
            // set hand tiles
            HandPanelController.SetTiles(HandTiles, OpenMelds, WinningTile);
            yield return new WaitForSeconds(GameSettings.SummaryPanelDelayTime);
            // set dora panel
            DoraController.SetDoras(DoraIndicators, UraDoraIndicators);
            yield return new WaitForSeconds(GameSettings.SummaryPanelDelayTime);
            // show yaku items, total fan and fu
            yield return
                StartCoroutine(PointInfoController.SetPointInfo(PointInfo, GameSettings.SummaryPanelDelayTime));
            YakuRankController.SetYakuRank(PointInfo);
            yield return new WaitForSeconds(GameSettings.SummaryPanelDelayTime);
            YakuPointPanelController.SetPoint(Point);
            // show confirm button
            yield return StartCoroutine(ConfirmButtonController.StartCountDown(GameSettings.SummaryPanelWaitingTime,
                () =>
                {
                    ConfirmButtonController.CountDownController.gameObject.SetActive(false);
                    ConfirmButtonController.Button.interactable = false;
                    Debug.Log("Clicked");
                }));
        }
    }
}