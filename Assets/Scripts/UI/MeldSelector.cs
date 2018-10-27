using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace UI
{
	public class MeldSelector : MonoBehaviour
	{
		public GameObject MeldPrefab;

		public void AddMelds(IEnumerable<Meld> melds, UnityAction<Meld> callback)
		{
			foreach (var meld in melds)
			{
				var obj = Instantiate(MeldPrefab, transform);
				var meldVisualizer = obj.GetComponent<MeldVisualizer>();
				meldVisualizer.SetMeld(meld, callback);
			}
		}

		public void ResetMelds()
		{
			transform.DestroyAllChild();
		}
	}
}
