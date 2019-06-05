using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public static class UnityExtension
    {
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }
        
        public static void TraversalChildren(this Transform transform, UnityAction<Transform> action)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                action.Invoke(transform.GetChild(i));
            }
        }
    }
}