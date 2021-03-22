﻿using Netherlands3D.InputHandler;
using Netherlands3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Netherlands3D.ObjectInteraction
{
    public class Interactable : MonoBehaviour
    {
        public ContextPointerMenu.ContextState contextMenuState = ContextPointerMenu.ContextState.CUSTOM_OBJECTS;

        [Header("Interactable settings")]
        public bool blockMouseSelectionInteractions = false;
        public bool blockMouseNavigationInteractions = true;
        public bool blockKeyboardNavigationInteractions = false;
        
        private InputActionMap actionMap;
        public InputActionMap ActionMap {
            get
            {
                return actionMap;
            }
            set
            {
                actionMap = value;
            }
        }

        /// <summary>
        /// This interactable becomes the main interactable for the Selector
        /// </summary>
        public virtual void TakeInteractionPriority()
        {
            if (ActionMap != null) ActionMap.Enable();
            Selector.Instance.SetActiveInteractable(this);
        }
        /// <summary>
        /// Flagges the interactable interaction as done, releasing the priority focus inside the selector.
        /// </summary>
        public virtual void StopInteraction()
        {
            if (ActionMap != null) ActionMap.Disable();

            if(HasInteractionPriority())
                Selector.Instance.SetActiveInteractable(null);
        }

        /// <summary>
        /// Virtual methods for basic actions triggered from the Selector
        /// </summary>
        public virtual void Select() { }
        public virtual void SecondarySelect() { }
        public virtual void Deselect() { }

		private void OnDisable()
		{
            //Make sure to always release the priority for input when we are disabled
            StopInteraction();
        }

		/// <summary>
		/// Returns if this is the interactable we are currently hovering
		/// </summary>
		/// <returns></returns>
		public virtual bool IsHovered()
        {
            return Selector.Instance.GetHoveringInteractable() == this;
        }

        public virtual bool HasInteractionPriority()
        {
            //Always have priority if none is set
            if (!Selector.Instance.GetActiveInteractable()) return true;

            return Selector.Instance.GetActiveInteractable() == this;
        }
    }
}