using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Common.Interfaces;
using Mahjong.Model;
using GamePlay.Client.Model;
using GamePlay.Client.View.SubManagers;

namespace GamePlay.Client.View
{
    public class ReadyHintManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IObserver<ClientRoundStatus>
    {
        public Image ReadyImage;
        public DiscardHintManager DiscardHintManager;
        private IList<Tile> waitingList;

        public void OnPointerEnter(PointerEventData eventData)
        {
            DiscardHintManager.SetWaitingTiles(waitingList);
            DiscardHintManager.Show();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DiscardHintManager.Close();
        }

        public void UpdateStatus(ClientRoundStatus subject)
        {
            if (subject == null) return;
            waitingList = subject.WaitingTiles;
            if (waitingList != null && waitingList.Count > 0)
                ReadyImage.gameObject.SetActive(true);
            else
                ReadyImage.gameObject.SetActive(false);
        }
    }
}
