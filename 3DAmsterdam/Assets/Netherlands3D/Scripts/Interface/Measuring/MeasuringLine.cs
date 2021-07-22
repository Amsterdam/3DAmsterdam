using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.Interface;
using Netherlands3D.LayerSystem;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
	private AssetbundleMeshLayer[] targetLayers;

	[SerializeField]
	private List<Transform> linePoints;

	[SerializeField]
	private Transform previewTargetPoint;

	[SerializeField]
	private Material lineMaterial;

	[SerializeField]
	private Material linePlacedMaterial;

	private Mesh targetMesh;
	private bool newColliderFound = false;
	private MeshCollider targetCollider;

	[SerializeField]
	private int maxVertexSnapPerFrame = 10000;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		positions = new Vector3[linePoints.Count];

		StartCoroutine(SnapToClosestVertex());
	}

	private void OnEnable()
	{
		HelpMessage.Instance.Show("<b>Klik</b> om een beginpunt te plaatsen.\nDruk op <b>Escape</b> om te annuleren.");

		HideLineAndPoints(); //Start hidden, wait for clicks
		TakeInteractionPriority();

		Selector.Instance.registeredClickInput.AddListener(PlacePoint);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		HideLineAndPoints();
		Selector.Instance.registeredClickInput.RemoveListener(PlacePoint);
	}

	public override void Escape()
	{
		HideLineAndPoints();		
		gameObject.SetActive(false);
	}

	private void DisplayPoint(Transform targetPoint)
	{
		targetPoint.gameObject.SetActive(true);
	}

	private void HideLineAndPoints()
	{
		placementStepIndex = -1;
		lineRenderer.enabled = false;
		lineRenderer.material = lineMaterial;

		for (int i = 1; i < linePoints.Count; i++)
		{
			linePoints[i].gameObject.SetActive(false);
		} 
		if(distanceText) Destroy(distanceText.gameObject);
	}

	private void PlacePoint()
	{
		Vector3 placementPoint = previewTargetPoint.position;
		StepThroughPlacement(placementPoint);
	}

	private void PreviewNextPoint()
	{
		Vector3 previewPoint = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
		PrepareColliders(previewPoint);

		//Shoot ray to get precise point on collider
		if (Physics.Raycast(Selector.mainSelectorRay, out RaycastHit hit))
		{
			var hitCollider = hit.collider.GetComponent<MeshCollider>();
			if(hitCollider != targetCollider)
			{
				targetCollider = hitCollider;
				newColliderFound = true;
			}

			if(targetCollider)
				targetMesh = targetCollider.sharedMesh;

			previewPoint = hit.point;
		}

		previewTargetPoint.position = previewPoint;
		AutoScalePointByDistance(previewTargetPoint);

		//Move the next point in line to the previewposition if it was not the last point
		if (placementStepIndex == 0)
		{
			linePoints[1].transform.position = previewPoint;
			positions[1] = previewPoint;
		}
	}

	private void PrepareColliders(Vector3 placementPoint)
	{
		//Make sure our targetlayers have colliders
		foreach (var hitTargetLayer in targetLayers)
		{
			//Make sure all tiles close to mouse have a collider
			hitTargetLayer.AddMeshColliders(placementPoint);
		}
	}

	/// <summary>
	/// Placement steps where 0 is clear/nothing shown, and the others place the points, and show the line
	/// </summary>
	/// <param name="placementPoint"></param>
	private void StepThroughPlacement(Vector3 placementPoint)
	{
		placementStepIndex++;
		if (placementStepIndex == linePoints.Count) placementStepIndex = 0;

		print("Measure step index: " + placementStepIndex);
		if (placementStepIndex == 0)
		{
			//Change line to dotted line
			lineRenderer.material = lineMaterial;
			lineRenderer.enabled = true;
			foreach (var linePoint in linePoints) linePoint.position = placementPoint;

			HelpMessage.Instance.Show("<b>Klik</b> ergens anders om een eindpunt te plaatsen.\nDruk op <b>Escape</b> om te annuleren.");
		}
		else
		{
			lineRenderer.material = linePlacedMaterial;
			linePoints[placementStepIndex].transform.position = placementPoint;
		}
		DisplayPoint(linePoints[placementStepIndex]);
	}

	private void Update()
	{
		//Always preview next point
		if (!Selector.doingMultiselect)
		{
			PreviewNextPoint();
		}

		if (placementStepIndex > -1)
		{
			UpdateLinePositions();
			DrawAutoScalingLine();
		}
	}

	private IEnumerator SnapToClosestVertex()
	{
		while (Selector.doingMultiselect && newColliderFound)
		{
			Debug.Log("Trying to snap");
			newColliderFound = false;

			var closestDistance = float.MaxValue;
			var closestVertex = Vector3.one * float.MaxValue;

			//Check all mesh verts tied to this triangle for distance;
			for (int i = 0; i < targetMesh.vertices.Length; i++)
			{
				if ((i % maxVertexSnapPerFrame) == 0)
				{
					yield return new WaitForEndOfFrame();
				}

				var vertexWorldCoordinate = targetCollider.transform.TransformPoint(targetMesh.vertices[i]);
				var vertexDistance = Vector3.Distance(vertexWorldCoordinate, previewTargetPoint.transform.position);
				if (vertexDistance < closestDistance)
					closestVertex = vertexWorldCoordinate;
			}

			previewTargetPoint.transform.position = closestVertex;
		}
	}

	private void DrawAutoScalingLine()
	{
		//Autoscaling of line and points, based on point positions from camera
		var closestPointDistance = float.MaxValue;
		for (int i = 0; i < positions.Length; i++)
		{
			var point = linePoints[i];
			var pointDistance = AutoScalePointByDistance(point);

			if (pointDistance < closestPointDistance)
				closestPointDistance = pointDistance;
		}
		lineRenderer.startWidth = lineRenderer.endWidth = lineWidth * closestPointDistance;
	}

	private float AutoScalePointByDistance(Transform point)
	{
		var cameraDistanceToPoint = Vector3.Distance(point.position, CameraModeChanger.Instance.ActiveCamera.transform.position);
		point.transform.localScale = Vector3.one * cameraDistanceToPoint * pointScale;

		return cameraDistanceToPoint;
	}

	private void UpdateLinePositions()
	{
		//Update our positions array we can feed the linerenderer
		for (int i = 0; i < linePoints.Count; i++)
		{
			positions[i] = linePoints[i].position;
		}
		lineRenderer.SetPositions(positions);

		//Draw the distance text on top of our line if points differ
		if (positions[0] != positions[1])
		{
			var lineCenter = Vector3.Lerp(positions[0], positions[1], 0.5f);
			if (!distanceText) distanceText = CoordinateNumbers.Instance.CreateDistanceNumber();

			distanceText.AlignWithWorldPosition(lineCenter);
			var distanceMeasured = Vector3.Distance(positions[0], positions[1]);
			distanceText.DrawDistance(distanceMeasured, "m");
			distanceText.ResetInput();

			HelpMessage.Instance.Show($"De gemeten afstand is <b>~{distanceMeasured:F2}</b> meter.\n\n<b>Klik</b> om een nieuw begintpunt te plaatsen of\ndruk op <b>Escape</b> om te annuleren.");
		}
		else if (distanceText)
		{
			Destroy(distanceText.gameObject);
		}
	}
}
