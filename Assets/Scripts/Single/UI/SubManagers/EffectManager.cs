using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Type = Single.UI.PlayerEffectManager.Type;

namespace Single.UI.SubManagers
{
    public class EffectManager : MonoBehaviour
    {
        public Image RichiImage;
        public Image ChowImage;
        public Image PongImage;
        public Image KongImage;
        public Image BeiImage;
        public Image TsumoImage;
        public Image RongImage;

        private readonly IDictionary<Type, Image> dict = new Dictionary<Type, Image>();

        private void Awake()
        {
            dict.Clear();
            foreach (Type e in Enum.GetValues(typeof(Type)))
            {
                var fieldInfo = GetType().GetField($"{e}Image");
                var image = (Image)fieldInfo.GetValue(this);
                dict.Add(e, image);
            }
        }

        public float StartAnimation(Type type)
        {
            var image = dict[type];
            var sequence = DOTween.Sequence();
            sequence.Append(image.DOFade(1, MahjongConstants.FadeDuration))
                .Append(image.DOFade(0, MahjongConstants.FadeDuration));
            return sequence.Duration();
        }
    }
}
