using Single;
using Single.MahjongDataType;
using UnityEngine;

namespace Multi
{
	public class PlayerOpenHolder : MonoBehaviour
	{
		public MahjongSelector MahjongSelector;

		private float offset = 0f;
		
		public void Open(Meld meld, Tile discardTile, MeldInstanceType instanceType)
		{
			var prefab = MahjongSelector.PrefabDict[instanceType];
			var meldObject = Instantiate(prefab, transform);
			meldObject.transform.localPosition = new Vector3(0, 0, offset);
			meldObject.transform.localRotation = Quaternion.identity;
			var meldInstance = meldObject.GetComponent<MeldInstance>();
			meldInstance.SetMeld(meld, discardTile);
			offset -= meldInstance.MeldWidth + MahjongConstants.Gap;
		}
	}
}
