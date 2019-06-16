using System.Collections.Generic;
using System.Linq;
using GamePlay.Client.Model;
using GamePlay.Server.Model;
using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Client.Controller.GameState
{
    public class PlayerRongState : ClientState
    {
        public int[] RongPlayerIndices;
        public string[] RongPlayerNames;
        public PlayerHandData[] HandData;
        public Tile WinningTile;
        public Tile[] DoraIndicators;
        public Tile[] UraDoraIndicators;
        public bool[] RongPlayerRichiStatus;
        public NetworkPointInfo[] RongPointInfos;
        public int[] TotalPoints;

        public override void OnClientStateEnter()
        {
            var indices = RongPlayerIndices.Select((playerIndex, index) => index).ToArray();
            var dataArray = indices.Select(index => new SummaryPanelData
            {
                HandInfo = new PlayerHandInfo
                {
                    HandTiles = HandData[index].HandTiles,
                    OpenMelds = HandData[index].OpenMelds,
                    WinningTile = WinningTile,
                    DoraIndicators = DoraIndicators,
                    UraDoraIndicators = UraDoraIndicators,
                    IsRichi = RongPlayerRichiStatus[index],
                    IsTsumo = false
                },
                PointInfo = new PointInfo(RongPointInfos[index]),
                TotalPoints = TotalPoints[index],
                PlayerName = RongPlayerNames[index]
            });
            var dataQueue = new Queue<SummaryPanelData>(dataArray);
            ShowRongPanel(dataQueue);
        }

        private void ShowRongPanel(Queue<SummaryPanelData> queue)
        {
            if (queue.Count > 0)
            {
                // show panel for this data
                var data = queue.Dequeue();
                controller.PointSummaryPanelManager.ShowPanel(data, () => ShowRongPanel(queue));
            }
            else
            {
                // no more data to show
                Debug.Log("Sending readiness message");
                controller.PointSummaryPanelManager.StopCountDown();
                ClientBehaviour.Instance.ClientReady();
            }
        }

        public override void OnClientStateExit()
        {
            controller.PointSummaryPanelManager.Close();
        }

        public override void OnStateUpdate()
        {
        }
    }
}