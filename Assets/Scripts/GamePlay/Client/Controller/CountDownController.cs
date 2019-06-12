using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GamePlay.Client.Controller
{
    public class CountDownController : MonoBehaviour
    {
        public NumberPanelController NumberController;
        private WaitForSeconds wait = new WaitForSeconds(1f);
        private Coroutine currentTimerCoroutine = null;
        public bool IsCountingDown => currentTimerCoroutine != null;
        private int mTimeLeft;

        public void StartCountDown(int countDown, UnityAction callback)
        {
            if (currentTimerCoroutine != null)
            {
                StopCoroutine(currentTimerCoroutine);
                currentTimerCoroutine = null;
            }
            gameObject.SetActive(true);
            mTimeLeft = countDown;
            // SetTime(countDown);
            currentTimerCoroutine = StartCoroutine(CountDown(callback));
        }

        public int StopCountDown()
        {
            if (currentTimerCoroutine != null)
            {
                StopCoroutine(currentTimerCoroutine);
                currentTimerCoroutine = null;
            }
            NumberController.Close();
            return mTimeLeft;
        }

        private IEnumerator CountDown(UnityAction callback)
        {
            for (; mTimeLeft > 0; mTimeLeft--)
            {
                SetTime(mTimeLeft);
                yield return wait;
            }
            callback.Invoke();
            gameObject.SetActive(false);
            currentTimerCoroutine = null;
        }

        private void SetTime(int timeLeft)
        {
            if (timeLeft == 0)
            {
                NumberController.gameObject.SetActive(false);
                return;
            }
            NumberController.SetNumber(timeLeft);
        }
    }
}