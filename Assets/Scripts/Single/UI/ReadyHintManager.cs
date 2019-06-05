using System.Collections.Generic;
using Single.MahjongDataType;
using Single.MahjongDataType.Interfaces;
using Single.UI.SubManagers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Single.UI
{
    public class ReadyHintManager : MonoBehaviour, IObserver<ClientRoundStatus>, IPointerEnterHandler, IPointerExitHandler
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
