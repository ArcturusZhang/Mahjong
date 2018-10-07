using System.Collections;
using System.Linq;
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

        private Sprite[] baseTimeSprites;
        private Sprite[] bonusTimeSprites;
        private WaitForSeconds wait;

        private int mBaseTime;
        private int mBonusTime;

        private void Awake()
        {
            wait = new WaitForSeconds(1f);
            StartCoroutine(LoadSpritesAsync());
        }

        private IEnumerator LoadSpritesAsync()
        {
            var sprites = Resources.LoadAll<Sprite>("Textures/UIElements/mjdesktop3");
            baseTimeSprites = new Sprite[10];
            bonusTimeSprites = new Sprite[10];
            for (int i = 0; i < 10; i++)
            {
                baseTimeSprites[i] = sprites.FirstOrDefault(sprite => sprite.name == $"time{i}");
                Debug.Log(baseTimeSprites[i].name);
                bonusTimeSprites[i] = sprites.FirstOrDefault(sprite => sprite.name == $"bonus{i}");
                Debug.Log(bonusTimeSprites[i].name);
            }

            yield return null;
        }

        public void StartCountDown(int baseTime, int bonusTime, UnityAction callback)
        {
            mBaseTime = baseTime;
            mBonusTime = bonusTime;
            SetTime(mBaseTime, mBonusTime);
            currentTimerCoroutine = StartCoroutine(CountDown(callback));
        }

        public int StopCountDown()
        {
            if (currentTimerCoroutine != null) StopCoroutine(currentTimerCoroutine);
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
                var sprite = baseTimeSprites[baseTime];
                BaseTimeImage.sprite = sprite;
            }

            if (bonusTime == 0)
            {
                PlusImage.gameObject.SetActive(false);
                BonusTimeImages[0].gameObject.SetActive(false);
                BonusTimeImages[1].gameObject.SetActive(false);
            }

            int first = bonusTime / 10;
            int second = bonusTime % 10;

            if (first > 0)
            {
                BonusTimeImages[0].gameObject.SetActive(true);
                BonusTimeImages[1].gameObject.SetActive(true);
                BonusTimeImages[0].sprite = bonusTimeSprites[first];
                BonusTimeImages[1].sprite = bonusTimeSprites[second];
            }
            else
            {
                BonusTimeImages[0].gameObject.SetActive(true);
                BonusTimeImages[1].gameObject.SetActive(false);
                BonusTimeImages[0].sprite = bonusTimeSprites[second];
            }
        }
    }
}