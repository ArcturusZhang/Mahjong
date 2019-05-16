using System.Collections.Generic;
using Single.MahjongDataType;
using Single.Managers;
using Single.UI.Elements;
using Single.UI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class HandPanelManager : ManagerBase
    {
        private const float Width = 64;
        private const float DefaultLastDrawX = 425;
        [SerializeField] private HandTile[] handTiles;
        [SerializeField] private HandTile lastDrawTile;
        public RectTransform LastDrawRect;
        public Image Zhenting;

        private void Update()
        {
            if (CurrentRoundStatus == null) return;
            var count = ShowHandTiles();
            ShowLastDraw(count);
            Zhenting.gameObject.SetActive(CurrentRoundStatus.IsZhenting);
        }

        private int ShowHandTiles()
        {
            var tiles = CurrentRoundStatus.LocalPlayerHandTiles;
            int length = tiles == null ? 0 : tiles.Count;
            for (int i = 0; i < length; i++)
            {
                handTiles[i].gameObject.SetActive(true);
                handTiles[i].SetTile(tiles[i]);
            }
            for (int i = length; i < handTiles.Length; i++)
            {
                handTiles[i].gameObject.SetActive(false);
            }
            return length;
        }

        private void ShowLastDraw(int count)
        {
            var lastDraw = CurrentRoundStatus.GetLastDraw(0);
            if (lastDraw == null)
                lastDrawTile.gameObject.SetActive(false);
            else
            {
                lastDrawTile.SetTile((Tile)lastDraw);
                lastDrawTile.gameObject.SetActive(true);
            }
            int diff = handTiles.Length - count;
            LastDrawRect.anchoredPosition = new Vector2(DefaultLastDrawX - diff * Width, 0);
        }

        public void SetCandidates(IList<Tile> candidates)
        {
            for (int i = 0; i < handTiles.Length; i++)
            {
                SetCandidate(handTiles[i], candidates);
            }
            SetCandidate(lastDrawTile, candidates);
        }

        public void RemoveCandidates()
        {
            for (int i = 0; i < handTiles.Length; i++)
            {
                RemoveCandidate(handTiles[i]);
            }
            RemoveCandidate(lastDrawTile);
        }

        private void SetCandidate(HandTile instance, IList<Tile> candidates)
        {
            // if (!instance.gameObject.activeSelf) return;
            var tile = instance.Tile;
            if (candidates.Contains(tile))
                instance.TurnOn();
            else
                instance.TurnOff();
        }

        private void RemoveCandidate(HandTile instance)
        {
            instance.TurnOn();
        }

        public void LockTiles()
        {
            for (int i = 0; i < handTiles.Length; i++)
            {
                handTiles[i].SetLock(true);
            }
        }

        public void UnlockTiles()
        {
            for (int i = 0; i < handTiles.Length; i++)
            {
                handTiles[i].SetLock(false);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
