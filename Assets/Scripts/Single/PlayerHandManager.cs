using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Debug = Single.Debug;

namespace Single
{
    public class PlayerHandManager : MonoBehaviour
    {
        public Transform handHolder;
        public Transform drawnHolder;
        [HideInInspector] public int Count;
        [HideInInspector] public List<Tile> Tiles = null;
        [HideInInspector] public Tile? LastDraw = null;
        private bool discarding = false;
        private WaitForSeconds discardingWait = new WaitForSeconds(MahjongConstants.PlayerHandTilesSortDelay);

        private void Update()
        {
            if (!discarding)
            {
                HoldTiles();
                LastDrawTile();
            }
        }

        private void HoldTiles()
        {
            if (Count > handHolder.childCount)
            {
                Debug.LogWarning($"Not enough tiles to show, cap to {handHolder.childCount}", false);
            }
            for (int i = 0; i < Count; i++)
            {
                handHolder.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = Count; i < handHolder.childCount; i++)
            {
                handHolder.GetChild(i).gameObject.SetActive(false);
            }
            if (Tiles == null) return;
            for (int i = 0; i < Tiles.Count; i++)
            {
                handHolder.GetChild(i).GetComponent<TileInstance>().SetTile(Tiles[i]);
            }
        }

        private void LastDrawTile()
        {
            if (LastDraw == null)
            {
                drawnHolder.gameObject.SetActive(false);
                return;
            }
            drawnHolder.gameObject.SetActive(true);
            drawnHolder.transform.localPosition = new Vector3(Count * MahjongConstants.HandTileWidth + MahjongConstants.LastDrawGap, 0, 0);
        }

        public void DiscardTile(bool discardingLastDraw)
        {
            discarding = true;
            if (discardingLastDraw) drawnHolder.gameObject.SetActive(false);
            else {
                int tileIndex = Random.Range(0, Count);
                handHolder.GetChild(tileIndex).gameObject.SetActive(false);
            }
            StartCoroutine(StopDiscarding());
        }

        private IEnumerator StopDiscarding() {
            yield return discardingWait;
            discarding = false;
        }
    }
}
