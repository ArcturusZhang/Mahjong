using System.Collections;
using Single;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PointSummaryPanel
{
	public class ConfirmButtonController : MonoBehaviour
	{
		public Button Button;
		public Image CountDownImage;
		
		private readonly WaitForSeconds wait = new WaitForSeconds(1f);

		private void OnDisable()
		{
			gameObject.SetActive(false);
		}

		public IEnumerator StartCountDown(int countDown, UnityAction callback)
		{
			gameObject.SetActive(true);
			Button.interactable = true;
			Button.onClick.AddListener(callback);
			while (countDown > 0)
			{
				CountDownImage.sprite = ResourceManager.Instance.BonusTime(Mathf.CeilToInt(countDown));
				yield return wait;
				countDown--;
			}
			callback.Invoke();
		}
	}
}
