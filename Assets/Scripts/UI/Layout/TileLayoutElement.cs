using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Layout
{
	public class TileLayoutElement : MonoBehaviour, ILayoutElement
	{
		public float Width;
		public float Height;
		public bool Drawing;
		public Tile Tile;
		
		public void CalculateLayoutInputHorizontal()
		{
			// I do not need this
		}

		public void CalculateLayoutInputVertical()
		{
			// I do not need this
		}

		public float minWidth { get; }
		public float preferredWidth { get; }
		public float flexibleWidth { get; }
		public float minHeight { get; }
		public float preferredHeight { get; }
		public float flexibleHeight { get; }
		public int layoutPriority { get; }
	}
}
