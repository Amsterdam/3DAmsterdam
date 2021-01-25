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
	/// This class handles the base order in which we handle input (divided in action maps) for interactable objects.
	/// We provide a main raycast for objects that require a ray based on the mouse/pointer position.
	/// Here we also maintain the lists of multiselected objects, and set the right context menu.
	/// </summary>
	public class Selector : MonoBehaviour
	{
		[SerializeField]
		private OutlineObject outline;

		public static Selector Instance = null;

		public List<OutlineObject> selectedObjects;

		private Ray ray;
		private RaycastHit[] hits;

		private InputActionMap selectorActionMap;

		private IAction clickedAction;
		private IAction clickedSecondaryAction;
		private IAction multiselectAction;

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
		private Interactable[] defaultInteractables;

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
			multiselectAction = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.Multiselect);

			//Listeners
			clickedAction.SubscribePerformed(Click);
			clickedSecondaryAction.SubscribePerformed(SecondaryClick);
			multiselectAction.SubscribePerformed(MultiselectStart);

			multiselectAction.SubscribeCancelled(MultiselectFinish);
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
			//Always raycast to look for hover actions
			ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			RaycastHit[] hits = Physics.RaycastAll(ray, 10000, raycastLayers.value);
			if (hits.Length > 0 && !activeInteractable)
			{
				HoveringInteractableUnderRay(hits);
			}

			if (activeInteractable)
			{
				activeInteractable.SetRay(ray);
				EnableCameraActionMaps(true, false);
			}
			else
			{
				DisableAllActionMaps();
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
			sortedHits = hits.OrderBy(h => h.distance)
				.ThenBy(n => n.collider.gameObject.layer == raycastPriorityLayer.value)
				.ToArray();

			//Find interactables in our hit list
			foreach (var hit in sortedHits)
			{
				//Hovering an interactable?
				hoveringInteractable = hit.collider.GetComponent<Interactable>();
				if (!hoveringInteractable) hoveringInteractable = hit.collider.transform.root.GetComponent<Interactable>();

				//If we found an interactable under the mouse enable its own actions if it has a map
				if(hoveringInteractable){
					hoveringInteractable.SetRay(ray);
					if (hoveringInteractable.ActionMap != null)
						hoveringInteractable.ActionMap.Enable();
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Enable action maps for camera interaction. 
		/// Keyboard and mouse events are split in two action maps, to be able to enable/disable them specifically.
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
			Debug.Log("Selector click");
			if(!HoveringInterface() && hoveringInteractable)
			{
				hoveringInteractable.Select();
			}
			else if (!HoveringInterface() && !hoveringInteractable)
			{
				Debug.Log("Pass click down to 'special layers with a delay'");
				DeselectAll();
			}
		}
		private void SecondaryClick(IAction action)
		{
			Debug.Log("Selector secondary click");
			if(!HoveringInterface())
			{
				if (hoveringInteractable)
				{
					//Open context menu based on the interactable we are hovering
					ContextPointerMenu.Instance.SwitchState(hoveringInteractable.contextMenuState);
					ContextPointerMenu.Instance.SetTargetInteractable(hoveringInteractable);
					ContextPointerMenu.Instance.Appear();
					hoveringInteractable.Select();
				}
				else
				{
					ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.DEFAULT);
					ContextPointerMenu.Instance.SetTargetInteractable(null);
					ContextPointerMenu.Instance.Appear();
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
		}

		private void MultiselectStart(IAction action)
		{
			if (!HoveringInterface())
			{
				//enable selectiontool actionmap
			}
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