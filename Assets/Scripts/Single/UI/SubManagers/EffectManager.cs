using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Single.UI.SubManagers
{
    public class EffectManager : MonoBehaviour
    {
        public Animator RichiAnimator;
        public Animator ChowAnimator;
        public Animator PongAnimator;
        public Animator KongAnimator;
        public Animator TsumoAnimator;
        public Animator RongAnimator;

        private readonly IDictionary<AnimationType, Animator> dict = new Dictionary<AnimationType, Animator>();

        private void OnEnable()
        {
            dict.Clear();
            foreach (AnimationType e in Enum.GetValues(typeof(AnimationType)))
            {
                var fieldInfo = GetType().GetField($"{e}Animator");
                var animator = (Animator)fieldInfo.GetValue(this);
                dict.Add(e, animator);
            }
        }

        public void ShowAnimation(AnimationType type)
        {
            dict[type].gameObject.SetActive(true);
        }

        public IEnumerator StartAnimation(AnimationType type)
        {
            ShowAnimation(type);
            yield return new WaitForSeconds(MahjongConstants.AnimationDelay);
            Fade(type);
            yield return new WaitForSeconds(MahjongConstants.AnimationDelay);
            Close(type);
        }

        public void Fade(AnimationType type)
        {
            dict[type].SetTrigger("Fade");
        }

        public void Close(AnimationType type)
        {
            dict[type].gameObject.SetActive(false);
        }
    }
}
