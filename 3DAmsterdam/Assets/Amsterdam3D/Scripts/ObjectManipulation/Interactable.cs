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
        public string actionMapName = "";

        public ContextPointerMenu.ContextState contextMenuState = ContextPointerMenu.ContextState.CUSTOM_OBJECTS;

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

        public virtual void SetRay(Ray ray)  
        {
            receivedRay = ray;
        }

        public virtual void TakePriority()
        {
            Selector.Instance.SetActiveInteractable(this);
        }

        public virtual bool IsHovered()
        {
            return Selector.Instance.GetHoveringInteractable() == this;
        }

        public virtual bool HasPriority()
        {
            //Always have priority if none is set
            if (!Selector.Instance.GetActiveInteractable()) return true;

            return Selector.Instance.GetActiveInteractable() == this;
        }

        public virtual void InteractionCompleted()
        {
            Selector.Instance.SetActiveInteractable(null);
        }
    }
}