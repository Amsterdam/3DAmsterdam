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

		private FreeShape freeShape;

		[SerializeField]
		private Material defaultMaterial;
		[SerializeField]
		private Material hoverMaterial;

		private MeshRenderer renderer;

		[SerializeField]
		private float screenSize = 10.0f;

		private void Start()
		{
			freeShape = GetComponentInParent<FreeShape>();
			renderer = GetComponent<MeshRenderer>();

			if(renderer)
				renderer.material = defaultMaterial;
		}

		private void Update()
		{
			this.transform.localScale = Vector3.one * Vector3.Distance(Camera.main.transform.position, transform.position) * screenSize;
		}

		private void OnMouseEnter()
		{
			renderer.material = hoverMaterial;
		}
		private void OnMouseExit()
		{
			if (dragging) return;
			renderer.material = defaultMaterial;
		}

		private void OnMouseDown()
		{
			dragging = true;
			draggingAHandle = true;
			objectY = this.transform.position.y;
		}

		private void OnMouseUp()
		{
			renderer.material = defaultMaterial;
			dragging = false;
			draggingAHandle = false;
		}

		private void OnMouseDrag()
		{
			MoveHandleIntoDragDirection();
		}

		private void MoveHandleIntoDragDirection()
		{
			var dragTargetPosition = GetWorldPositionOnPlane(Input.mousePosition, objectY);
			var originalLocalPosition = transform.localPosition;
			transform.position = dragTargetPosition;

			transform.localPosition = new Vector3(
				(Axis.x != 0.0f) ? transform.localPosition.x : originalLocalPosition.x,
				(Axis.y != 0.0f) ? transform.localPosition.y : originalLocalPosition.y,
				(Axis.z != 0.0f) ? transform.localPosition.z : originalLocalPosition.z
			);

			freeShape.UpdateShape(this);
		}

		public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float planeWorldY)
		{
			var ray = Camera.main.ScreenPointToRay(screenPosition);

			var planeNormal = Vector3.up;
			if (axis == Vector3.up) 
			{
				//Up handle uses a plane looking at camera, flattened on the Y 
				planeNormal = Camera.main.transform.position - this.transform.position;
				planeNormal.y = 0;
			}			

			var worldPlane = new Plane(planeNormal, new Vector3(transform.position.x, planeWorldY,transform.position.z));
			worldPlane.Raycast(ray, out float distance);
			return ray.GetPoint(distance);
		}
	}
}