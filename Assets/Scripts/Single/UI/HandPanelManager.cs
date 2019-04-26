using System.Collections.Generic;
using Single.MahjongDataType;
using Single.UI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class HandPanelManager : MonoBehaviour
    {
        public HandTileInstance[] HandTileInstances;
        public HandTileInstance LastDrawInstance;
        [HideInInspector] public List<Tile> Tiles;
        [HideInInspector] public Tile? LastDraw;

        private void Update()
        {
            ShowHandTiles();
            ShowLastDraw();
        }

        private void ShowHandTiles()
        {
            int length = Tiles == null ? 0 : Tiles.Count;
            for (int i = 0; i < length; i++)
            {
                HandTileInstances[i].gameObject.SetActive(true);
                HandTileInstances[i].SetTile(Tiles[i]);
            }
            for (int i = length; i < HandTileInstances.Length; i++)
            {
                HandTileInstances[i].gameObject.SetActive(false);
            }
        }

        private void ShowLastDraw()
        {
            if (LastDraw == null)
                LastDrawInstance.gameObject.SetActive(false);
            else
            {
                LastDrawInstance.SetTile((Tile)LastDraw);
                LastDrawInstance.gameObject.SetActive(true);
            }
        }

        public void SetCandidates(IList<Tile> candidates)
        {
            for (int i = 0; i < HandTileInstances.Length; i++)
            {
                SetCandidate(HandTileInstances[i], candidates);
            }
            SetCandidate(LastDrawInstance, candidates);
        }

        public void RemoveCandidates()
        {
            for (int i = 0; i < HandTileInstances.Length; i++)
            {
                RemoveCandidate(HandTileInstances[i]);
            }
            RemoveCandidate(LastDrawInstance);
        }

        private void SetCandidate(HandTileInstance instance, IList<Tile> candidates)
        {
            var tile = instance.Tile;
            instance.SetAvailable(candidates.Contains(tile));
        }

        private void RemoveCandidate(HandTileInstance instance)
        {
            instance.SetAvailable(true);
        }

        public void LockTiles() {
            for (int i = 0; i < HandTileInstances.Length; i++) {
                HandTileInstances[i].Lock();
            }
        }

        public void UnlockTiles() {
            for (int i = 0; i < HandTileInstances.Length; i++) {
                HandTileInstances[i].Unlock();
            }
        }
    }
}
