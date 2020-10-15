using Amsterdam3D.CameraMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : ObjectManipulation
{
	[SerializeField]
	LayerMask dropTargetLayerMask;

	[Tooltip("Optional origin to use as center to place object on ground")]
	[SerializeField]
	private Transform optionalOffsetPoint;
	private Vector3 offset;

	private bool snapToGround = true;

	[SerializeField]
	private bool stickToMouseOnSpawn = true;

	[SerializeField]
	private Collider collider;

	private Vector3 clickOffset3D;

	private void Start()
	{
		if (optionalOffsetPoint)
			offset = optionalOffsetPoint.localPosition;

		collider = GetComponent<Collider>();
		if (stickToMouseOnSpawn)
		{
			collider.enabled = false;
			StartCoroutine(StickToMouse());
		}
	}

	/// <summary>
	/// Makes the new object stick to the mouse untill we click.
	/// Enable the collider, so raycasts can pass through the object while dragging.
	/// </summary>
	/// <returns></returns>
	IEnumerator StickToMouse()
	{
		//Keep following mouse untill we clicked
		while (!Input.GetMouseButton(0))
		{
			FollowMousePointer();
			yield return new WaitForEndOfFrame();
		}
		collider.enabled = true;
	}

	private void OnMouseDrag()
	{
		if (EventSystem.current.IsPointerOverGameObject()) return;

		FollowMousePointer();
	}

	public override void OnMouseDown(){
		collider.enabled = false;
		base.OnMouseDown();

		//We want a semi 2D offset based on where we clicked on the ground/other colliders
		clickOffset3D = this.transform.position - GetMousePointOnLayerMask();
		clickOffset3D.y = 0;

		StartCoroutine(ScrollToChangeOffset());
	}
	public override void OnMouseUp()
	{
		base.OnMouseUp();
		collider.enabled = true;

		StopAllCoroutines();
	}

	private IEnumerator ScrollToChangeOffset()
	{
		float scrollDelta;
		while (true)
		{
			scrollDelta = Input.GetAxis("Mouse ScrollWheel");
			offset += new Vector3(0, -scrollDelta, 0);
			yield return new WaitForEndOfFrame();
		}
	}

	private void FollowMousePointer()
	{
		this.transform.position = GetMousePointOnLayerMask() - offset + clickOffset3D;
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
