using UnityEngine;

namespace UI.ResourcesBundle
{
    [CreateAssetMenu(menuName = "Mahjong/NumberSprites")]
    public class NumberSprites : ScriptableObject
    {
        public Sprite[] Numbers;

        public Sprite GetNumber(int index)
        {
            return Numbers[index];
        }
    }
}