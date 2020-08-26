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
	}
	public override void OnMouseUp()
	{
		base.OnMouseUp();
		collider.enabled = true;
	}

	private void FollowMousePointer()
	{
		this.transform.position = GetMousePointOnLayerMask() - offset;
	}

	private Vector3 GetMousePointOnLayerMask()
	{
		RaycastHit hit;

		var ray = CameraControls.Instance.camera.ScreenPointToRay(Input.mousePosition);
		if (snapToGround && Physics.Raycast(ray, out hit, CameraControls.Instance.camera.farClipPlane, dropTargetLayerMask.value))
		{
			return hit.point;
		}
		else
		{
			return CameraControls.Instance.GetMousePositionInWorld();
		}
	}
}
