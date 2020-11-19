using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Amsterdam3D.FreeShape
{
	public class ScaleHandle : ObjectManipulation
	{
		private bool dragging = false;
		private float startObjectY = 0.0f;

		private FreeShape freeShape;

		[SerializeField]
		private Material defaultMaterial;
		[SerializeField]
		private Material hoverMaterial;

		private MeshRenderer meshRenderer;

		private void Awake()
		{
			freeShape = GetComponentInParent<FreeShape>();
			meshRenderer = GetComponent<MeshRenderer>();
			if (meshRenderer)
				meshRenderer.material = defaultMaterial;
		}

		public override void OnMouseDown()
		{
			base.OnMouseDown();
			dragging = true;
			startObjectY = this.transform.position.y;
		}
		public override void OnMouseUp()
		{
			base.OnMouseUp();
			if (meshRenderer)
				meshRenderer.material = defaultMaterial;
			dragging = false;
		}
		private void OnMouseEnter()
		{
			if (meshRenderer)
				meshRenderer.material = hoverMaterial;
		}
		private void OnMouseExit()
		{
			if (dragging || !meshRenderer) return;
			meshRenderer.material = defaultMaterial;
		}

		private void OnMouseDrag()
		{
			MoveHandleIntoDragDirection();
		}

		private void MoveHandleIntoDragDirection()
		{
			var dragTargetPosition = GetWorldPositionOnPlane(Input.mousePosition, startObjectY) - clickOffsetOnWorldPlane;
			var originalLocalPosition = transform.localPosition;
			transform.position = dragTargetPosition;

			transform.localPosition = new Vector3(
				(AxisConstraint.x != 0.0f) ? transform.localPosition.x : originalLocalPosition.x,
				(AxisConstraint.y != 0.0f) ? transform.localPosition.y : originalLocalPosition.y,
				(AxisConstraint.z != 0.0f) ? transform.localPosition.z : originalLocalPosition.z
			);

			freeShape.UpdateShape(this);
		}
	}
}