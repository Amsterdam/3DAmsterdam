using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.FreeShape
{
	public class FreeShape : MonoBehaviour
	{
		[SerializeField]
		private Transform shape;

		[SerializeField]
		private Handle handleXMin;
		[SerializeField]
		private Handle handleXPlus;
		[SerializeField]
		private Handle handleZMin;
		[SerializeField]
		private Handle handleZPlus;

		[SerializeField]
		private Handle handleY;

		private Vector3 shapeStartScale;

		void Start()
		{
			shapeStartScale = shape.localScale;
		}

		void LateUpdate()
		{
			ClampHandlesToShape();
			ScaleShape();
		}

		public void ScaleShape()
		{
			shape.transform.localPosition = new Vector3(
				Mathf.Lerp(handleXMin.transform.localPosition.x, handleXPlus.transform.localPosition.x, 0.5f),
				Mathf.Lerp(handleXMin.transform.localPosition.y, handleXPlus.transform.localPosition.y, 0.5f),
				Mathf.Lerp(handleXMin.transform.localPosition.z, handleXPlus.transform.localPosition.z, 0.5f)
			);

			shape.transform.localScale = new Vector3(
				Vector3.Distance(handleXMin.transform.localPosition, handleXPlus.transform.localPosition) - handleXPlus.StartOffset.x*2.0f,
				Vector3.Distance(-handleY.transform.localPosition, handleY.transform.localPosition) - handleY.StartOffset.y * 2.0f,
				Vector3.Distance(handleXMin.transform.localPosition, handleXPlus.transform.localPosition) - handleZPlus.StartOffset.z * 2.0f
			);

		}

		private void ClampHandlesToShape()
		{
			ClampHandle(handleXMin);
			ClampHandle(handleXPlus);
			ClampHandle(handleZMin);
			ClampHandle(handleZPlus);
	}

		private void ClampHandle(Handle handle)
		{
			handle.transform.localPosition = new Vector3(
				(shape.localScale.x / shapeStartScale.x) * handle.StartOffset.x,
				(shape.localScale.y / shapeStartScale.y) * handle.StartOffset.y,
				(shape.localScale.z / shapeStartScale.z) * handle.StartOffset.z
			);
		}
	}
}
