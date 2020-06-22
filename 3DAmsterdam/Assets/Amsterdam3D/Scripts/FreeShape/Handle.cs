using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.FreeShape
{
	public class Handle : MonoBehaviour
	{
		private bool dragging = false;
		public static bool draggingAHandle = false;
		private float objectY = 0.0f;

		[SerializeField]
		private Vector3 axis;
		public Vector3 Axis { get => axis; }

		private Vector3 startOffset;
		public Vector3 StartOffset { get => startOffset; }


		private void Start()
		{
			startOffset = this.transform.localPosition;
		}

		private void OnMouseDown()
		{
			dragging = true;
			draggingAHandle = true;

			objectY = this.transform.position.y;
		}

		private void Update()
		{
			if (!dragging) return;
			MoveHandleIntoDragDirection();
		}

		private void MoveHandleIntoDragDirection()
		{
			var dragTargetPosition = GetWorldPositionOnPlane(Input.mousePosition, objectY);
			transform.position = dragTargetPosition;
			transform.localPosition = new Vector3(transform.localPosition.x*Axis.x, transform.localPosition.y * Axis.y, transform.localPosition.z * Axis.z);
		}

		private void OnMouseUp()
		{
			dragging = false;
			draggingAHandle = false;
		}

		public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float planeWorldY)
		{
			var ray = Camera.main.ScreenPointToRay(screenPosition);
			var worldPlane = new Plane(Vector3.up, new Vector3(transform.position.x, planeWorldY,transform.position.z));
			worldPlane.Raycast(ray, out float distance);
			return ray.GetPoint(distance);
		}
	}
}