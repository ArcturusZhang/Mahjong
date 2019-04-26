using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Single.UI.Controller
{
    [RequireComponent(typeof(Animator))]
    public class RoundDrawItemController : MonoBehaviour
    {
        private Animator animator;

        private void OnEnable() {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            animator.SetTrigger("Fade");
        }
    }
}
