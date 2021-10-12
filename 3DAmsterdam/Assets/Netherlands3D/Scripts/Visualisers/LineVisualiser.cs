using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineVisualiser : MonoBehaviour
{
    [SerializeField] 
    private Vector3ListsEvent lineCoordinatesEvent;

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

	private void DrawLine(List<IList<Vector3>> multiLinePoints)
	{
		foreach (List<Vector3> linePoints in multiLinePoints)
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
