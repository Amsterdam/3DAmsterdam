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
	private Transform pointToPutOnGround;

	private bool snapToGround = true;
	private float dropDownDistance = 300.0f;
	private bool stickToMouseOnSpawn = true;

	[SerializeField]
	private Collider collider;

	private void Start()
	{
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

		collider.enabled = false;
		StopAllCoroutines();
		FollowMousePointer();
	}

	private void OnMouseUp()
	{
		collider.enabled = true;
	}

	private void FollowMousePointer()
	{
		RaycastHit hit;
		var ray = CameraControls.Instance.camera.ScreenPointToRay(Input.mousePosition);

		/*if (Input.GetMouseButtonDown(0))
		{
			//Check if a collider is under our mouse, if not get a point on NAP~0
			if (Physics.Raycast(ray, out hit))
			{
				rotatePoint = hit.point;
				focusPointChanged(rotatePoint);
			}
			else if (new Plane(Vector3.up, new Vector3(0.0f, Constants.ZERO_GROUND_LEVEL_Y, 0.0f)).Raycast(ray, out float enter))
			{
				rotatePoint = ray.GetPoint(enter);
				focusPointChanged(rotatePoint);
			}
		}*/

		this.transform.position = GetWorldPositionOnPlane(Input.mousePosition, Constants.ZERO_GROUND_LEVEL_Y) - clickOffset;
		if (snapToGround)
			DropDownOnGround();
	}

	private void DropDownOnGround()
	{
		//Try to see if we can fall down on the ground
		if (Physics.Raycast(this.transform.position + Vector3.up * 150.0f, Vector3.down, out RaycastHit hit, dropDownDistance, dropTargetLayerMask.value))
		{
			print("DROPPING ON :" + hit.transform.name);
			this.transform.position = hit.point;
			/*if (pointToPutOnGround)
				this.transform.Translate(-pointToPutOnGround.localPosition);*/
		}
	}
}
