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

    private List<MeasureLine> measureLines = new List<MeasureLine>();
    private MeasurePoint firstPoint;
    //private MeasurePoint secondPoint;
    private Vector3 mousePositionInWorld;

    private UitbouwBase uitbouw;

    protected override void Awake()
    {
        numberOfLines = 0;
        lines = new List<BuildingMeasuring>();
        uitbouw = GetComponent<UitbouwBase>();
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
            //print("end exists: " + (measureLines[i].End != null));
            var end = measureLines[i].End != null ? measureLines[i].End.transform.position : mousePositionInWorld;
            lines[i].SetLinePosition(start, end);
        }
    }

    protected override void Measuring_DistanceInputOverride(BuildingMeasuring source, Vector3 direction, float delta)
    {
        //flip direction if points selected in wrong order
        var index = lines.IndexOf(source);
        if (measureLines[index].End.GetComponentInParent<SelectableMesh>() == mySelectableMesh)
            delta *= -1; 
        uitbouw.GetComponent<UitbouwMovement>().SetPosition(uitbouw.transform.position + direction * delta);
    }

    protected override void Update()
    {
        base.Update();
        if (DrawDistanceActive)
            HandleUserInput();

        if (Input.GetKeyDown(KeyCode.L))
            SetAllowMeasurement(!DrawDistanceActive);

        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var line in lines)
            {
                Destroy(line.gameObject);
            }
            lines = new List<BuildingMeasuring>();
            measureLines = new List<MeasureLine>();
        }
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
                var line = new MeasureLine(firstPoint, null);
                measureLines.Add(line); //create the line, by setting the end point to null, the mouse position will be used as endpoint in DrawLines()
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
                    print("hovering over valid second opint");
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
        Debug.Log("first point", firstPoint);
        if (firstPoint.GetComponentInParent<SelectableMesh>() == mySelectableMesh)
        {
            print("first point is my mesh");
            foreach (var otherMesh in otherSelectableMeshes)
            {
                Debug.Log("checking: ", otherMesh);
                if (otherMesh.ActivePoint)
                {
                    Debug.Log("second point is other mesh: " + otherMesh, otherMesh.ActivePoint);
                    point = otherMesh.ActivePoint;
                    return true;
                }
            }
        }
        // else check only uitbouwMesh
        else if(mySelectableMesh.ActivePoint)
        {
            point = mySelectableMesh.ActivePoint;
            print("second point is my mesh");
            return true;
        }
        return false;
    }

    public void SetAllowMeasurement(bool allowed)
    {
        DrawDistanceActive = allowed && !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall;

        if (allowed)
        {
            otherSelectableMeshes = new SelectableMesh[1];
            otherSelectableMeshes[0] = RestrictionChecker.ActivePerceel.GetComponentInChildren<SelectableMesh>();
            otherSelectableMeshes[0].SelectVertices();

            mySelectableMesh.SelectVertices();
        }
    }
}
