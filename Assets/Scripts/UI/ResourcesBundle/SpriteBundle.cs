using UnityEngine;

namespace UI.ResourcesBundle
{
    [CreateAssetMenu(menuName = "Mahjong/SpriteBundle")]
    public class SpriteBundle : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;

        public Sprite Get(int index)
        {
            return sprites[index];
        }

        public int Length => sprites.Length;
    }
}