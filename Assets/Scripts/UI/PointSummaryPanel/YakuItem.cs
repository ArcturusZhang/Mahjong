using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Debug = System.Diagnostics.Debug;

namespace UI.PointSummaryPanel
{
	public class YakuItem : MonoBehaviour
	{
		public Image Normal;
		public Image YakuMan;
		public Image[] Fans;
		public Text YakuName;
		
		public void SetYakuItem(YakuValue yaku, bool 青天井 = false)
		{
			Debug.Assert(YakuName != null, nameof(YakuName) + " != null");
			YakuName.text = yaku.Name;
			if (yaku.Type == YakuType.Normal || 青天井)
			{
				// use normal display
				Normal.gameObject.SetActive(true);
				YakuMan.gameObject.SetActive(false);
				int value = yaku.Type == YakuType.Normal ? yaku.Value : yaku.Value * YakuSettings.YakumanBaseFan;
				Fans.SetNumber(value, ResourceManager.Instance.FanNumber);
			}
			else
			{
				// use yaku man display
				Normal.gameObject.SetActive(false);
				YakuMan.gameObject.SetActive(true);
			}
		}
	}
}
