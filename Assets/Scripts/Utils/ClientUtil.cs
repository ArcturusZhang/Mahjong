using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;


namespace Utils
{
    public static class ClientUtil
    {
        public static void ReplaceListener(Button button, UnityAction listener)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(listener);
        }

        public static List<int> GetDigits(int number)
        {
            Assert.IsTrue(number >= 0, "number >= 0");
            var digits = new List<int>();
            if (number == 0)
            {
                digits.Add(0);
                return digits;
            }

            while (number > 0)
            {
                digits.Add(number % 10);
                number /= 10;
            }

            digits.Reverse();
            return digits;
        }

        public static bool ArrayEquals<T>(T[] array1, T[] array2)
        {
            Debug.Log($"[AssertEqual] Comparing array: {string.Join(", ", array1)} with array: {string.Join(", ", array2)}");
            if (array1.Length != array2.Length) return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1[i].Equals(array2[i])) return false;
            }

            return true;
        }
    }
}