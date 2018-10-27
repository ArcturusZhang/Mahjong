using Single;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.PointSummaryPanel
{
	public class NormalPointInfo : MonoBehaviour
	{
		public Image[] Fans;
		public Image[] Fus;

		public void SetPointInfo(int fan, int fu)
		{
			// Set fan
			Fans.SetNumber(fan, ResourceManager.Instance.FanNumber);
			// Set fu
			Fus.SetNumber(fu, ResourceManager.Instance.FuNumber);
		}
	}
}
