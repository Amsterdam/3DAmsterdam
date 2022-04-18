using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.InputHandler;
using Netherlands3D.Interface;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.ModelParsing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.ObjectInteraction
{
	public class Transformable : Interactable
	{
		[SerializeField]
		LayerMask dropTargetLayerMask;

		[SerializeField]
		private Vector3 offset;

		[SerializeField]
		public Masking.RuntimeMask mask;

		private bool snapToGround = true;

		public bool madeWithExternalTool = false;
		public bool gridShaped = false;
		public bool snapToGrid = false;
		private bool maskArea = false;
		[SerializeField]
		public bool stickToMouse = true;

		private Collider meshCollider;
		public static Transformable lastSelectedTransformable;

		private Bounds bounds;
		private IAction placeAction;
		private ActionEventClass placeActionEvent;

		[System.Serializable]
		public class ObjectPlacedEvent : UnityEvent<GameObject> { };
		public ObjectPlacedEvent placedTransformable;

		private void Awake()
		{
			//Make sure our transformable still allows moving around, but blocks clicks/selects untill we place it
			blockMouseSelectionInteractions = true;
			blockKeyboardNavigationInteractions = false;
			blockMouseNavigationInteractions = false;
		}

		private void Start()
		{
			contextMenuState = ContextPointerMenu.ContextState.CUSTOM_OBJECTS;

			ActionMap = ActionHandler.actions.Transformable;
			placeAction = ServiceLocator.GetService<ActionHandler>().GetAction(ActionHandler.actions.Transformable.Place);

			meshCollider = GetComponent<Collider>();

			bounds = new Bounds(gameObject.transform.position, Vector3.zero);
			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
			bounds = mesh.bounds;

			if (stickToMouse)
			{
				StartPlacementByClick();
			}
		}

		private void StartPlacementByClick()
		{
			ServiceLocator.GetService<HelpMessage>().Show("<b>Klik</b> op het punt waar het object geplaatst moet worden\n\nGebruik de <b>Escape</b> toets om te annuleren");

			PlacementSettings();
			placeActionEvent = placeAction.SubscribePerformed(Place);
			TakeInteractionPriority();
			StartCoroutine(StickToMouse());
			meshCollider.enabled = false;
		}

		private void PlacementSettings()
		{
			gridShaped = IsGridShaped(bounds);
			if (gridShaped)
			{
				PlaceOnGrid(true);

				ServiceLocator.GetService<PropertiesPanel>().OpenObjectInformation("", true, 10);
				ServiceLocator.GetService<PropertiesPanel>().AddTitle("Plaatsingsopties");
				ServiceLocator.GetService<PropertiesPanel>().AddTextfield("De afmetingen van dit object passen binnen ons grid.\nGebruik de volgende opties om direct uit te lijnen en/of het bestaande gebied weg te maskeren.");
				ServiceLocator.GetService<PropertiesPanel>().AddSpacer(20);
				ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Uitlijnen op grid", true, (action) =>
				{
					PlaceOnGrid(action);
				});
				ServiceLocator.GetService<PropertiesPanel>().AddActionCheckbox("Gebied maskeren", maskArea, (action) =>
				{
					maskArea = action;
					if (mask && maskArea == false)
					{
						mask.Clear();
					}
				});
			}
		}

		private void PlaceOnGrid(bool enable)
		{
			snapToGrid = enable;
			if (snapToGrid)
			{
				ServiceLocator.GetService<VisualGrid>().Show();
			}
			else
			{
				ServiceLocator.GetService<VisualGrid>().Hide();
			}
		}

		private bool IsGridShaped(Bounds bounds)
		{
			if (((bounds.max.x - bounds.min.x) % ServiceLocator.GetService<VisualGrid>().CellSize) + 1 < 2)
			{
				if (((bounds.max.z - bounds.min.z) % ServiceLocator.GetService<VisualGrid>().CellSize) + 1 < 2)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Makes the new object stick to the mouse untill we click.
		/// Enable the collider, so raycasts can pass through the object while dragging.
		/// </summary>
		IEnumerator StickToMouse()
		{
			//Keep following mouse untill we clicked
			while (stickToMouse)
			{
				FollowMousePointer();
				yield return new WaitForEndOfFrame();
			}
			//stickToMouse = false;
			meshCollider.enabled = true;
		}

		public void Place(IAction action)
		{
			if (!ServiceLocator.GetService<Selector>().HoveringInterface() && stickToMouse && action.Performed)
			{
				Debug.Log("Placed Transformable");

				blockMouseSelectionInteractions = false;
				blockKeyboardNavigationInteractions = false;
				blockMouseNavigationInteractions = true;

				stickToMouse = false;
				placedTransformable.Invoke(this.gameObject);

				Select();
				StopInteraction();
				
				//If we used grid snapping, make sure to hide grid after placing
				if(snapToGrid)
				{
					PlaceOnGrid(false);
				}

				//If we enabled the auto masking, make sure it is applied
				if (mask && maskArea)
				{
					mask.MoveToBounds(gameObject.GetComponent<Renderer>().bounds);
				}

				//If this is a custom made transformable, check for a material remap
				ServiceLocator.GetService<PropertiesPanel>().ClearGeneratedFields();
				if (madeWithExternalTool && !ServiceLocator.GetService<MaterialLibrary>().AutoRemap(gameObject))
				{
					ServiceLocator.GetService<PropertiesPanel>().OpenCustomObjects();
				}
			}
			else
			{
				Debug.Log("Did not place Transformable");
			}
		}

		public override void Select()
		{
			if (stickToMouse) return;

			base.Select();
			if (!stickToMouse && lastSelectedTransformable != this)
			{
				if (lastSelectedTransformable) lastSelectedTransformable.Deselect();

				ShowTransformProperties();
			}

			//Place a highlight on our object
			ServiceLocator.GetService<Selector>().HighlightObject(this.gameObject);
		}
		public override void SecondarySelect()
		{
			if (stickToMouse) return;

			Select();
		}

		public override void Deselect()
		{
			base.Deselect();
			ServiceLocator.GetService<PropertiesPanel>().DeselectTransformable(this, true);
		}

		public override void Escape()
		{
			base.Escape();

			if(stickToMouse)
			{
				//Im still placing this object, use escape to abort placement, and remove this object
				Destroy(gameObject);
			}
		}

		/// <summary>
		/// Show the transform property panel for this transformable
		/// </summary>
		/// <param name="gizmoTransformType">0=Translate, 1=Rotate, 2=Scale,Empty=Keep previous</param>
		public void ShowTransformProperties(int gizmoTransformType = -1)
		{
			lastSelectedTransformable = this;
			ServiceLocator.GetService<PropertiesPanel>().OpenCustomObjects(this, gizmoTransformType);

		}

		/// <summary>
		/// Method allowing the triggers for when this object bounds were changed so the thumbnail will be rerendered.
		/// </summary>
		public void RenderTransformableThumbnail()
		{
			int objectOriginalLayer = this.gameObject.layer;
			this.gameObject.layer = ServiceLocator.GetService<PropertiesPanel>().ThumbnailExclusiveLayer;

			//Render transformable using the bounds of all the nested renderers (allowing for complexer models with subrenderers)
			Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}
			ServiceLocator.GetService<PropertiesPanel>().RenderThumbnailContaining(bounds);
			this.gameObject.layer = objectOriginalLayer;
		}

		private void FollowMousePointer()
		{
			if (ServiceLocator.GetService<Selector>().HoveringInterface()) return;
			Vector3 aimedPosition = GetMousePointOnLayerMask();
			Vector3 newPosition;
			if (aimedPosition == Vector3.zero)
			{
				return;
			}
			newPosition = aimedPosition - offset;

			if (snapToGrid)
			{
				newPosition.x -= ((newPosition.x + bounds.min.x) % ServiceLocator.GetService<VisualGrid>().CellSize);
				newPosition.z -= ((newPosition.z + bounds.min.z) % ServiceLocator.GetService<VisualGrid>().CellSize);

			}
			if (mask && maskArea)
			{
				mask.MoveToBounds(gameObject.GetComponent<Renderer>().bounds);
			}
			this.transform.position = newPosition;

		}

		private void OnDestroy()
		{
			//Hide transformpanel if we were destroyed
			ServiceLocator.GetService<PropertiesPanel>().DeselectTransformable(this);

			//Remove our placement event
			placeAction.UnSubscribe(placeActionEvent);
		}

		/// <summary>
		/// Returns the mouse position on the layer.
		/// If the raycast fails (didnt hit anything) we use plane set at average ground height.
		/// </summary>
		/// <returns>The world point where our mouse is</returns>
		private Vector3 GetMousePointOnLayerMask()
		{

			RaycastHit hit;
			if (Physics.Raycast(Selector.mainSelectorRay, out hit, ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.farClipPlane, dropTargetLayerMask.value))
			{
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("UI"))
				{
					return Vector3.zero;
				}

				if (snapToGround)
				{
					return hit.point;
				}

				return Vector3.zero;
			}
			else
			{
				return ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.GetPointerPositionInWorld();
			}
		}

	}
}