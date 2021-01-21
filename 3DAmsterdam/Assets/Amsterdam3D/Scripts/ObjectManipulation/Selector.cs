using Amsterdam3D.CameraMotion;
using Amsterdam3D.InputHandler;
using System;
using System.Collections;
using System.Collections.Generic;
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
		private RaycastHit hit;

		private InputActionMap selectorActionMap;

		private IAction clickedAction;
		private IAction clickedSecondaryAction;
		private IAction multiselectAction;

		[SerializeField]
		private LayerMask raycastLayers;

		public Interactable GetActiveInteractable() => activeInteractable;
		public Interactable GetHoveringInteractable() => hoveringInteractable;

		[SerializeField]
		private Interactable activeInteractable;
		[SerializeField]
		private Interactable hoveringInteractable;

		[SerializeField]
		private Interactable[] defaultInteractables;

		void Awake()
		{
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
			ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 10000, raycastLayers.value))
			{
				if (!activeInteractable)
				{
					FindInteractableUnderRay();
				}
				else
				{
					activeInteractable.SetRay(ray);
					if (!HoveringInterface()) EnableCameraActionMaps(true, false);
				}
			}
			else if (activeInteractable)
			{
				activeInteractable.SetRay(ray);
				if (!HoveringInterface()) EnableCameraActionMaps(true, false);
			}
			else
			{
				if (!HoveringInterface()) DisableAllActionMaps();
				EnableCameraActionMaps(true, true);
			}

			//TODO: Mobile touch inputs will be handled from here as well. For example: 
			//Two fingers and pinch out on Interactable (shoot ray from centroid of 2 touches) --> send pinch delta to interactable, to maybe scale the object if its a Transformable.
			//Two fingers pinch without raycast hit? Zoom camera. Centroid of pinches delta rotates around point.
		}

		/// <summary>
		/// Finds a interactable under the raycast, and enables its actionmap
		/// </summary>
		private void FindInteractableUnderRay()
		{
			//No active interactable, but we might be hovering one.
			hoveringInteractable = hit.collider.GetComponent<Interactable>();
			if (hoveringInteractable)
			{
				hoveringInteractable.SetRay(ray);
				hoveringInteractable.ActionMap.Enable();
			}
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
			CameraModeChanger.Instance.CurrentCameraControls.EnableMouseActionMap(enableMouseActions);
		}

		/// <summary>
		/// Disables all action maps (except our own main Selector actionmap )
		/// </summary>
		private void DisableAllActionMaps()
		{
			foreach(var actionMap in ActionHandler.actions.asset.actionMaps)
			{
				if(actionMap != selectorActionMap)
					actionMap.Disable();
			}
		}

		private void Click(IAction action)
		{
			Debug.Log("Selector click");
			if (!HoveringInterface() && !hoveringInteractable)
			{
				Debug.Log("Pass click down to 'special layers with a delay'");
			}
		}
		private void SecondaryClick(IAction action)
		{
			Debug.Log("Selector secondary click");
			if (!HoveringInterface())
			{	
				if(hoveringInteractable)
				{
					Debug.Log("Secondary click on a interactable");
					//Open context menu based on the interactable we are hovering
					ContextPointerMenu.Instance.SwitchState(hoveringInteractable.contextMenuState);
					ContextPointerMenu.Instance.SetTargetInteractable(hoveringInteractable);
				}
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