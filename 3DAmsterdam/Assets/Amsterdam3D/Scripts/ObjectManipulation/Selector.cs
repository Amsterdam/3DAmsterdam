using Amsterdam3D.CameraMotion;
using Amsterdam3D.InputHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			selectedObjects = new List<OutlineObject>();
		}

		private void Update()
		{
			//Always raycast to look for hoover actions
			ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 10000, raycastLayers.value))
			{
				if(HooveringInteractableCollider())
				{
					//Disable action map with camera mouse actions
					//ActionHandler.actions.GodView.Disable();
				}
				else{
					//Enable action map with camera mouse actions
					//ActionHandler.actions.GodView.Enable();
				}
			}

			//Only allow click/selects starts that swap action maps if we are not hovering interface
			if (!HoveringInterface())
			{
				
			}
		}

		private bool HooveringInteractableCollider()
		{
			if (hit.collider.CompareTag("Gizmo"))
			{
				//ActionHandler.actions.Gizmos.Enable();
				return true;
			}
			else if (hit.collider.CompareTag("Transformable"))
			{
				//ActionHandler.actions.Gizmos.Enable();
				return true;
			}
			return false;
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