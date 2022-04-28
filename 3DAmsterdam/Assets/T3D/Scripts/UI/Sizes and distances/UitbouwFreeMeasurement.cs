using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.T3D;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;

public struct MeasureLine
{
    public MeasurePoint Start;
    public MeasurePoint End;

    public MeasureLine(MeasurePoint start, MeasurePoint end)
    {
        Start = start;
        End = end;
    }
}

public class UitbouwFreeMeasurement : DistanceMeasurement
{
    [SerializeField]
    private SelectableMesh mySelectableMesh;
    private SelectableMesh[] otherSelectableMeshes;

    private List<MeasureLine> measureLines;
    private MeasurePoint firstPoint;
    //private MeasurePoint secondPoint;
    private Vector3 mousePositionInWorld;

    protected override void Awake()
    {
        numberOfLines = 0;
        lines = new List<BuildingMeasuring>();
    }

    //private void Start()
    //{
    //    mySelectableMesh = GetComponent<SelectableMesh>();
    //}

    protected override void DrawLines()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            var start = measureLines[i].Start.transform.position;
            var end = measureLines[i].End ? measureLines[i].End.transform.position : mousePositionInWorld;
            lines[i].SetLinePosition(start, end);
        }
    }

    protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
    {
        //todo: update position after user input 
    }

    protected override void Update()
    {
        base.Update();
        if (DrawDistanceActive)
            HandleUserInput();

        if (Input.GetKeyDown(KeyCode.L))
            SetAllowMeasurement(!DrawDistanceActive);
    }

    private void HandleUserInput()
    {
        // if clicking: if a valid first point is already present, and the click is on a valid second point: finalize the line. else: if clicking on a valid first point, start the line.
        if (Input.GetMouseButtonDown(0))
        {
            if (firstPoint)
            {
                if (IsHoveringOverValidSecondPoint(out var point))
                {
                    measureLines[measureLines.Count - 1] = new MeasureLine(firstPoint, point); //set the end point of the last line in the list
                    firstPoint = null; //set that there is no longer an active first point
                }
            }
            else if (IsHoveringOverValidPoint(out var point))
            {
                firstPoint = point;
                measureLines.Add(new MeasureLine(firstPoint, null)); //create the line, by setting the end point to null, the mouse position will be used as endpoint in DrawLines()
                lines.Add(CreateNewMeasurement());
                numberOfLines++;
            }
        }
        else //if not clicking: if there is no first point do nothing, otherwise the second point should be at mouse position, unless hovering over a MeasurePoint.
        {
            if (firstPoint)
            {
                if (IsHoveringOverValidSecondPoint(out var point))
                {
                    mousePositionInWorld = point.transform.position;
                }
                else
                {
                    var ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
                    var planeIntersect = RestrictionChecker.ActivePerceel.PerceelPlane.Raycast(ray, out float enter);
                    if (planeIntersect)
                    {
                        mousePositionInWorld = ray.origin + ray.direction * enter;
                    }
                }
            }
        }
    }

    private bool IsHoveringOverValidPoint(out MeasurePoint point)
    {
        point = null;
        if (mySelectableMesh.ActivePoint)
        {
            point = mySelectableMesh.ActivePoint;
            return true;
        }
        else
        {
            foreach (var mesh in otherSelectableMeshes)
            {
                if (mesh.ActivePoint)
                {
                    point = mesh.ActivePoint;
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsHoveringOverValidSecondPoint(out MeasurePoint point)
    {
        point = null;
        // if first point is part of uitbouwMesh: only check other meshes.
        if (firstPoint.GetComponentInParent<SelectableMesh>() == mySelectableMesh)
        {
            foreach (var mesh in otherSelectableMeshes)
            {
                if (mesh.ActivePoint)
                {
                    point = mesh.ActivePoint;
                    return true;
                }
            }
        }
        // else check only uitbouwMesh
        else if (mySelectableMesh.ActivePoint)
        {
            point = mySelectableMesh.ActivePoint;
            return true;
        }
        return false;
    }

    public void SetAllowMeasurement(bool allowed)
    {
        DrawDistanceActive = allowed && !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall;

        if (allowed)
        {
            otherSelectableMeshes = new SelectableMesh[2];
            otherSelectableMeshes[0] = RestrictionChecker.ActivePerceel.GetComponentInChildren<SelectableMesh>();
            otherSelectableMeshes[0].SelectVertices();

            mySelectableMesh.SelectVertices();
        }
    }
}
