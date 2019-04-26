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

        private readonly IDictionary<PlayerEffectManager.Type, Animator> dict = new Dictionary<PlayerEffectManager.Type, Animator>();

        private void OnEnable()
        {
            dict.Clear();
            foreach (PlayerEffectManager.Type e in Enum.GetValues(typeof(PlayerEffectManager.Type)))
            {
                var fieldInfo = GetType().GetField($"{e}Animator");
                var animator = (Animator)fieldInfo.GetValue(this);
                dict.Add(e, animator);
            }
        }

        public void ShowAnimation(PlayerEffectManager.Type type)
        {
            dict[type].gameObject.SetActive(true);
        }

        public IEnumerator StartAnimation(PlayerEffectManager.Type type)
        {
            ShowAnimation(type);
            yield return new WaitForSeconds(MahjongConstants.AnimationDelay);
            Fade(type);
            yield return new WaitForSeconds(MahjongConstants.AnimationDelay);
            Close(type);
        }

        public void Fade(PlayerEffectManager.Type type)
        {
            dict[type].SetTrigger("Fade");
        }

        public void Close(PlayerEffectManager.Type type)
        {
            dict[type].gameObject.SetActive(false);
        }
    }
}
