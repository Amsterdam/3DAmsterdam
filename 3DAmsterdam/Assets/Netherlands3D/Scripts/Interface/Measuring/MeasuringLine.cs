using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeasuringLine : MonoBehaviour
{
	private LineRenderer lineRenderer;
	private Vector3[] positions;

	[SerializeField]
	private float lineWidth = 1.0f;
	
	[SerializeField]
	private float pointScale = 1.0f;

	private Distance distanceText;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		positions = new Vector3[transform.childCount];

		distanceText = CoordinateNumbers.Instance.CreateDistanceNumber();
	}

	private void Update()
	{
		UpdateLinePositions();
	}

	private void UpdateLinePositions()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			positions[i] = transform.GetChild(i).position;
		}

		lineRenderer.SetPositions(positions);

		if (Application.isPlaying)
		{
			var lineCenter = Vector3.Lerp(positions[0], positions[1], 0.5f);
			distanceText.AlignWithWorldPosition(lineCenter);

			var closestPointDistance = float.MaxValue;
			for (int i = 0; i < positions.Length; i++)
			{
				var cameraDistanceToPoint = Vector3.Distance(positions[i], CameraModeChanger.Instance.ActiveCamera.transform.position);
				transform.GetChild(i).transform.localScale = Vector3.one * cameraDistanceToPoint * pointScale;

				if (cameraDistanceToPoint < closestPointDistance)
					closestPointDistance = cameraDistanceToPoint;
			}	

			lineRenderer.startWidth = lineRenderer.endWidth = lineWidth * closestPointDistance;

			distanceText.DrawDistance(Vector3.Distance(positions[0], positions[1]), "m");
		}
	}
}
