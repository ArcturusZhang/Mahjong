using System.Collections.Generic;
using Common.Interfaces;
using GamePlay.Client.Model;
using GamePlay.Client.View.Elements;
using GamePlay.Client.View.SubManagers;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View
{
    public class HandPanelManager : MonoBehaviour, IObserver<ClientRoundStatus>
    {
        private const float Width = 64;
        private const float DefaultLastDrawX = 425;
        [SerializeField] private HandTile[] handTiles;
        [SerializeField] private HandTile lastDrawTile;
        public RectTransform LastDrawRect;
        public Image Zhenting;
        public DiscardHintManager DiscardHintManager;

        public IList<HandTile> HandTiles => handTiles;
        public HandTile LastDrawTile => lastDrawTile;

        private int ShowHandTiles(ClientRoundStatus status)
        {
            var tiles = status.LocalPlayerHandTiles;
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

        private void ShowLastDraw(int count, ClientRoundStatus status)
        {
            var lastDraw = status.GetLastDraw(0);
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

        private void ToggleForbiddens(ClientRoundStatus status)
        {
            if (status.ForbiddenTiles == null)
            {
                RemoveCandidates();
                return;
            }
            var forbiddens = status.ForbiddenTiles;
            for (int i = 0; i < handTiles.Length; i++)
            {
                var instance = handTiles[i];
                if (!instance.interactable) continue;
                if (forbiddens.Contains(instance.Tile)) instance.TurnOff();
            }
            if (lastDrawTile.interactable && forbiddens.Contains(lastDrawTile.Tile)) lastDrawTile.TurnOff();
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

        public void UpdateStatus(ClientRoundStatus subject)
        {
            if (subject == null) return;
            var count = ShowHandTiles(subject);
            ShowLastDraw(count, subject);
            ToggleForbiddens(subject);
            Zhenting.gameObject.SetActive(subject.IsZhenting);
            if (subject.PossibleWaitingTiles == null)
                DiscardHintManager.Close();
        }
    }
}
