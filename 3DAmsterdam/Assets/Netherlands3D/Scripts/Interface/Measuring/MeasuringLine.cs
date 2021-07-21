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

	private List<Transform> childPoints;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();

		childPoints = new List<Transform>();
		foreach (Transform child in transform) childPoints.Add(child);
		positions = new Vector3[childPoints.Count];
	}

	private void OnEnable()
	{
		HelpMessage.Instance.Show("<b>Klik</b> om een beginpunt te plaatsen.\nDruk op <b>Escape</b> om te annuleren.");

		HideLineAndPoints(); //Start hidden, wait for clicks
		TakeInteractionPriority();

		PrepareColliders(default);

		Selector.Instance.registeredClickInput.AddListener(PlacePoint);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Escape();
		Selector.Instance.registeredClickInput.RemoveListener(PlacePoint);
	}

	public override void Escape()
	{
		HideLineAndPoints();
		HelpMessage.Instance.Hide(true);
		
		gameObject.SetActive(false);
	}

	private void ShowPoint(Transform targetPoint)
	{
		lineRenderer.enabled = true;
		targetPoint.gameObject.SetActive(true);
	}
	private void HideLineAndPoints()
	{
		placementStepIndex = -1;
		lineRenderer.enabled = false;
		foreach (var point in childPoints) point.gameObject.SetActive(false);
		if(distanceText) Destroy(distanceText.gameObject);
	}

	private void PlacePoint()
	{
		Vector3 placementPoint = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
		PrepareColliders(placementPoint);

		//Shoot ray to get precise point on collider
		if (Physics.Raycast(Selector.mainSelectorRay, out RaycastHit hit))
		{
			placementPoint = hit.point;
		}

		StepThroughPlacement(placementPoint);
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
		print("Measure step: " + placementStepIndex);
		placementStepIndex++;
		if (placementStepIndex == childPoints.Count) placementStepIndex = 0;

		if (placementStepIndex == 0)
		{
			//First click places all points
			foreach (var childPoint in childPoints) childPoint.transform.position = placementPoint;
			HelpMessage.Instance.Show("<b>Klik</b> ergens anders om een eindpunt te plaatsen.\nDruk op <b>Escape</b> om te annuleren.");
		}
		else
		{
			childPoints[placementStepIndex].transform.position = placementPoint;
		}

		ShowPoint(childPoints[placementStepIndex]);
		UpdateLinePositions();
	}

	private void LateUpdate()
	{
		AutoScale();
	}

	private void AutoScale()
	{
		if (!lineRenderer.enabled) return;

		//Autoscaling of line and points, based on point positions from camera
		var closestPointDistance = float.MaxValue;
		for (int i = 0; i < positions.Length; i++)
		{
			var cameraDistanceToPoint = Vector3.Distance(positions[i], CameraModeChanger.Instance.ActiveCamera.transform.position);
			childPoints[i].transform.localScale = Vector3.one * cameraDistanceToPoint * pointScale;

			if (cameraDistanceToPoint < closestPointDistance)
				closestPointDistance = cameraDistanceToPoint;
		}
		lineRenderer.startWidth = lineRenderer.endWidth = lineWidth * closestPointDistance;
	}

	private void UpdateLinePositions()
	{
		//Update our positions array we can feed the linerenderer
		for (int i = 0; i < transform.childCount; i++)
		{
			positions[i] = transform.GetChild(i).position;
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

			HelpMessage.Instance.Show($"De gemeten afstand is <b>~{distanceMeasured:F2}</b> meter.\n\n<b>Klik</b> om een nieuw begintpunt te plaatsen of\ndruk op <b>Escape</b> om te annuleren.");
		}
		else if (distanceText)
		{
			Destroy(distanceText.gameObject);
		}
	}
}
