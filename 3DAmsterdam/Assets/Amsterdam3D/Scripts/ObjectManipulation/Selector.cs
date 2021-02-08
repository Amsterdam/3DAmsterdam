using Amsterdam3D.CameraMotion;
using Amsterdam3D.InputHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Amsterdam3D.Interface
{
	/// <summary>
	/// The Selector object handles the base order in which we handle input (divided in action maps) for interactable objects.
	/// We provide a main raycast for objects that require a ray based on the mouse/pointer position.
	/// Here we also maintain the lists of multiselected objects, and set the right context menu.
	/// This is also the place from where we determine what camera movement actions are allowed. They may be overruled by other interactions.
	/// </summary>
	public class Selector : MonoBehaviour
	{
		[SerializeField]
		private OutlineObject outline;

		public static Selector Instance = null;

		public List<OutlineObject> selectedObjects;

		public static Ray mainSelectorRay;
		public static RaycastHit[] hits;

		private InputActionMap selectorActionMap;

		private IAction clickedAction;
		private IAction clickedSecondaryAction;
		private IAction multiSelectAction;

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

		void Awake()
		{
			priority3DInterfaceHitLayer = LayerMask.NameToLayer(priority3DInterfaceHitLayerName);

			if (Instance == null)
			{
				Instance = this;
			}
			selectedObjects = new List<OutlineObject>();
			InitializeActions();
		}

		private void InitializeActions()
		{
			selectorActionMap = ActionHandler.actions.Selector;

			clickedAction = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.Click);
			clickedSecondaryAction = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.ClickSecondary);
			multiSelectAction = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.Multiselect);

			//Listeners
			clickedAction.SubscribePerformed(Click);
			multiSelectAction.SubscribePerformed(Multiselect);
			multiSelectAction.SubscribeCancelled(Multiselect);

			clickedSecondaryAction.SubscribePerformed(SecondaryClick);
		}


		private void OnEnable()
		{
			selectorActionMap.Enable();
		}
		private void OnDisable()
		{
			selectorActionMap.Disable();
		}

		public void SetActiveInteractable(Interactable interactable)
		{
			activeInteractable = interactable;
		}

		private void Update()
		{
			//Always update our main selector ray, and raycast for hovers
			mainSelectorRay = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			hits = Physics.RaycastAll(mainSelectorRay, 10000, raycastLayers.value);
			if (hits.Length > 0)
			{
				HoveringInteractableUnderRay(hits);
			}
			else{
				hoveringInteractable = null;
			}

			EnableCameraActionMaps(true, !activeInteractable);

			if (activeInteractable)
			{
				EnableCameraActionMaps(true, false);
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
			CameraModeChanger.Instance.CurrentCameraControls.EnableKeyboardActionMap(enableKeyboardActions);
			CameraModeChanger.Instance.CurrentCameraControls.EnableMouseActionMap(!HoveringInterface() && enableMouseActions);
		}

		/// <summary>
		/// Disables all action maps (except our own main Selector actionmap )
		/// </summary>
		private void DisableAllActionMaps()
		{
			foreach(var actionMap in ActionHandler.actions.asset.actionMaps)
			{
				if((!hoveringInteractable || (hoveringInteractable && hoveringInteractable.ActionMap != actionMap)) && (!hoveringInteractable || (hoveringInteractable && hoveringInteractable.ActionMap != actionMap)) && actionMap != selectorActionMap && !CameraModeChanger.Instance.CurrentCameraControls.UsesActionMap(actionMap))
					actionMap.Disable();
			}
		}

		private void Click(IAction action)
		{
			Select();
		}
		private void SecondaryClick(IAction action)
		{
			Debug.Log("Selector secondary click");
			ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
			SecondarySelect();
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
				ContextPointerMenu.Instance.SwitchState(hoveringInteractable.contextMenuState);
				ContextPointerMenu.Instance.SetTargetInteractable(hoveringInteractable);
				ContextPointerMenu.Instance.Appear();
				hoveringInteractable.SecondarySelect();

				foreach (var interactable in delayedInteractables)
				{
					if (interactable != hoveringInteractable)
						interactable.Deselect();
				}
			}
			else
			{
				ContextPointerMenu.Instance.SetTargetInteractable(null);
				ContextPointerMenu.Instance.Appear();
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

		private void MultiselectFinish(IAction action)
		{
			//Check selected region for
			//Custom objects
			//Static layers

			//What kind of context / combinations etc. etc.
		}

		private bool HoveringInterface()
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