using UnityEngine;

namespace UI.PointSummaryPanel
{
	public class YakuPointPanelController : MonoBehaviour
	{
		public NumberPanelController NumberPanelController;
		public void SetPoint(int point)
		{
			gameObject.SetActive(true);
			NumberPanelController.SetNumber(point);
		}
	}
}
