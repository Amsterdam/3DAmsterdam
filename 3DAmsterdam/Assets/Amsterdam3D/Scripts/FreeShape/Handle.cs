using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.FreeShape
{
	public class Handle : ObjectManipulation
	{
		private bool dragging = false;
		private float objectY = 0.0f;

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
		public override void OnMouseDown()
		{
			base.OnMouseDown();
			dragging = true;
			objectY = this.transform.position.y;
		}
		public override void OnMouseUp()
		{
			base.OnMouseUp();
			renderer.material = defaultMaterial;
			dragging = false;
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
	}
}