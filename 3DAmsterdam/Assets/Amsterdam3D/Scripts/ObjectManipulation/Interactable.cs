using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
    public class Interactable : MonoBehaviour
    {
        public virtual void HandleRay(Ray ray)  {}

        public virtual void TakePriority()
        {
            Selector.activeInteractable = this;
        }

        public virtual void InteractionCompleted()
        {
            Selector.activeInteractable = null;
        }
    }
}