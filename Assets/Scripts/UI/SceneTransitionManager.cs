using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(Animator))]
    public class SceneTransitionManager : MonoBehaviour
    {
        public Animator animator;
        public void FadeOut()
        {
            animator.SetTrigger("end");
        }
    }
}
