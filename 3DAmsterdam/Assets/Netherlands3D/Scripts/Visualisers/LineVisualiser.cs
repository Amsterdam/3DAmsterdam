using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Visualisers
{
	public class LineVisualiser : MonoBehaviour
	{
		[SerializeField]
		private Vector3ListEvent lineCoordinatesEvent;

		[SerializeField]
		private TriggerEvent finishedDrawing;

		[SerializeField]
		private Material lineRendererMaterial;

		[SerializeField]
		private Color lineColor;

		[SerializeField]
		private float thickness = 1.0f;

		void Start()
		{
			lineCoordinatesEvent.unityEvent.AddListener(DrawLine);
		}
		
		private void DrawLine(List<Vector3> linePoints)
		{
			var lineRenderObject = new GameObject();
			LineRenderer newLineRenderer = lineRenderObject.AddComponent<LineRenderer>();
			newLineRenderer.positionCount = linePoints.Count;
			newLineRenderer.material = lineRendererMaterial;
			newLineRenderer.startWidth = thickness;
			newLineRenderer.endWidth = thickness;
			newLineRenderer.startColor = lineColor;
			newLineRenderer.endColor = lineColor;

			for (int i = 0; i < linePoints.Count; i++)
			{
				newLineRenderer.SetPosition(i, linePoints[i]);
			}
		}
	}
}