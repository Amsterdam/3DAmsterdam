using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Netherlands3D.Interface
{
	/// <summary>
	/// The Selector object handles the base order in which we handle input (divided in action maps) for interactable objects.
	/// We provide a main raycast for objects that require a ray based on the mouse/pointer position.
	/// Here we also maintain the lists of multiselected objects, and set the right context menu.
	/// This is also the place from where we determine what camera movement actions are allowed. They may be overruled by other interactions.
	/// </summary>
	public class Selector : MonoBehaviour, IUniqueService
	{
		[SerializeField]
		private OutlineObject outline;

		public List<OutlineObject> selectedObjects;

		public static Ray mainSelectorRay;
		public static RaycastHit[] hits;

		private InputActionMap selectorActionMap;

		private IAction clickedAction;
		private IAction clickedSecondaryAction;
		private IAction multiSelectAction;
		private IAction escapeAction;

		public static bool doingMultiselect = false;

		[SerializeField]
		private LayerMask raycastLayers;

		[SerializeField]
		private LayerMask raycastPriorityLayer;

		public Interactable GetActiveInteractable() => activeInteractable;
		public Interactable GetHoveringInteractable() => hoveringInteractable;

		[SerializeField]
		private Interactable activeInteractable;
		[SerializeField]
		private Interactable hoveringInteractable;

		[SerializeField]
		private Interactable[] delayedInteractables;

		private string priority3DInterfaceHitLayerName = "Interface3D";
		private int priority3DInterfaceHitLayer;

		[SerializeField]
		private RaycastHit[] sortedHits;

		[SerializeField]
		private Interactable multiSelector;

		[Header("Report any click action to these objects")]
		public UnityEvent registeredClickInput;

		void Start()
		{
			priority3DInterfaceHitLayer = LayerMask.NameToLayer(priority3DInterfaceHitLayerName);
			selectedObjects = new List<OutlineObject>();
			InitializeActions();
		}

		private void InitializeActions()
		{
			selectorActionMap = ActionHandler.actions.Selector;
			selectorActionMap.Enable();

			clickedAction = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.Selector.Click);
			clickedSecondaryAction = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.Selector.ClickSecondary);
			multiSelectAction = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.Selector.Multiselect);
			escapeAction = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.Selector.Escape);

			//Listeners
			clickedAction.SubscribePerformed(Click);
			multiSelectAction.SubscribePerformed(Multiselect);
			multiSelectAction.SubscribeCancelled(Multiselect);
			escapeAction.SubscribePerformed(Escape);

			//clickedSecondaryAction.SubscribePerformed(SecondaryClick);
		}

		public void SetActiveInteractable(Interactable newActiveInteractable)
		{
			if (newActiveInteractable != null && newActiveInteractable != activeInteractable)
			{
				//Abort/escape our current interactable
				if (activeInteractable) activeInteractable.Escape();
				activeInteractable = newActiveInteractable;
			}
			else
			{
				activeInteractable = null;
			}
		}

		private void Update()
		{
			//Always update our main selector ray, and raycast for Interactables that we are hovering
			mainSelectorRay = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			hits = Physics.RaycastAll(mainSelectorRay, 10000, raycastLayers.value);
			if (hits.Length > 0)
			{
				HoveringInteractableUnderRay(hits);
			}
			else{
				hoveringInteractable = null;
			}

			//Enable camera action maps based on the active interactable
			if (activeInteractable)
			{
				EnableCameraActionMaps(!activeInteractable.blockKeyboardNavigationInteractions, !activeInteractable.blockMouseNavigationInteractions);
			}
			else
			{
				EnableCameraActionMaps(true, true);
			}

			//TODO: Mobile touch inputs will be handled from here as well. For example: 
			//Two fingers and pinch out on Interactable (shoot ray from centroid of 2 touches) --> send pinch delta to interactable, to maybe scale the object if its a Transformable.
			//Two fingers pinch without raycast hit? Zoom camera. Centroid of pinches delta rotates around point.
		}

		/// <summary>
		/// Finds a interactable under the raycast, and enables its actionmap
		/// </summary>
		private bool HoveringInteractableUnderRay(RaycastHit[] hits)
		{
			//Sort our hit list. 
			//We want our 3D interface items to always take priority, then ordered by distance.
			sortedHits = hits.OrderBy(n => n.collider.gameObject.layer == raycastPriorityLayer.value).ThenBy(h => h.distance).ToArray();

			//Find interactables in our hit list
			foreach (var hit in sortedHits)
			{
				//Hovering an interactable?
				hoveringInteractable = hit.collider.GetComponent<Interactable>();
				if (!hoveringInteractable) hoveringInteractable = hit.collider.transform.root.GetComponent<Interactable>();

				//If we found an interactable under the mouse enable its own actions if it has a map
				if(hoveringInteractable){
					if (hoveringInteractable.ActionMap != null)
						hoveringInteractable.ActionMap.Enable();
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Enable action maps for camera interaction. 
		/// Keyboard and mouse events are split in two action maps, to be able to enable/disable them seperately.
		/// </summary>
		/// <param name="enableKeyboardActions">Enable the camera action map containing keyboard inputs</param>
		/// <param name="enableMouseActions">Enable the camera action map containing mouse inputs</param>
		private void EnableCameraActionMaps(bool enableKeyboardActions, bool enableMouseActions)
		{
			ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.EnableKeyboardActionMap(enableKeyboardActions);
			ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.EnableMouseActionMap(!HoveringInterface() && !doingMultiselect && enableMouseActions);
		}

		/// <summary>
		/// Disables all action maps (except our own main Selector actionmap )
		/// </summary>
		private void DisableAllActionMaps()
		{
			foreach(var actionMap in ActionHandler.actions.asset.actionMaps)
			{
				if((!hoveringInteractable || (hoveringInteractable && hoveringInteractable.ActionMap != actionMap)) && (!hoveringInteractable || (hoveringInteractable && hoveringInteractable.ActionMap != actionMap)) && actionMap != selectorActionMap && !ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.UsesActionMap(actionMap))
					actionMap.Disable();
			}
		}

		private void Click(IAction action)
		{
			//Let any listeners know we have made a general click
			registeredClickInput.Invoke();

			//Catch clicks if we do not have an active interactable, or one that does not block our clicks.
			if (!activeInteractable || !activeInteractable.blockMouseSelectionInteractions)
				Select();
		}
		private void SecondaryClick(IAction action)
		{
			//Let any listeners know we have made a general click
			registeredClickInput.Invoke();

			if (!activeInteractable || !activeInteractable.blockMouseSelectionInteractions)
			{
				ServiceLocator.GetService<ContextPointerMenu>().SwitchState(ContextPointerMenu.ContextState.DEFAULT);
				SecondarySelect();
			}
		}

		private void Multiselect(IAction action)
		{
			if (action.Cancelled)
			{
				doingMultiselect = false;

			}
			else if (action.Performed)
			{
				doingMultiselect = true;
				//Let any listeners know we have made a general click action
				registeredClickInput.Invoke();
			}
		}

		private void Escape(IAction action)
		{
			if(activeInteractable)
			{
				activeInteractable.Escape();
			}
		}

		private void Select()
		{
			if (HoveringInterface()) return;
			//Selector click. If we do not have a hovering interactable, try to select our delayed interactables.
			//The buildings are delayed interactables, because we need to download metadata before we know if/where we hit a building
			if (hoveringInteractable)
			{
				hoveringInteractable.Select();

				foreach (var interactable in delayedInteractables)
				{
					if (interactable != hoveringInteractable)
						interactable.Deselect();
				}
			}
			else
			{
				foreach (var interactable in delayedInteractables)
				{
					interactable.Select();
				}
				DeselectAll();
			}
		}
		private void SecondarySelect()
		{
			if (HoveringInterface()) return;
			//Selector click. If we do not have a hovering interactable, try to select our delayed interactables.
			//The buildings are delayed interactables, because we need to download metadata before we know if/where we hit a building
			if (hoveringInteractable)
			{
				//Open context menu based on the interactable we are hovering
				ServiceLocator.GetService<ContextPointerMenu>().SwitchState(hoveringInteractable.contextMenuState);
				ServiceLocator.GetService<ContextPointerMenu>().SetTargetInteractable(hoveringInteractable);
				ServiceLocator.GetService<ContextPointerMenu>().Appear();
				hoveringInteractable.SecondarySelect();

				foreach (var interactable in delayedInteractables)
				{
					if (interactable != hoveringInteractable)
						interactable.Deselect();
				}
			}
			else
			{
				ServiceLocator.GetService<ContextPointerMenu>().SetTargetInteractable(null);
				ServiceLocator.GetService<ContextPointerMenu>().Appear();
				foreach (var interactable in delayedInteractables)
				{
					interactable.SecondarySelect();
				}
			}
		}

		private void DeselectAll()
		{
			//Deselect all transformables
			var transformables = FindObjectsOfType<Transformable>();
			foreach(var tranformable in transformables)
			{
				tranformable.Deselect();
			}

			ClearHighlights();
		}

		public bool HoveringInterface()
		{
			return EventSystem.current.IsPointerOverGameObject();
		}

		/// <summary>
		/// Add an outline object to the target gameobject
		/// </summary>
		/// <param name="gameObject"></param>
		public void HighlightObject(GameObject gameObject)
		{
			ClearHighlights();
			selectedObjects.Add(Instantiate(outline, gameObject.transform));
		}

		/// <summary>
		/// Destroys all the current outlines
		/// </summary>
		public void ClearHighlights()
		{
			foreach (var outlinedObject in selectedObjects)
			{
				if (outlinedObject != null)
					Destroy(outlinedObject.gameObject);
			}
			selectedObjects.Clear(); //May allow multiselect in future features
		}
	}
}