using System.Collections;
using GamePlay.Client.Model;
using GamePlay.Client.View;
using GamePlay.Server.Model;
using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Client.Controller.GameState
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
            CurrentRoundStatus.SetCurrentPlaceIndex(TsumoPlayerIndex);
            var placeIndex = CurrentRoundStatus.CurrentPlaceIndex;
            CurrentRoundStatus.DrawTile(placeIndex, WinningTile);
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
            var duration = controller.ShowEffect(placeIndex, PlayerEffectManager.Type.Tsumo);
            yield return new WaitForSeconds(duration);
            controller.PointSummaryPanelManager.ShowPanel(data, () =>
            {
                Debug.Log("Sending readiness message");
                ClientBehaviour.Instance.ClientReady();
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