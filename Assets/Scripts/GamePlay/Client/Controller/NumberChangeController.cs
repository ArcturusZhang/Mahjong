using UI.ResourcesBundle;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace GamePlay.Client.Controller
{
    public class NumberChangeController : MonoBehaviour
    {
        [Header("Number settings")]
        public Transform NumberParent;
        public GameObject DigitPrefab;
        public SpriteBundle PositiveSprites;
        public SpriteBundle NegativeSprites;
        public Sprite PlusSign;
        public Sprite MinusSign;

        public void SetNumber(int number)
        {
            NumberParent.DestroyAllChildren();
            if (number < 0)
            {
                var obj = Instantiate(DigitPrefab, NumberParent);
                obj.name = "minus";
                var image = obj.GetComponent<Image>();
                image.sprite = MinusSign;
                SetAbsNumber(-number, NegativeSprites);
            }
            else
            {
                var obj = Instantiate(DigitPrefab, NumberParent);
                obj.name = "plus";
                var image = obj.GetComponent<Image>();
                image.sprite = PlusSign;
                SetAbsNumber(number, PositiveSprites);
            }
        }

        private void SetAbsNumber(int number, SpriteBundle bundle)
        {
            var digits = ClientUtil.GetDigits(number);
            for (int i = 0; i < digits.Count; i++)
            {
                var obj = Instantiate(DigitPrefab, NumberParent);
                obj.name = $"Digit{i}";
                var image = obj.GetComponent<Image>();
                image.sprite = bundle.Get(digits[i]);
            }
        }

        public void Close()
        {
            NumberParent.DestroyAllChildren();
        }
    }
}