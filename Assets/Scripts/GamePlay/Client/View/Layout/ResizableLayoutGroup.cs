using System;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.Layout
{
    public class ResizableLayoutGroup : MonoBehaviour, ILayoutGroup
    {
        [Header("Layout settings")]
        public TextAlignment TextAlignment;
        public Vector2 BestElementSize;
        public int BestThreshold;
        private Vector2 size;

        private void OnEnable()
        {
            size = new Vector2(BestElementSize.x * BestThreshold, BestElementSize.y);
            ((RectTransform)transform).sizeDelta = size;
        }

        public void SetLayoutHorizontal()
        {
            float scaleFactor = Mathf.Min(1, (float)BestThreshold / transform.childCount);
            float offset = GetCurrentOffset(TextAlignment, scaleFactor);
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = (RectTransform)transform.GetChild(i);
                child.sizeDelta = BestElementSize * scaleFactor;
                child.anchoredPosition = new Vector2(offset, 0);
                offset += child.sizeDelta.x;
            }
        }

        private float GetCurrentOffset(TextAlignment alignment, float scaleFactor)
        {
            switch (alignment)
            {
                case TextAlignment.Left:
                    return BestElementSize.x * scaleFactor / 2;
                case TextAlignment.Center:
                    return (size.x - transform.childCount * BestElementSize.x * scaleFactor) / 2 +
                           BestElementSize.x / 2 * scaleFactor;
                case TextAlignment.Right:
                    return size.x - transform.childCount * BestElementSize.x * scaleFactor +
                           BestElementSize.x / 2 * scaleFactor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
        }

        public void SetLayoutVertical()
        {
        }
    }
}
