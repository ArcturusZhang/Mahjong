using UnityEngine;

namespace Managers
{
    [RequireComponent(typeof(Animator))]
    public class SceneTransitionManager : MonoBehaviour
    {
        public Animator animator;
        public void FadeIn()
        {
            animator.SetTrigger("start");
        }
        public void FadeOut()
        {
            animator.SetTrigger("end");
        }
    }
}
