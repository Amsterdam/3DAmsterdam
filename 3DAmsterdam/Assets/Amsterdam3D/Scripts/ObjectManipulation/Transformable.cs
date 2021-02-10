using Amsterdam3D.CameraMotion;
using Amsterdam3D.InputHandler;
using Amsterdam3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformable : Interactable
{
	[SerializeField]
	LayerMask dropTargetLayerMask;

	[SerializeField]
	private Vector3 offset;

	private bool snapToGround = true;

	[SerializeField]
	private bool stickToMouse = true;

	private Collider meshCollider;
	public static Transformable lastSelectedTransformable;

	private IAction placeAction;

	private void Start()
	{
		contextMenuState = ContextPointerMenu.ContextState.CUSTOM_OBJECTS;

		ActionMap = ActionHandler.actions.Transformable;
		placeAction = ActionHandler.instance.GetAction(ActionHandler.actions.Transformable.Place);

		meshCollider = GetComponent<Collider>();
		if (stickToMouse)
		{
			placeAction.SubscribePerformed(Place);
			TakeInteractionPriority();
			StartCoroutine(StickToMouse());
			meshCollider.enabled = false;
		}
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
		stickToMouse = false;
		meshCollider.enabled = true;
	}

	public void Place(IAction action)
	{
		if(stickToMouse && action.Performed)
		{
			Debug.Log("Placed Transformable");
			stickToMouse = false;
			Select();
			StopInteraction();
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
		Selector.Instance.HighlightObject(this.gameObject);
	}
	public override void SecondarySelect()
	{
		Select();
	}

	public override void Deselect()
	{
		base.Deselect();
		ObjectProperties.Instance.DeselectTransformable(this, true);
	}

	/// <summary>
	/// Show the transform property panel for this transformable
	/// </summary>
	/// <param name="gizmoTransformType">0=Translate, 1=Rotate, 2=Scale,Empty=Keep previous</param>
	public void ShowTransformProperties(int gizmoTransformType = -1)
	{
		lastSelectedTransformable = this;
		ObjectProperties.Instance.OpenPanel(gameObject.name);
		ObjectProperties.Instance.OpenTransformPanel(this, gizmoTransformType);
		UpdateBounds();
	}

	/// <summary>
	/// Method allowing the triggers for when this object bounds were changed so the thumbnail will be rerendered.
	/// </summary>
	public void UpdateBounds()
	{
		int objectOriginalLayer = this.gameObject.layer;
		this.gameObject.layer = ObjectProperties.Instance.ThumbnailExclusiveLayer;

		//Render transformable using the bounds of all the nested renderers (allowing for complexer models with subrenderers)
		Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
		foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
		{
			bounds.Encapsulate(renderer.bounds);
		}
		ObjectProperties.Instance.RenderThumbnailContaining(bounds);
		this.gameObject.layer = objectOriginalLayer;
	}

	private void FollowMousePointer()
	{
		this.transform.position = GetMousePointOnLayerMask() - offset;
		
	}

	/// <summary>
	/// Returns the mouse position on the layer.
	/// If the raycast fails (didnt hit anything) we use plane set at average ground height.
	/// </summary>
	/// <returns>The world point where our mouse is</returns>
	private Vector3 GetMousePointOnLayerMask()
	{
		RaycastHit hit;
		if (snapToGround && Physics.Raycast(Selector.mainSelectorRay, out hit, CameraModeChanger.Instance.ActiveCamera.farClipPlane, dropTargetLayerMask.value))
		{
			return hit.point;
		}
		else
		{
			return CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
		}
	}

}
