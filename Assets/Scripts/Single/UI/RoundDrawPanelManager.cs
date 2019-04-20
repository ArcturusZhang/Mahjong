using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;

namespace Single.UI
{
    public class RoundDrawPanelManager : MonoBehaviour
    {
        public Transform ReadySign;
        public Transform NotReadySign;
        public Transform WaitingTilesParent;

        public void Ready(Tile[] waitingTiles) {
            StartCoroutine(ShowAfterDelay(MahjongConstants.ReadyPanelDelay, true, false));
            for (int i = 0; i < WaitingTilesParent.childCount; i++) {
                var t = WaitingTilesParent.GetChild(i);
                t.gameObject.SetActive(i < waitingTiles.Length);
                if (i < waitingTiles.Length) {
                    var tileInstance = t.GetComponent<HandTileInstance>();
                    tileInstance.SetTile(waitingTiles[i]);
                }
            }
        }

        public void NotReady() {
            StartCoroutine(ShowAfterDelay(MahjongConstants.ReadyPanelDelay, false, true));
        }

        private IEnumerator ShowAfterDelay(float delay, bool ready, bool notReady) {
            yield return new WaitForSeconds(delay);
            ReadySign.gameObject.SetActive(ready);
            NotReadySign.gameObject.SetActive(notReady);
        }

        public void Close() {
            ReadySign.gameObject.SetActive(false);
            NotReadySign.gameObject.SetActive(false);
        }
    }
}
