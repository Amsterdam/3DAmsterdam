using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
    public class Interactable : MonoBehaviour
    {
        public void Hover(Ray ray)
        {

        }

        public void TakePriority()
        {
            Selector.activeInteractable = this;
        }

        public void InteractionCompleted()
        {
            Selector.activeInteractable = null;
        }
    }
}