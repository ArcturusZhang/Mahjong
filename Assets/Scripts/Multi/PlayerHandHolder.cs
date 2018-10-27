using System.Collections.Generic;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Multi
{
	public class PlayerHandHolder : MonoBehaviour
	{
		public GameObject TilePrefab;
		public int TileCount = 0;

		public void DrawingTile()
		{
			InstantiateTile(TileCount + 1);
		}

		public void DrawingTile(Tile tile)
		{
			var tileObject = InstantiateTile(TileCount + 1);
			var tileInstance = tileObject.GetComponent<TileInstance>();
			tileInstance.SetTile(tile);
		}

		public void DrawTile()
		{
			InstantiateTile(TileCount++);
		}

		public void DrawTile(Tile tile)
		{
			var tileObject = InstantiateTile(TileCount++);
			var tileInstance = tileObject.GetComponent<TileInstance>();
			tileInstance.SetTile(tile);
		}

		public void DrawTiles(int count)
		{
			for (int i = 0; i < count; i++)
			{
				InstantiateTile(TileCount++);
			}
		}

		public void DrawTiles(List<Tile> tiles)
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				var tileObject = InstantiateTile(TileCount++);
				var tileInstance = tileObject.GetComponent<TileInstance>();
				tileInstance.SetTile(tiles[i]);
			}
		}

		public void DiscardTile(bool discardLastDraw)
		{
			int index = discardLastDraw ? transform.childCount - 1 : Random.Range(0, transform.childCount - 1);
			Destroy(transform.GetChild(index).gameObject);
			if (!discardLastDraw) TileCount--;
		}

		public void Refresh(int count)
		{
			TileCount = 0;
			DeleteAllTiles();
			DrawTiles(count);
		}

		public void Refresh(List<Tile> tiles)
		{
			TileCount = 0;
			DeleteAllTiles();
			DrawTiles(tiles);
		}

		private void DeleteAllTiles()
		{
			transform.DestroyAllChild();
		}

		private GameObject InstantiateTile(int index)
		{
			var position = new Vector3(index * (MahjongConstants.TileWidth + MahjongConstants.Gap),
				MahjongConstants.TileHeight / 2, 0);
			var rotation = MahjongConstants.FacePlayer;
			var tileObject = Instantiate(TilePrefab, transform);
			tileObject.transform.localPosition = position;
			tileObject.transform.localRotation = rotation;
			tileObject.name = $"handTile{index}";
			return tileObject;
		}
	}
}
