using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3d.Interface
{
    public class InterfaceLayers : MonoBehaviour
    {
        private Animator animator;
        private bool toggledVisible = false;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }
        
        public void ToggleVisibility(bool visible)
        {
            toggledVisible = visible;
            animator.SetBool("AnimateIn", toggledVisible);
        }
    }
}
