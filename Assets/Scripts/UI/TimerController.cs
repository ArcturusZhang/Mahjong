using System.Collections;
using Single;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class TimerController : MonoBehaviour
    {
        public Image BaseTimeImage;
        public Image PlusImage;
        public Image[] BonusTimeImages;

        private Coroutine currentTimerCoroutine;

        private WaitForSeconds wait;

        private int mBaseTime;
        private int mBonusTime;

        private void Awake()
        {
            wait = new WaitForSeconds(1f);
        }

        public void StartCountDown(int baseTime, int bonusTime, UnityAction callback)
        {
            if (currentTimerCoroutine != null)
            {
                StopCoroutine(currentTimerCoroutine);
                currentTimerCoroutine = null;
            }
            mBaseTime = baseTime;
            mBonusTime = bonusTime;
            SetTime(mBaseTime, mBonusTime);
            currentTimerCoroutine = StartCoroutine(CountDown(callback));
        }

        public int StopCountDown()
        {
            if (currentTimerCoroutine != null)
            {
                StopCoroutine(currentTimerCoroutine);
                currentTimerCoroutine = null;
            }
            DisableVisualElements();
            return mBonusTime;
        }

        private void DisableVisualElements()
        {
            BaseTimeImage.gameObject.SetActive(false);
            PlusImage.gameObject.SetActive(false);
            foreach (var bonusTimeImage in BonusTimeImages)
            {
                bonusTimeImage.gameObject.SetActive(false);
            }
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
            DisableVisualElements();
        }

        private void SetTime(int baseTime, int bonusTime)
        {
            if (baseTime > 9)
            {
                Debug.LogWarning("Base time cannot be larger than 9");
                baseTime = 9;
            }

            if (baseTime < 0) baseTime = 0;

            if (bonusTime > 99)
            {
                Debug.LogWarning("Bonus time cannot be larger than 99");
            }

            if (bonusTime < 0) bonusTime = 0;

            if (baseTime == 0)
            {
                BaseTimeImage.gameObject.SetActive(false);
                PlusImage.gameObject.SetActive(false);
            }
            else
            {
                BaseTimeImage.gameObject.SetActive(true);
                PlusImage.gameObject.SetActive(true);
                var sprite = ResourceManager.Instance.BaseTime(baseTime);
                BaseTimeImage.sprite = sprite;
            }

            if (bonusTime == 0)
            {
                PlusImage.gameObject.SetActive(false);
                BonusTimeImages[0].gameObject.SetActive(false);
                BonusTimeImages[1].gameObject.SetActive(false);
                return;
            }

            int first = bonusTime / 10;
            int second = bonusTime % 10;

            if (first > 0)
            {
                BonusTimeImages[0].gameObject.SetActive(true);
                BonusTimeImages[1].gameObject.SetActive(true);
                BonusTimeImages[0].sprite = ResourceManager.Instance.BonusTime(first);
                BonusTimeImages[1].sprite = ResourceManager.Instance.BonusTime(second);
            }
            else
            {
                BonusTimeImages[0].gameObject.SetActive(true);
                BonusTimeImages[1].gameObject.SetActive(false);
                BonusTimeImages[0].sprite = ResourceManager.Instance.BonusTime(second);
            }
        }
    }
}