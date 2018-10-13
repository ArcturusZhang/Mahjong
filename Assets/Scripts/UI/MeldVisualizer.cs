using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class MeldVisualizer : MonoBehaviour, IPointerClickHandler
	{
		public Image Tile1;
		public Image Tile2;
		public Image Tile3;

		private Meld mMeld;
		private UnityAction<Meld> mCallback;

		public void SetMeld(Meld meld, UnityAction<Meld> callback)
		{
			SetMeld(meld);
			mCallback = callback;
		}

		private void SetMeld(Meld meld)
		{
			mMeld = meld;
			var list = meld.Tiles;
			Tile1.sprite = ResourceManager.Instance.GetTileSprite(list[0]);
			Tile2.sprite = ResourceManager.Instance.GetTileSprite(list[1]);
			Tile3.sprite = ResourceManager.Instance.GetTileSprite(list[2]);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Debug.Log($"Meld {mMeld} is clicked");
			if (mCallback == null)
			{
				Debug.LogError("Callback is missing!");
				return;
			}
			mCallback.Invoke(mMeld);
		}
	}
}
