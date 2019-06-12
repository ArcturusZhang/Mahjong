using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GamePlay.Client.Controller
{
    public class TimerController : MonoBehaviour
    {
        public Image PlusImage;
        public NumberPanelController BaseTimeController;
        public NumberPanelController BonusTimeController;
        private Coroutine currentTimerCoroutine = null;
        private WaitForSeconds wait = new WaitForSeconds(1f);
        private int mBaseTime;
        private int mBonusTime;

        public bool IsCountingDown => currentTimerCoroutine != null;

        /// <summary>
        /// Starts count down with the given time, invoke callback when time expires.
        /// </summary>
        /// <param name="baseTime"></param>
        /// <param name="bonusTime"></param>
        /// <param name="callback"></param>
        public void StartCountDown(int baseTime, int bonusTime, UnityAction callback)
        {
            if (currentTimerCoroutine != null)
            {
                StopCoroutine(currentTimerCoroutine);
                currentTimerCoroutine = null;
            }
            gameObject.SetActive(true);
            mBaseTime = baseTime;
            mBonusTime = bonusTime;
            SetTime(mBaseTime, mBonusTime);
            currentTimerCoroutine = StartCoroutine(CountDown(callback));
        }

        /// <summary>
        /// Stops the count down immediately, return the bonus turn time left.
        /// </summary>
        /// <returns>The bonus time left</returns>
        public int StopCountDown()
        {
            if (currentTimerCoroutine != null)
            {
                StopCoroutine(currentTimerCoroutine);
                currentTimerCoroutine = null;
            }
            gameObject.SetActive(false);
            return mBonusTime;
        }

        private IEnumerator CountDown(UnityAction callback)
        {
            for (; mBaseTime > 0; mBaseTime--)
            {
                SetTime(mBaseTime, mBonusTime);
                yield return wait;
            }

            if (mBonusTime > 0)
                for (; mBonusTime >= 0; mBonusTime--)
                {
                    SetTime(mBaseTime, mBonusTime);
                    yield return wait;
                }

            callback.Invoke();
            gameObject.SetActive(false);
            currentTimerCoroutine = null;
        }

        private void SetTime(int baseTime, int bonusTime)
        {
            if (baseTime < 0) baseTime = 0;

            if (bonusTime < 0) bonusTime = 0;

            if (baseTime == 0)
            {
                BaseTimeController.gameObject.SetActive(false);
                PlusImage.gameObject.SetActive(false);
            }
            else
            {
                BaseTimeController.gameObject.SetActive(true);
                PlusImage.gameObject.SetActive(true);
                BaseTimeController.SetNumber(baseTime);
            }

            if (bonusTime == 0)
            {
                PlusImage.gameObject.SetActive(false);
                BonusTimeController.gameObject.SetActive(false);
                return;
            }
            BonusTimeController.SetNumber(bonusTime);
        }
    }
}