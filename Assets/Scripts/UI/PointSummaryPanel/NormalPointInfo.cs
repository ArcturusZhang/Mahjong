using Single;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.PointSummaryPanel
{
	public class NormalPointInfo : MonoBehaviour
	{
		public NumberPanelController FanController;
		public NumberPanelController FuController;
//		public Image[] Fans;
//		public Image[] Fus;

		public void SetPointInfo(int fan, int fu)
		{
			// Set fan
			FanController.SetNumber(fan);
			// Set fu
			FuController.SetNumber(fu);
		}
	}
}
