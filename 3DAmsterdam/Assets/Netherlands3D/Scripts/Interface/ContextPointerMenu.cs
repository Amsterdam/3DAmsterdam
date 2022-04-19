using Netherlands3D.Cameras;
using Netherlands3D.Interface.Modular;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
	public class ContextPointerMenu : MonoBehaviour, IUniqueService
	{
		[SerializeField]
		private RectTransform contextItemsPanel = default;

		[SerializeField]
		private Button transformSubmenuItem;
		[SerializeField]
		private RectTransform transformSubMenu = default;

		public ContextState state = ContextState.DEFAULT;

		private Interactable targetInteractable;
		private Transformable targetTransformable;

		[SerializeField]
		[Tooltip("Select buttons that should be active for specific states")]
		private StateButtons[] buttonsAvailableOnState;

		private Button[] allButtons;


		[Serializable]
		public class StateButtons
		{
			public ContextState state;
			public Button[] activeButtons;
		}

		public enum ContextState
		{
			DEFAULT,
			CUSTOM_OBJECTS,
			BUILDING_SELECTION,
			MULTI_BUILDING_SELECTION
		}

		private void Start()
		{
			//Add a listener to every containing button that closes our context menu on click
			allButtons = GetComponentsInChildren<Button>();
			foreach (Button button in allButtons)
				button.onClick.AddListener(CloseContextMenu);

			SwitchState(ContextState.DEFAULT);
			contextItemsPanel.gameObject.SetActive(false);
		}

		public void SetTargetInteractable(Interactable newTargetInteractable)
		{
			targetInteractable = newTargetInteractable;
			if(newTargetInteractable)
				targetTransformable = newTargetInteractable.GetComponent<Transformable>();
		}

		/// <summary>
		/// Start transforming the focus object of our contextmenu
		/// </summary>
		/// <param name="setGizmoTransformType">0=Translate, 1=Rotate, 2=Scale</param>
		public void TransformObject(int setGizmoTransformType = 0)
		{
			//Enable gizmo
			if (!targetTransformable) return;

			targetTransformable.ShowTransformProperties(setGizmoTransformType);
			CloseContextMenu();
		}

		/// <summary>
		/// Disables our right mouse context menu, and resets its state do default
		/// </summary>
		void CloseContextMenu()
		{
			contextItemsPanel.gameObject.SetActive(false);
			transformSubMenu.gameObject.SetActive(false);
		}

		/// <summary>
		/// Switches the right click menu state, enabling/disabling the right buttons matching that state
		/// </summary>
		/// <param name="newState">The new state detemining what buttons are active/disabled</param>
		public void SwitchState(ContextState newState)
		{
			state = newState;

			//Start by disabling all buttons but disablig their interactibility
			foreach (Button button in allButtons)
				button.interactable = false;

			//Now only active the buttons that should be active in this new state
			foreach (StateButtons stateButtons in buttonsAvailableOnState)
			{
				if (state == stateButtons.state)
				{
					foreach (Button button in stateButtons.activeButtons)
						button.interactable = true;
					return;
				}
			}
		}

		/// <summary>
		/// Pops out transform options if the button is active
		/// </summary>
		public void PopOutTransformSubmenu()
		{
			if (transformSubmenuItem.interactable)
			{
				transformSubMenu.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Moves our context menu to our pointer, and actives it with an animation. 
		/// </summary>
		public void Appear()
		{
			CloseContextMenu();

			contextItemsPanel.transform.position = Mouse.current.position.ReadValue();
			contextItemsPanel.gameObject.SetActive(true);

			contextItemsPanel.GetComponent<ClickOutsideToClose>().IgnoreClicks(1);
		}
	}
}