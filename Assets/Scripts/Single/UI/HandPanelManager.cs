using System.Collections.Generic;
using Single.MahjongDataType;
using Single.Managers;
using Single.UI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class HandPanelManager : ManagerBase
    {
        private const float Width = 64;
        private const float LastDrawPositionX = 425;
        public HandTileInstance[] HandTileInstances;
        public HandTileInstance LastDrawInstance;
        public RectTransform LastDrawRect;

        private void Update()
        {
            if (CurrentRoundStatus == null) return;
            var count = ShowHandTiles();
            ShowLastDraw(count);
        }

        private int ShowHandTiles()
        {
            var tiles = CurrentRoundStatus.LocalPlayerHandTiles;
            int length = tiles == null ? 0 : tiles.Count;
            for (int i = 0; i < length; i++)
            {
                HandTileInstances[i].gameObject.SetActive(true);
                HandTileInstances[i].SetTile(tiles[i]);
            }
            for (int i = length; i < HandTileInstances.Length; i++)
            {
                HandTileInstances[i].gameObject.SetActive(false);
            }
            return length;
        }

        private void ShowLastDraw(int count)
        {
            var lastDraw = CurrentRoundStatus.GetLastDraw(0);
            if (lastDraw == null)
                LastDrawInstance.gameObject.SetActive(false);
            else
            {
                LastDrawInstance.SetTile((Tile)lastDraw);
                LastDrawInstance.gameObject.SetActive(true);
            }
            int diff = HandTileInstances.Length - count;
            LastDrawRect.anchoredPosition = new Vector2(LastDrawPositionX - diff * Width, 0);
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

        public void LockTiles()
        {
            for (int i = 0; i < HandTileInstances.Length; i++)
            {
                HandTileInstances[i].Lock();
            }
        }

        public void UnlockTiles()
        {
            for (int i = 0; i < HandTileInstances.Length; i++)
            {
                HandTileInstances[i].Unlock();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
