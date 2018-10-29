using System.Collections.Generic;
using Single;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Layout
{
    public class HorizontalTileLayout : MonoBehaviour, ILayoutGroup
    {
        private readonly Queue<ElementGroup> queue = new Queue<ElementGroup>();

        public void SetLayoutHorizontal()
        {
            float xOffset = 0;
            for (int x = 0; x < transform.childCount; x++)
            {
                var childTransform = (RectTransform) transform.GetChild(x);
                var layoutElement = childTransform.GetComponent<TileLayoutElement>();
                if (layoutElement == null) continue;
                if (layoutElement.Drawing)
                {
                    queue.Enqueue(new ElementGroup {rectTransform = childTransform, layoutElement = layoutElement});
                    continue;
                }

                childTransform.anchoredPosition = new Vector2(xOffset, childTransform.anchoredPosition.y);
                childTransform.sizeDelta = new Vector2(layoutElement.Width, layoutElement.Height);
                xOffset += layoutElement.Width;
            }

            xOffset += MahjongConstants.UiGap;
            while (queue.Count > 0)
            {
                var group = queue.Dequeue();
                var childTransform = group.rectTransform;
                var layoutElement = group.layoutElement;
                childTransform.anchoredPosition = new Vector2(xOffset, childTransform.anchoredPosition.y);
                childTransform.sizeDelta = new Vector2(layoutElement.Width, layoutElement.Height);
                xOffset += layoutElement.Width;
            }
        }

        public void SetLayoutVertical()
        {
            // do nothing
        }

        private struct ElementGroup
        {
            internal RectTransform rectTransform;
            internal TileLayoutElement layoutElement;
        }
    }
}