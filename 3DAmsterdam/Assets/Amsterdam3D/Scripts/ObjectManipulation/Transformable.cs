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

	private IAction selectAction;

	private void Start()
	{
		actionMapName = "Transformable";

		selectAction = ActionHandler.instance.GetAction(ActionHandler.actions.Transformable.Select);
        selectAction.SubscribePerformed(Select, 1);
		
		meshCollider = GetComponent<Collider>();
		if (stickToMouse)
		{
			meshCollider.enabled = false;
			StartCoroutine(StickToMouse());
		}
	}

	public void Select(IAction action)
	{
		if (!stickToMouse && lastSelectedTransformable != this)
		{
			ShowTransformProperties();
		}
		else if(stickToMouse)
		{
			stickToMouse = false;
			ContextPointerMenu.Instance.SetTargetInteractable(this);
			ShowTransformProperties();
		}
	}

	public void Deselect()
	{
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

	/// <summary>
	/// Makes the new object stick to the mouse untill we click.
	/// Enable the collider, so raycasts can pass through the object while dragging.
	/// </summary>
	IEnumerator StickToMouse()
	{
		//Keep following mouse untill we clicked
		while (stickToMouse && !Input.GetMouseButton(0) && !Input.GetMouseButtonDown(1))
		{
			FollowMousePointer();
			yield return new WaitForEndOfFrame();
		}
		stickToMouse = false;

		ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.CUSTOM_OBJECTS);
		ContextPointerMenu.Instance.SetTargetInteractable(this);

		ShowTransformProperties();
		
		meshCollider.enabled = true;
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

		var ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
		if (snapToGround && Physics.Raycast(ray, out hit, CameraModeChanger.Instance.ActiveCamera.farClipPlane, dropTargetLayerMask.value))
		{
			return hit.point;
		}
		else
		{
			return CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
		}
	}

}
