using Amsterdam3D.CameraMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Amsterdam3D.Interface
{
	public class ContextPointerMenu : MonoBehaviour
	{
		[SerializeField]
		private RectTransform contextItemsPanel = default;

		public static ContextPointerMenu Instance = null;

		public ContextState state = ContextState.DEFAULT;

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
			if (Instance == null)
			{
				Instance = this;
			}

			//Add a listener to every containing button that closes our context menu on click
			allButtons = GetComponentsInChildren<Button>();
			foreach (Button button in allButtons)
				button.onClick.AddListener(CloseContextMenu);

			SwitchState(ContextState.DEFAULT);
			contextItemsPanel.gameObject.SetActive(false);
		}

		public void SetTargetTransformable(Transformable newTarget)
		{
			targetTransformable = newTarget;
		}

		/// <summary>
		/// Start transforming the focus object of our contextmenu
		/// </summary>
		public void TransformObject()
		{
			//Enable gizmo
			if (!targetTransformable) return;
				targetTransformable.ShowTransformProperties();
		}

		/// <summary>
		/// Disables our right mouse context menu, and resets its state do default
		/// </summary>
		void CloseContextMenu()
		{
			contextItemsPanel.gameObject.SetActive(false);
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

		void Update()
		{
			//TODO: replace with centralized input system to avoid conflicts with other scripts polling for the same buttons
			if (Input.GetMouseButtonUp(1) && !EventSystem.current.IsPointerOverGameObject())
			{
				Appear();
			}
		}

		/// <summary>
		/// Moves our context menu to our pointer, and actives it with an animation. 
		/// </summary>
		private void Appear()
		{
			contextItemsPanel.transform.position = Input.mousePosition;

			//Always disable the panel first, so our appear animation plays again
			contextItemsPanel.gameObject.SetActive(false);
			contextItemsPanel.gameObject.SetActive(true);
		}
	}
}