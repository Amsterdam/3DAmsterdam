using Amsterdam3D.InputHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Amsterdam3D.Interface
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField]
        private string actionMapName = "";

        public Ray receivedRay;

        private InputActionMap actionMap;
        public InputActionMap ActionMap {
            get
            {
                if(actionMap == null) actionMap = ActionHandler.actions.asset.FindActionMap(actionMapName);
                return actionMap;
            }
            set
            {
                actionMap = value;
            }
        }

        public virtual void Hover(Ray ray)  
        {
            receivedRay = ray;
        }

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