using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Mahjong.Model;
using GamePlay.Client.View.Elements;

namespace GamePlay.Client.View
{
    public class WaitingPanelManager : MonoBehaviour
    {
        public Transform ReadySign;
        public Image ReadyImage;
        public SimpleTile[] ReadyTiles;
        public Transform NotReadySign;
        public Image NotReadyImage;
        public Transform WaitingTilesParent;

        public void Ready(Tile[] waitingTiles)
        {
            for (int i = 0; i < WaitingTilesParent.childCount; i++)
            {
                var instance = ReadyTiles[i];
                instance.gameObject.SetActive(i < waitingTiles.Length);
                if (i < waitingTiles.Length)
                {
                    instance.SetTile(waitingTiles[i]);
                    var image = instance.GetComponent<Image>();
                    image.color = new Color(1, 1, 1, 0);
                    image.DOFade(1, AnimationDuration);
                }
            }
            ShowPanel(true);
        }

        public void NotReady()
        {
            ShowPanel(false);
        }

        private void ShowPanel(bool ready)
        {
            ReadySign.gameObject.SetActive(ready);
            if (ready)
            {
                ReadyImage.color = new Color(1, 1, 1, 0);
                ReadyImage.DOFade(1, AnimationDuration);
            }
            NotReadySign.gameObject.SetActive(!ready);
            if (!ready)
            {
                NotReadyImage.color = new Color(1, 1, 1, 0);
                NotReadyImage.DOFade(1, AnimationDuration);
            }
        }

        private const float AnimationDuration = 0.5f;

        public void Close()
        {
            ReadySign.gameObject.SetActive(false);
            NotReadySign.gameObject.SetActive(false);
        }
    }
}
