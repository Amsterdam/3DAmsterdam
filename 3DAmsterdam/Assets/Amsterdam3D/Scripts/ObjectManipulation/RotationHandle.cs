using Amsterdam3D.FreeShape;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationHandle : ObjectManipulation
{
	[SerializeField]
	private Transform rotationCenter;

	private GameObject rotationOrigin;
	private Vector3 dragStartLocation;
	private float startYRotation = 0;

	private void Start()
	{
		if (!rotationCenter)
			rotationCenter = this.transform;
	}

	public override void OnMouseDown()
	{
		base.OnMouseDown();
		rotationOrigin = new GameObject();
		rotationOrigin.name = "CustomShapeRotationOrigin";
		rotationOrigin.transform.position = rotationCenter.position;

		dragStartLocation = GetWorldPositionOnPlane(Input.mousePosition, this.transform.position.y);

		startYRotation = rotationOrigin.transform.eulerAngles.y;

		transform.parent.SetParent(rotationOrigin.transform);

	}
	public override void OnMouseUp()
	{
		base.OnMouseUp();
		transform.parent.SetParent(null);
		Destroy(rotationOrigin);
	}

	private void OnMouseDrag()
	{
		RotateParentOverOrigin();
	}

	private void RotateParentOverOrigin()
	{
		var dragTargetPosition = GetWorldPositionOnPlane(Input.mousePosition, this.transform.position.y);

		var startNormal = (dragStartLocation - rotationOrigin.transform.position).normalized;
		var targetNormal = (dragTargetPosition - rotationOrigin.transform.position).normalized;
		startNormal.y = 0;
		targetNormal.y = 0;

		Debug.DrawLine(rotationOrigin.transform.position, rotationOrigin.transform.position+startNormal, Color.green);
		Debug.DrawLine(rotationOrigin.transform.position, rotationOrigin.transform.position+targetNormal, Color.red);

		var angle = Vector3.SignedAngle(targetNormal, startNormal,Vector3.up);
		rotationOrigin.transform.rotation = Quaternion.Euler(0.0f,-angle,0.0f);
	}
}
