using Multi;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
	[RequireComponent(typeof(MeshRenderer))]
	public class TileInstance : MonoBehaviour
	{
		public Tile Tile;

		public void SetTile(Tile tile)
		{
			Tile = tile;
			var material = GetComponent<MeshRenderer>().material;
			material.mainTexture = MahjongConstants.GetTileTexture(Tile);
		}
	}
}
