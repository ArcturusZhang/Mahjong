using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Single.UI.SubManagers;
using Multi.ServerData;
using static Single.UI.SubManagers.PlayerPointTransferManager;
using UnityEngine.UI;
using Single.UI.Controller;
using UnityEngine.Events;
using Single.MahjongDataType;
using Single.Managers;

namespace Single.UI
{
    public class PointTransferManager : ManagerBase
    {
        public PlayerPointTransferManager[] SubManagers;
        public Button ConfirmButton;
        public CountDownController ConfirmCountDownController;

        private void Update()
        {
            if (CurrentRoundStatus == null) return;
            for (int placeIndex = 0; placeIndex < SubManagers.Length; placeIndex++)
            {
                int playerIndex = CurrentRoundStatus.GetPlayerIndex(placeIndex);
                if (IsValidPlayer(playerIndex))
                {
                    SubManagers[placeIndex].gameObject.SetActive(true);
                    SubManagers[placeIndex].PlayerName = CurrentRoundStatus.GetPlayerName(placeIndex);
                }
                else
                    SubManagers[placeIndex].gameObject.SetActive(false);
            }
        }

        public void SetTransfer(int[] points, IList<PointTransfer> transfers, UnityAction callback)
        {
            gameObject.SetActive(true);
            for (int placeIndex = 0; placeIndex < SubManagers.Length; placeIndex++)
            {
                int playerIndex = CurrentRoundStatus.GetPlayerIndex(placeIndex);
                if (!IsValidPlayer(playerIndex)) continue;
                // get corresponding transfers
                var localTransfers = new List<Transfer>();
                foreach (var transfer in transfers)
                {
                    if (transfer.From == playerIndex)
                    {
                        localTransfers.Add(new Transfer
                        {
                            Type = GetTransferType(transfer.From, transfer.To),
                            Amount = -transfer.Amount
                        });
                    }
                    if (transfer.To == playerIndex)
                    {
                        localTransfers.Add(new Transfer
                        {
                            Type = Type.None,
                            Amount = transfer.Amount
                        });
                    }
                }
                SubManagers[placeIndex].SetTransfers(points[placeIndex], localTransfers);
            }
            // count down
            ConfirmButton.onClick.RemoveAllListeners();
            ConfirmButton.onClick.AddListener(() =>
            {
                callback();
                ConfirmCountDownController.StopCountDown();
            });
            ConfirmCountDownController.StartCountDown(MahjongConstants.SummaryPanelWaitingTime, callback);
            // todo -- places animation
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private static Type GetTransferType(int from, int to)
        {
            if (from < 0) return Type.None;
            int diff = to - from;
            if (diff < 0) diff += 4;
            switch (diff)
            {
                case 1:
                    return Type.Right;
                case 2:
                    return Type.Straight;
                case 3:
                    return Type.Left;
                default:
                    return Type.None;
            }
        }

        private void OnDisable()
        {
            foreach (var manager in SubManagers)
            {
                manager.gameObject.SetActive(false);
            }
        }

        private bool IsValidPlayer(int index)
        {
            return index >= 0 && index < CurrentRoundStatus.TotalPlayers;
        }
    }
}
