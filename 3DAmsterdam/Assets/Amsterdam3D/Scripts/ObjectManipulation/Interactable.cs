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

        /// <summary>
        /// Sets the main ray used for determining click/pointer position
        /// </summary>
        /// <param name="ray">Ray usualy served by the selector (that shoots our main pointer ray)</param>
        public virtual void SetRay(Ray ray)  
        {
            receivedRay = ray;
        }

        /// <summary>
        /// This interactable becomes the main interactable for the Selector
        /// </summary>
        public virtual void TakePriority()
        {
            Selector.Instance.SetActiveInteractable(this);
        }

		public virtual void Select()
		{
			
		}
        public virtual void Deselect()
        {

        }

        /// <summary>
        /// Returns if this is the interactable we are currently hovering
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Flagges the interactable interaction as done, releasing the priority focus inside the selector.
        /// </summary>
        public virtual void InteractionCompleted()
        {
            Selector.Instance.SetActiveInteractable(null);
        }
    }
}