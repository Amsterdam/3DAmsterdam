using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.Interface;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingMeasuring : Interactable
{
    private LineRenderer lineRenderer;

    private Vector3[] positions;

    [SerializeField]
    private float lineWidth = 1.0f;

    [SerializeField]
    private float pointScale = 1.0f;

    private NumberInputField distanceLabel;

    private int placementStepIndex = -1;

    [SerializeField]
    private List<MeasurePoint> linePoints;
    public List<MeasurePoint> LinePoints => linePoints;

    [SerializeField]
    private Material lineMaterial;

    [SerializeField]
    private Material linePlacedMaterial;

    [SerializeField]
    private Material lineSelectedMaterial;

    private MeshCollider targetCollider;

    public float Distance { get { return Vector3.Distance(positions[0], positions[1]); } }

    public delegate void DistanceInputOverrideEventHandler(BuildingMeasuring source, Vector3 direction, float delta);
    public event DistanceInputOverrideEventHandler DistanceInputOverride;
    public delegate void DeleteButtonPressedEventHandler(BuildingMeasuring source);
    public event DeleteButtonPressedEventHandler DeleteButtonPressed;

    private bool deleteable;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        positions = new Vector3[linePoints.Count];
    }

    private void OnEnable()
    {
        //ServiceLocator.GetService<HelpMessage>().Show("<b>Klik</b> om een beginpunt te plaatsen.\nDruk op <b>Escape</b> om te annuleren.");

        ResetLine(); //Start hidden, wait for clicks
                     //TakeInteractionPriority();

        PlacePoint(transform.position);
        PlacePoint(transform.position);

        //ServiceLocator.GetService<Selector>().registeredClickInput.AddListener(PlacePoint);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ResetLine();
        //ServiceLocator.GetService<Selector>().registeredClickInput.RemoveListener(PlacePoint);
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

        if (distanceLabel)
        {
            distanceLabel.DistanceInputOverride -= DistanceLabel_DistanceInputOverride;
            distanceLabel.DeleteButtonPressed -= DistanceLabel_DeleteButtonPressed;
            Destroy(distanceLabel.gameObject);
        }
    }

    public void PlacePoint(Vector3 placementPoint)
    {
        //Vector3 placementPoint = (Selector.doingMultiselect && placementStepIndex == 0) ? linePoints[1].transform.position : previewTargetPoint.position;
        StepThroughPlacement(placementPoint);
    }

    public void SetLinePosition(Vector3 start, Vector3 end)
    {
        if (linePoints.Count != 2)
            Debug.LogError("Line needs to have 2 points");

        linePoints[0].transform.position = start;
        linePoints[1].transform.position = end;
    }

    private void PreviewNextPoint()
    {
        Vector3 previewPoint = ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.GetPointerPositionInWorld();

        //Shoot ray to get precise point on collider
        if (Physics.Raycast(Selector.mainSelectorRay, out RaycastHit hit))
        {
            var hitCollider = hit.collider.GetComponent<MeshCollider>();
            if (hitCollider && hitCollider != targetCollider)
            {
                targetCollider = hitCollider;
                //targetMesh = targetCollider.sharedMesh;
            }
            previewPoint = hit.point;
        }

        //Move the next point in line to the previewposition if it was not the last point
        if (placementStepIndex == -1 && Selector.doingMultiselect)
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
            else
            {
                linePoints[1].transform.position = previewPoint;
                ChangeLineType(MeasurePoint.Shape.POINT);
            }
        }

        //previewTargetPoint.position = previewPoint;
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
            UpdateLineColor();
        }
    }

    private void UpdateLineColor()
    {
        if (distanceLabel && distanceLabel.IsSelected)
        {
            lineRenderer.material = lineSelectedMaterial;
        }
        else
        {
            lineRenderer.material = lineMaterial;
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
            if (!distanceLabel)
            {
                distanceLabel = ServiceLocator.GetService<CoordinateNumbers>().CreateNumberInputField();
                distanceLabel.DistanceInputOverride += DistanceLabel_DistanceInputOverride;
                distanceLabel.DeleteButtonPressed += DistanceLabel_DeleteButtonPressed;
                distanceLabel.EnableDeleteButton(deleteable);
                //distanceLabel.EnableDeleteButton(!ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall && RestrictionChecker.ActiveUitbouw.Gizmo.Mode != GizmoMode.MoveHeight); //todo: move this elsewhere
            }


            distanceLabel.Distance.AlignWithWorldPosition(lineCenter);
            var distanceMeasured = Vector3.Distance(positions[0], positions[1]);

            if (!distanceLabel.GetComponentInChildren<UnityEngine.UI.InputField>().isFocused)
            {
                distanceLabel.Distance.DrawDistance(distanceMeasured * 100, "", 0);
                distanceLabel.Distance.ResetInput();
            }

            //ServiceLocator.GetService<HelpMessage>().Show($"De gemeten afstand is <color=#39cdfe><b>~{distanceMeasured:F2}</b></color> meter.\n<b>Klik</b> om de lijn te plaatsen.\nHoud <b>Shift</b> ingedrukt om alleen de hoogte te meten. \nDruk op <b>Escape</b> om te annuleren.");
        }
        else if (distanceLabel)
        {
            distanceLabel.DistanceInputOverride -= DistanceLabel_DistanceInputOverride;
            distanceLabel.DeleteButtonPressed -= DistanceLabel_DeleteButtonPressed;
            Destroy(distanceLabel.gameObject);
        }
    }


    private void DistanceLabel_DistanceInputOverride(NumberInputField source, float distance)
    {
        var direction = positions[0] - positions[1];
        var distanceMeasured = direction.magnitude;
        var difference = distance - distanceMeasured;
        DistanceInputOverride.Invoke(this, direction.normalized, difference);
    }

    private void DistanceLabel_DeleteButtonPressed(NumberInputField source)
    {
        DeleteButtonPressed?.Invoke(this);
    }

    public void SetDistanceLabelInteractable(bool interactable)
    {
        if (distanceLabel)
            distanceLabel.SetInteractable(interactable);
    }

    public void EnableDeleteButton(bool enabled)
    {
        deleteable = enabled;
        if (distanceLabel)
            distanceLabel.EnableDeleteButton(enabled);
    }
}
