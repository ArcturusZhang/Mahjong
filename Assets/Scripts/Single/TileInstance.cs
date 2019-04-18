using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
	[RequireComponent(typeof(MeshRenderer))]
	public class TileInstance : MonoBehaviour
	{
		public Tile Tile;

		private MeshRenderer meshRenderer;

		private void Awake()
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}

		public void SetTile(Tile tile)
		{
			Tile = tile;
			var material = meshRenderer.material;
			material.mainTexture = ResourceManager.Instance?.GetTileTexture(tile);
		}
	}
}
