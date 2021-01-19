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
			selectorActionMap = ActionHandler.actions.asset.FindActionMap("Selector");

			clickedAction = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.Click);
			clickedSecondaryAction = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.ClickSecondary);
			multiselectAction = ActionHandler.instance.GetAction(ActionHandler.actions.Selector.Multiselect);

			clickedAction.SubscribePerformed(Click, 0);
			clickedSecondaryAction.SubscribePerformed(SecondaryClick, 0);
			multiselectAction.SubscribePerformed(MultiselectStart, 0);

			multiselectAction.SubscribeCancelled(MultiselectFinish, 0);
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
					//No active interactable, but we might be hovering one.
					hoveringInteractable = hit.collider.GetComponent<Interactable>();
					if (hoveringInteractable)
					{
						hoveringInteractable.Hover(ray);
						hoveringInteractable.ActionMap.Enable();
					}
				}
				else
				{
					//TODO: we have an active interactable, so (mouse) camera motions are blocked
					activeInteractable.Hover(ray);
				}
			}
			else if (activeInteractable)
			{
				
			}
			else
			{
				EnableObjectActionMaps(false);
				EnableCameraActionMaps(true, true);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="enableKeyboardActions"></param>
		/// <param name="enableMouseActions"></param>
		private void EnableCameraActionMaps(bool enableKeyboardActions, bool enableMouseActions)
		{
			//two action maps.
			//We can be dragging an interactable around and move camera while pressing arrow keys
		}

		/// <summary>
		/// Enable or disable object specific action maps
		/// </summary>
		/// <param name="enable">Enable action maps</param>
		private void EnableObjectActionMaps(bool enable)
		{
			if(enable)
			{
				ActionHandler.actions.Transformable.Enable();
			}
			else{
				ActionHandler.actions.Transformable.Disable();
			}
		}

		private void Click(IAction action)
		{
			if (!HoveringInterface())
			{
				if(!hoveringInteractable)
				{
					Debug.Log("Pass click down to 'special'");
				}
			}
		}
		private void SecondaryClick(IAction action)
		{
			if (!HoveringInterface())
			{
				//Determine context menu
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