using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulation : MonoBehaviour
{
	[SerializeField]
	private Vector3 axisContraint;
	public Vector3 AxisConstraint { get => axisContraint; }

	public static bool manipulatingObject = false;
	public float screenSize = 0.0f;

	[HideInInspector]
	public Vector3 clickOffset;

	private void Update()
	{
		if(screenSize > 0)
			this.transform.localScale = Vector3.one * Vector3.Distance(Camera.main.transform.position, transform.position) * screenSize;
	}

	public virtual void OnMouseDown()
	{
		SetClickOffset();
		manipulatingObject = true;
	}

	public void SetClickOffset()
	{
		clickOffset = GetWorldPositionOnPlane(Input.mousePosition, this.transform.position.y) - this.transform.position;
	}

	public virtual void OnMouseUp()
	{
		manipulatingObject = false;
	}

	public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float planeWorldYPosition)
	{
		var ray = Camera.main.ScreenPointToRay(screenPosition);

		var planeNormal = Vector3.up;
		if (AxisConstraint == Vector3.up)
		{
			//Up handle uses a plane looking at camera, flattened on the Y, so we can drag something up
			planeNormal = Camera.main.transform.position - this.transform.position;
			planeNormal.y = 0;
		}

		var worldPlane = new Plane(planeNormal, new Vector3(transform.position.x, planeWorldYPosition, transform.position.z));
		worldPlane.Raycast(ray, out float distance);
		return ray.GetPoint(distance);
	}
}
