using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using Single.UI.Elements;
using UnityEngine;

namespace Single.UI
{
    public class WaitingPanelManager : MonoBehaviour
    {
        public Transform ReadySign;
        public Transform NotReadySign;
        public Transform WaitingTilesParent;
        private SimpleTile[] instances;

        private void OnEnable()
        {
            if (instances == null || instances.Length < WaitingTilesParent.childCount)
                instances = new SimpleTile[WaitingTilesParent.childCount];
            for (int i = 0; i < WaitingTilesParent.childCount; i++)
            {
                var t = WaitingTilesParent.GetChild(i);
                instances[i] = t.GetComponent<SimpleTile>();
            }
        }

        public void Ready(Tile[] waitingTiles)
        {
            for (int i = 0; i < WaitingTilesParent.childCount; i++)
            {
                var instance = instances[i];
                instance.gameObject.SetActive(i < waitingTiles.Length);
                if (i < waitingTiles.Length)
                {
                    instance.SetTile(waitingTiles[i]);
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
            NotReadySign.gameObject.SetActive(!ready);
        }

        public void Close()
        {
            ReadySign.gameObject.SetActive(false);
            NotReadySign.gameObject.SetActive(false);
        }
    }
}
