using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.Interface;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasuringLine : Interactable
{
	private LineRenderer lineRenderer;

	private Vector3[] positions;

	[SerializeField]
	private float lineWidth = 1.0f;
	
	[SerializeField]
	private float pointScale = 1.0f;

	private Distance distanceText;

	private int placementStepIndex = -1;

	[SerializeField]
	private List<MeasurePoint> linePoints;

	[SerializeField]
	private MeasurePoint previewTargetPoint;

	[SerializeField]
	private Material lineMaterial;

	[SerializeField]
	private Material linePlacedMaterial;

	private Mesh targetMesh;
	//private bool newColliderFound = false;
	private MeshCollider targetCollider;

	[SerializeField]
	private int maxVertexSnapPerFrame = 10000;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		positions = new Vector3[linePoints.Count];

		//StartCoroutine(SnapToClosestVertex()); //Implementation of vertex snapping with modifier. Way too slow. Might be improved/usable later.
	}

	private void OnEnable()
	{
		ServiceLocator.GetService<HelpMessage>().Show("<b>Klik</b> om een beginpunt te plaatsen.\nDruk op <b>Escape</b> om te annuleren.");

		ResetLine(); //Start hidden, wait for clicks
		TakeInteractionPriority();

		ServiceLocator.GetService<Selector>().registeredClickInput.AddListener(PlacePoint);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ResetLine();
		ServiceLocator.GetService<Selector>().registeredClickInput.RemoveListener(PlacePoint);
	}

	public override void Escape()
	{
		ResetLine();		
		gameObject.SetActive(false);
	}

	private void DisplayPoints()
	{
		foreach (var linePoint in linePoints) linePoint.gameObject.SetActive(true);
	}

	private void ResetLine()
	{
		placementStepIndex = -1;
		lineRenderer.enabled = false;
		lineRenderer.material = lineMaterial;
		foreach (var linePoint in linePoints) linePoint.gameObject.SetActive(false);

		if (distanceText) Destroy(distanceText.gameObject);
	}

	private void PlacePoint()
	{
		Vector3 placementPoint = (Selector.doingMultiselect && placementStepIndex == 0) ? linePoints[1].transform.position : previewTargetPoint.transform.position;
		StepThroughPlacement(placementPoint);
	}

	private void PreviewNextPoint()
	{
		Vector3 previewPoint = ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.GetPointerPositionInWorld();

		//Shoot ray to get precise point on collider
		if (Physics.Raycast(Selector.mainSelectorRay, out RaycastHit hit))
		{
			var hitCollider = hit.collider.GetComponent<MeshCollider>();
			if(hitCollider != targetCollider)
			{
				targetCollider = hitCollider;
				targetMesh = targetCollider.sharedMesh;
			}
			previewPoint = hit.point;
		}

		//Move the next point in line to the previewposition if it was not the last point
		if(placementStepIndex == -1 && Selector.doingMultiselect)
		{
			//Only do height, straight up when using modifier
			linePoints[1].transform.position = previewPoint;
			foreach (var linePoint in linePoints) linePoint.ChangeShape(MeasurePoint.Shape.HEIGHT);
		}
		else if (placementStepIndex == 0)
		{
			if (Selector.doingMultiselect)
			{
				//Only do height, straight up when using modifier
				linePoints[1].transform.position = new Vector3(linePoints[0].transform.position.x, previewPoint.y, linePoints[0].transform.position.z);
				ChangeLineType(MeasurePoint.Shape.HEIGHT);
			}
			else{
				linePoints[1].transform.position = previewPoint;
				ChangeLineType(MeasurePoint.Shape.POINT);
			}
		}

		previewTargetPoint.transform.position = previewPoint;
		previewTargetPoint.AutoScalePointByDistance();
	}

	private void ChangeLineType(MeasurePoint.Shape shape)
	{
		foreach (var linePoint in linePoints) linePoint.ChangeShape(shape);
	}


	/// <summary>
	/// Placement steps where 0 is clear/nothing shown, and the others place the points, and show the line
	/// </summary>
	/// <param name="placementPoint"></param>
	private void StepThroughPlacement(Vector3 placementPoint)
	{
		placementStepIndex++;
		if (placementStepIndex == linePoints.Count) placementStepIndex = 0;

		if (placementStepIndex == 0)
		{
			//Change line to dotted line
			lineRenderer.material = lineMaterial;
			lineRenderer.enabled = true;
			foreach (var linePoint in linePoints) linePoint.transform.position = placementPoint;
		}
		else
		{
			lineRenderer.material = linePlacedMaterial;
			linePoints[placementStepIndex].transform.position = placementPoint;
		}
		DisplayPoints();
	}

	private void Update()
	{
		//Always preview next point
		PreviewNextPoint();

		if (placementStepIndex > -1)
		{
			UpdateLinePositions();
			DrawAutoScalingLine();
		}
	}

	private IEnumerator SnapToClosestVertex()
	{
		while (true)
		{
			if (Selector.doingMultiselect && targetMesh)
			{
				Debug.Log("Trying to snap");
				Mesh runningThroughMesh = targetMesh;

				var closestDistance = float.MaxValue;
				var closestVertex = Vector3.one * float.MaxValue;

				//Check all mesh verts tied to this triangle for distance;
				for (int i = 0; i < targetMesh.vertices.Length; i++)
				{
					if (targetMesh != runningThroughMesh) yield break;

					var vertexWorldCoordinate = targetCollider.transform.TransformPoint(targetMesh.vertices[i]);
					var vertexDistance = Vector3.Distance(vertexWorldCoordinate, previewTargetPoint.transform.position);
					if (vertexDistance < closestDistance)
						closestVertex = vertexWorldCoordinate;

					if ((i % maxVertexSnapPerFrame) == 0)
					{
						yield return new WaitForEndOfFrame();
					}
				}

				//We need to have finished going through the vertices, before we can override out target position:
				previewTargetPoint.transform.position = closestVertex;
			}
			yield return null;
		}
	}

	private void DrawAutoScalingLine()
	{
		//Autoscaling of line and points, based on point positions from camera
		var closestPointDistance = float.MaxValue;
		for (int i = 0; i < linePoints.Count; i++)
		{
			var point = linePoints[i];
			var pointDistance = point.AutoScalePointByDistance();

			if (pointDistance < closestPointDistance)
				closestPointDistance = pointDistance;
		}
		lineRenderer.startWidth = lineRenderer.endWidth = lineWidth * closestPointDistance;
	}

	private void UpdateLinePositions()
	{
		//Update our positions array we can feed the linerenderer
		for (int i = 0; i < linePoints.Count; i++)
		{
			positions[i] = linePoints[i].transform.position;
		}
		lineRenderer.SetPositions(positions);

		//Draw the distance text on top of our line if points differ
		if (positions[0] != positions[1] || Selector.doingMultiselect)
		{
			var lineCenter = Vector3.Lerp(positions[0], positions[1], 0.5f);
			if (!distanceText) distanceText = ServiceLocator.GetService<CoordinateNumbers>().CreateDistanceNumber();

			distanceText.AlignWithWorldPosition(lineCenter);
			var distanceMeasured = Vector3.Distance(positions[0], positions[1]);
			distanceText.DrawDistance(distanceMeasured, "m");
			distanceText.ResetInput();

			ServiceLocator.GetService<HelpMessage>().Show($"De gemeten afstand is <color=#39cdfe><b>~{distanceMeasured:F2}</b></color> meter.\n<b>Klik</b> om de lijn te plaatsen.\nHoud <b>Shift</b> ingedrukt om alleen de hoogte te meten. \nDruk op <b>Escape</b> om te annuleren.");
		}
		else if (distanceText)
		{
			Destroy(distanceText.gameObject);
		}
	}
}
