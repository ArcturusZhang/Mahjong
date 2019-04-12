using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI.PointSummaryPanel
{
	public class DoraController : MonoBehaviour
	{
		public Sprite TileBack;
		public Image[] Doras;
		public Image[] UraDoras;

		private void Start()
		{
			Assert.AreEqual(Doras.Length, UraDoras.Length);
		}

		public void SetDoras(Tile[] doras, Tile[] uraDoras = null)
		{
			gameObject.SetActive(true);
			if (doras == null) return;
			for (int i = 0; i < doras.Length; i++)
			{
				Doras[i].sprite = ResourceManager.Instance.GetTileSprite(doras[i]);
			}

			if (uraDoras == null) return;
			for (int i = 0; i < uraDoras.Length; i++)
			{
				UraDoras[i].sprite = ResourceManager.Instance.GetTileSprite(uraDoras[i]);
			}
		}

		private void OnDisable()
		{
			gameObject.SetActive(false);
			for (int i = 0; i < Doras.Length; i++)
			{
				Doras[i].sprite = TileBack;
				UraDoras[i].sprite = TileBack;
			}
		}
	}
}
