using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Single.UI.SubManagers;
using Multi.ServerData;
using static Single.UI.SubManagers.PlayerPointTransferManager;
using UnityEngine.UI;
using Single.UI.Controller;
using UnityEngine.Events;

namespace Single.UI
{
    public class PointTransferManager : MonoBehaviour
    {
        public PlayerPointTransferManager[] SubManagers;
        public Button ConfirmButton;
        public CountDownController ConfirmCountDownController;
        [HideInInspector] public int TotalPlayers;
        [HideInInspector] public int[] Places;
        [HideInInspector] public string[] PlayerNames;

        private void Update()
        {
            for (int i = 0; i < PlayerNames.Length; i++)
            {
                if (IsValidPlayer(Places[i]))
                {
                    SubManagers[i].PlayerName = PlayerNames[i];
                }
                else
                    SubManagers[i].gameObject.SetActive(false);
            }
        }

        public void SetTransfer(int[] points, IList<PointTransfer> transfers, UnityAction callback)
        {
            gameObject.SetActive(true);
            for (int i = 0; i < SubManagers.Length; i++)
            {
                if (!IsValidPlayer(Places[i])) continue;
                // get corresponding transfers
                var localTransfers = new List<Transfer>();
                foreach (var transfer in transfers)
                {
                    if (transfer.From == Places[i])
                    {
                        localTransfers.Add(new Transfer
                        {
                            Type = GetTransferType(transfer.From, transfer.To),
                            Amount = -transfer.Amount
                        });
                    }
                    if (transfer.To == Places[i])
                    {
                        localTransfers.Add(new Transfer
                        {
                            Type = Type.None,
                            Amount = transfer.Amount
                        });
                    }
                }
                SubManagers[i].SetTransfers(points[i], localTransfers);
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
            return index >= 0 && index < TotalPlayers;
        }
    }
}
