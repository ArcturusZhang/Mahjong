using System;
using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using Single.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Single.UI.Elements
{
    [RequireComponent(typeof(Image))]
    public class HandTile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsLastDraw;
        public bool interactable = true;
        private Image image;
        private RectTransform rect;
        private Tile tile;
        private bool locked = false;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0);
        }

        public Tile Tile => tile;

        public void SetTile(Tile tile)
        {
            gameObject.SetActive(true);
            this.tile = tile;
            var sprite = ResourceManager.Instance.GetTileSprite(tile);
            if (image == null) image = GetComponent<Image>();
            image.sprite = sprite;
        }

        public void TurnOff()
        {
            interactable = false;
            rect.DOAnchorPosY(0, AnimationDuration);
            image.DOColor(Color.gray, AnimationDuration);
        }

        public void TurnOn()
        {
            interactable = true;
            image.DOColor(Color.white, AnimationDuration);
        }

        public void SetLock(bool locked)
        {
            this.locked = locked;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable || locked) return;
            ClientBehaviour.Instance.OnDiscardTile(tile, IsLastDraw);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            rect.DOAnchorPosY(20, AnimationDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable) return;
            rect.DOAnchorPosY(0, AnimationDuration);
        }

        private const float AnimationDuration = 0.5f;
    }
}
