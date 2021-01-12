using Amsterdam3D.CameraMotion;
using Amsterdam3D.InputHandler;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformable : MonoBehaviour
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

	private IAction clickAction;

	private void Start()
	{
		clickAction = ActionHandler.instance.GetAction(ActionHandler.actions.GodView.MoveCamera);

		meshCollider = GetComponent<Collider>();
		if (stickToMouse)
		{
			meshCollider.enabled = false;
			StartCoroutine(StickToMouse());
		}
	}

	private void OnMouseDown()
	{
		if(!stickToMouse && lastSelectedTransformable != this)
		{
			ShowTransformProperties();
		}
		ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.CUSTOM_OBJECTS);
	}

	void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1))
		{
			stickToMouse = false;
			ContextPointerMenu.Instance.SetTargetTransformable(this);
			ShowTransformProperties();
		}
		ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.CUSTOM_OBJECTS);
	}

	/// <summary>
	/// Show the transform property panel for this transformable
	/// </summary>
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
		ContextPointerMenu.Instance.SetTargetTransformable(this);

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
