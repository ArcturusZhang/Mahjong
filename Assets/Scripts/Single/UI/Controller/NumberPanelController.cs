using System.Collections;
using UI.ResourcesBundle;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Utils;

namespace Single.UI.Controller
{
    public class NumberPanelController : MonoBehaviour
    {
        [Header("Number settings")]
        public Transform NumberParent;
        public GameObject DigitPrefab;
        public SpriteBundle NumberSprites;
        public Sprite MinusSign;

        public void SetNumber(int number)
        {
            NumberParent.DestroyAllChild();
            if (number < 0)
            {
                if (MinusSign != null)
                {
                    var obj = Instantiate(DigitPrefab, NumberParent);
                    obj.name = "minus";
                    var image = obj.GetComponent<Image>();
                    image.sprite = MinusSign;
                }
                else
                {
                    Debug.LogError($"Minus sign not assigned on GameObject {name}");
                }
                number = -number;
            }
            var digits = ClientUtil.GetDigits(number);

            for (int i = 0; i < digits.Count; i++)
            {
                var obj = Instantiate(DigitPrefab, NumberParent);
                obj.name = $"Digit{i}";
                var image = obj.GetComponent<Image>();
                image.sprite = NumberSprites.Get(digits[i]);
            }
        }
    }
}