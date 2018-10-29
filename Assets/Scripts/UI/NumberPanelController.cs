using System.Collections;
using UI.ResourcesBundle;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class NumberPanelController : MonoBehaviour
    {
        [Header("Number settings")]
        public Transform NumberParent;
        public GameObject DigitPrefab;
        public NumberSprites NumberSprites;

        public void SetNumber(int number)
        {
            Assert.IsTrue(number >= 0, "Point cannot be negative");
            NumberParent.DestroyAllChild();
            var digits = ClientUtil.GetDigits(number);
            
            for (int i = 0; i < digits.Count; i++)
            {
                var obj = Instantiate(DigitPrefab, NumberParent);
                obj.name = $"Digit{i}";
                var image = obj.GetComponent<Image>();
                image.sprite = NumberSprites.GetNumber(digits[i]);
            }
        }
    }
}