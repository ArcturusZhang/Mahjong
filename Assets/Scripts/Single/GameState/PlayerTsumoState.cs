using System.Collections;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single.MahjongDataType;
using Single.UI;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class PlayerTsumoState : ClientState
    {
        public int TsumoPlayerIndex;
        public string TsumoPlayerName;
        public PlayerHandData TsumoHandData;
        public Tile WinningTile;
        public Tile[] DoraIndicators;
        public Tile[] UraDoraIndicators;
        public bool IsRichi;
        public NetworkPointInfo TsumoPointInfo;
        public int TotalPoints;

        public override void OnClientStateEnter()
        {
            int placeIndex = CurrentRoundStatus.GetPlaceIndex(TsumoPlayerIndex);
            CurrentRoundStatus.SetLastDraw(placeIndex, WinningTile);
            var data = new SummaryPanelData
            {
                HandInfo = new PlayerHandInfo
                {
                    HandTiles = TsumoHandData.HandTiles,
                    OpenMelds = TsumoHandData.OpenMelds,
                    WinningTile = WinningTile,
                    DoraIndicators = DoraIndicators,
                    UraDoraIndicators = UraDoraIndicators,
                    IsRichi = IsRichi,
                    IsTsumo = true
                },
                PointInfo = new PointInfo(TsumoPointInfo),
                TotalPoints = TotalPoints,
                PlayerName = TsumoPlayerName
            };
            // reveal hand tiles
            controller.StartCoroutine(controller.RevealHandTiles(placeIndex, TsumoHandData));
            controller.StartCoroutine(ShowAnimations(placeIndex, data));
        }

        private IEnumerator ShowAnimations(int placeIndex, SummaryPanelData data)
        {
            yield return controller.ShowEffect(placeIndex, PlayerEffectManager.Type.Tsumo);
            controller.PointSummaryPanelManager.ShowPanel(data, () =>
            {
                Debug.Log("Sending readiness message");
                localPlayer.ClientReady(MessageIds.ServerPointTransferMessage);
            });
        }

        public override void OnClientStateExit()
        {
            Debug.Log($"Client exits {GetType().Name}");
            controller.PointSummaryPanelManager.Close();
        }

        public override void OnStateUpdate()
        {
        }
    }
}