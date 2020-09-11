using Amsterdam3D.CameraMotion;
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
	public Vector3 clickOffsetOnWorldPlane;

	private void Update()
	{
		if(screenSize > 0)
			this.transform.localScale = Vector3.one * Vector3.Distance(CameraControls.Instance.camera.transform.position, transform.position) * screenSize;
	}

	public virtual void OnMouseDown()
	{
		clickOffsetOnWorldPlane = GetWorldPositionOnPlane(Input.mousePosition, this.transform.position.y) - this.transform.position;
		manipulatingObject = true;
	}

	public virtual void OnMouseUp()
	{
		manipulatingObject = false;
	}

	/// <summary>
	/// Returns the world point on a plane.
	/// </summary>
	/// <param name="screenPosition">The point inside the screen</param>
	/// <param name="planeWorldYPosition">The Y position of our plane</param>
	/// <returns></returns>
	public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float planeWorldYPosition)
	{
		var ray = CameraControls.Instance.camera.ScreenPointToRay(screenPosition);

		var planeNormal = Vector3.up;
		if (AxisConstraint == Vector3.up)
		{
			//Up handle uses a plane looking at camera, flattened on the Y, so we can drag something up
			planeNormal = CameraControls.Instance.camera.transform.position - this.transform.position;
			planeNormal.y = 0;
		}

		var worldPlane = new Plane(planeNormal, new Vector3(transform.position.x, planeWorldYPosition, transform.position.z));
		worldPlane.Raycast(ray, out float distance);
		return ray.GetPoint(distance);
	}
}
