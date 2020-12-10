using Amsterdam3D.CameraMotion;
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

	private void Start()
	{
		meshCollider = GetComponent<Collider>();
		if (stickToMouse)
		{
			meshCollider.enabled = false;
			StartCoroutine(StickToMouse());
		}
	}

	private void OnMouseDown()
	{
		print("Click " + name);
		if(!stickToMouse) ShowTransformProperties();
	}

	void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(1))
		{
			stickToMouse = false;

			ContextPointerMenu.Instance.SwitchState(ContextPointerMenu.ContextState.CUSTOM_OBJECTS);
			ContextPointerMenu.Instance.SetTargetObject(gameObject);
		}
	}

	private void ShowTransformProperties()
	{
		ObjectProperties.Instance.OpenPanel(gameObject.name);
		ObjectProperties.Instance.RenderThumbnailFromPosition(CameraModeChanger.Instance.ActiveCamera.transform.position, gameObject.transform.position);
		ObjectProperties.Instance.AddTransformPanel(gameObject);
	}

	/// <summary>
	/// Makes the new object stick to the mouse untill we click.
	/// Enable the collider, so raycasts can pass through the object while dragging.
	/// </summary>
	/// <returns></returns>
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
		ContextPointerMenu.Instance.SetTargetObject(gameObject);

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
