using System.Collections;
using System.Collections.Generic;
using Multi;
using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PointSummaryPanel
{
    public class PointSummaryPanelController : MonoBehaviour
    {
        public Player Player;
        public HandPanelController HandPanelController;
        public DoraController DoraController;
        public PointInfoController PointInfoController;
        public YakuRankController YakuRankController;
        public ConfirmButtonController ConfirmButtonController;
//        public GameSettings MGameSettings; // todo -- to be removed, only for testing
//        public YakuSettings YakuSettings; // todo -- to be removed, only for testing

        public GameSettings GameSettings { private get; set; }
        public YakuSettings YakuSettings { private get; set; }
        public List<Tile> HandTiles { private get; set; }
        public List<Meld> OpenMelds { private get; set; }
        public Tile WinningTile { private get; set; }
        public PointInfo PointInfo { private get; set; }
        public List<Tile> DoraIndicators { private get; set; }
        public List<Tile> UraDoraIndicators { private get; set; }

//        // todo -- to be removed, only for testing
//        private void Awake()
//        {
//            GameSettings = MGameSettings;
////            HandTiles = new List<Tile>
////            {
////                new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1),
////                new Tile(Suit.Z, 2), new Tile(Suit.Z, 2), new Tile(Suit.Z, 2),
////                new Tile(Suit.Z, 4)
////            };
////            OpenMelds = new List<Meld>
////            {
////                new Meld(true, new Tile(Suit.Z, 3), new Tile(Suit.Z, 3), new Tile(Suit.Z, 3)),
////                new Meld(false, new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5))
////            };
////            WinningTile = new Tile(Suit.Z, 4);
//            HandTiles = new List<Tile>
//            {
//                new Tile(Suit.M, 7), new Tile(Suit.M, 8),
//                new Tile(Suit.S, 1), new Tile(Suit.S, 1), new Tile(Suit.S, 7), new Tile(Suit.S, 8), new Tile(Suit.S, 9),
//                new Tile(Suit.P, 7), new Tile(Suit.P, 8), new Tile(Suit.P, 9),
//                new Tile(Suit.P, 7), new Tile(Suit.P, 8), new Tile(Suit.P, 9)
//            };
//            OpenMelds = new List<Meld>();
//            WinningTile = new Tile(Suit.M, 9);
//            var handStatus = HandStatus.Menqing | HandStatus.Richi | HandStatus.Tsumo | HandStatus.OneShot;
//            var roundStatus = new RoundStatus
//            {
//                PlayerIndex = 0, RoundCount = 1, FieldCount = 1, TotalPlayer = 4, CurrentExtraRound = 0,
//                TilesLeft = 100
//            };
//            DoraIndicators = new List<Tile> {new Tile(Suit.S, 3)};
//            UraDoraIndicators = new List<Tile> {new Tile(Suit.P, 3)};
//            PointInfo = MahjongLogic.GetPointInfo(HandTiles, OpenMelds, WinningTile, handStatus,
//                roundStatus, YakuSettings, DoraIndicators, UraDoraIndicators);
//        }
//
//        // todo -- for test
//        private void Start()
//        {
//            StartCoroutine(SummaryPanelStart());
//        }

        public void ShowSummaryPanel()
        {
            gameObject.SetActive(true);
            StartCoroutine(SummaryPanelStart());
        }

        private void OnDisable()
        {
            HandPanelController.gameObject.SetActive(false);
            DoraController.gameObject.SetActive(false);
            PointInfoController.gameObject.SetActive(false);
            ConfirmButtonController.gameObject.SetActive(false);
        }

        private IEnumerator SummaryPanelStart()
        {
            yield return new WaitForSeconds(3f);
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
            // show confirm button
            yield return StartCoroutine(ConfirmButtonController.StartCountDown(GameSettings.SummaryPanelWaitingTime,
                () =>
                {
                    ConfirmButtonController.CountDownImage.gameObject.SetActive(false);
                    ConfirmButtonController.Button.interactable = false;
                    // send message to server
                    if (Player == null) return;
                    Player.connectionToServer.Send(MessageConstants.ReadinessMessageId, new ReadinessMessage
                    {
                        PlayerIndex = Player.PlayerIndex
                    });
                }));
        }
    }
}