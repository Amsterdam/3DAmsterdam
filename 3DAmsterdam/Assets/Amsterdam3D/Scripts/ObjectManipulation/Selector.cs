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
	public class Selector : MonoBehaviour
	{
		[SerializeField]
		private OutlineObject outline;
		public static Selector Instance = null;

		public List<OutlineObject> selectedObjects;

		private Ray ray;
		private RaycastHit hit;

		[SerializeField]
		private LayerMask raycastLayers;

		[SerializeField]
		private TagAndAction[] tagsAndActions;

		private Collider targetCollider;

		public static Interactable activeInteractable;
		private Interactable hoveringInteractable;

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			selectedObjects = new List<OutlineObject>();
		}

		private void Start()
		{
			FindActionMaps();
		}

		private void FindActionMaps()
		{
			for (int i = 0; i < tagsAndActions.Length; i++)
			{
				tagsAndActions[i].actionMap = ActionHandler.actions.asset.FindActionMap(tagsAndActions[i].actionMapName);
			}
		}

		private void Update()
		{
			//Always raycast to look for hover actions
			ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 10000, raycastLayers.value))
			{
				if(activeInteractable)
				{
					activeInteractable.Hover(ray);
				}
				else{
					//No active interactable, but we might be hovering one.
					hoveringInteractable = hit.collider.GetComponent<Interactable>();
					if (hoveringInteractable) hoveringInteractable.Hover(ray);
				}
				
				if (!activeInteractable && ActivateHoverActionMap())
				{
					EnableCameraActionMaps(true, false);
				}
			}
			else if(!activeInteractable)
			{
				EnableObjectActionMaps(false);
				EnableCameraActionMaps(true, true);
			}
		}

		private void EnableCameraActionMaps(bool enaleKeyboardActions, bool enableMouseActions)
		{
			//two action maps.
			//We can be dragging an interactable around and move camera while pressing arrow keys
		}

		private void EnableObjectActionMaps(bool enable)
		{
			for (int i = 0; i < tagsAndActions.Length; i++)
			{
				if (enable)
				{
					tagsAndActions[i].actionMap.Enable();
				}
				else{
					tagsAndActions[i].actionMap.Disable();
				}
			}
		}

		private void MultiselectStart(){
			if (!HoveringInterface())
			{
				//enable selectiontool actionmap
			}
		}

		private void MultiSelectFinish()
		{
			//Check selected region for
			//Custom objects
			//Static layers

			//What kind of context / combinations etc. etc.
		}

		private void InputStopped()
		{
			
		}

		private void Click(){
			if (!HoveringInterface())
			{
				
			}
		}

		private void SecondaryClick(){
			if (!HoveringInterface())
			{
				//Determine context menu
			}
		}

		private void RegisterSelectionInput(Ray ray)
		{
			//click, or right click
			

			//multiselect action?
			

			//no hovering object, check for click or right click in mid air, maybe theres a building there
			//pass ray down to SelectByID
			//show progress/waiting indicator
		}

		private bool ActivateHoverActionMap()
		{
			bool foundActionMap = false;
			for (int i = 0; i < tagsAndActions.Length; i++)
			{
				if (hit.collider.CompareTag(tagsAndActions[i].tag))
				{
					targetCollider = hit.collider;
					tagsAndActions[i].actionMap.Enable();
					foundActionMap = true;
				}
				else{
					tagsAndActions[i].actionMap.Disable();
				}
			}
			return foundActionMap;
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

	[System.Serializable]
	public struct TagAndAction{
		public string tag;
		public string actionMapName;
		public InputActionMap actionMap;
	}
}