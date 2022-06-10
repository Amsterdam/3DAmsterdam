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

    private bool measureToolActive;
    private UitbouwBase uitbouw;

    [SerializeField]
    private MeasurePoint cursorPointPrefab;

    protected override void Awake()
    {
        numberOfLines = 0;
        lines = new List<BuildingMeasuring>();
        uitbouw = GetComponent<UitbouwBase>();
    }

    private void Start()
    {
        otherSelectableMeshes = new SelectableMesh[2];
        otherSelectableMeshes[0] = RestrictionChecker.ActivePerceel.GetComponentInChildren<SelectableMesh>();
        otherSelectableMeshes[1] = RestrictionChecker.ActiveBuilding.GetComponentInChildren<SelectableMesh>();
    }

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

        if (!measureToolActive && uitbouw.Gizmo.Mode == GizmoMode.Measure)
            SetAllowMeasurement(true);
        else if (measureToolActive && uitbouw.Gizmo.Mode != GizmoMode.Measure)
            SetAllowMeasurement(false);

        if (measureToolActive)
            HandleUserInput();
    }

    private void HandleUserInput()
    {
        var ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
        var planeIntersect = RestrictionChecker.ActivePerceel.PerceelPlane.Raycast(ray, out float enter);

        bool isValidMeshPoint = IsHoveringOverValidPoint(out var hoverPoint);

        if (isValidMeshPoint)
            mousePositionInWorld = hoverPoint.transform.position;
        else if (planeIntersect)
            mousePositionInWorld = ray.origin + ray.direction * enter;


        //if (firstPointExists)
        //    cursorEndPoint.transform.position = mousePositionInWorld;
        //else
        //    cursorStartPoint.transform.position = mousePositionInWorld;

        //print("first point exists: " + firstPointExists);

        // if clicking: if a valid first point is already present, and the click is on a valid second point: finalize the line. else: if clicking on a valid first point, start the line.
        if (Input.GetMouseButtonDown(0))
        {
            if (!isValidMeshPoint)
            {
                hoverPoint = Instantiate(cursorPointPrefab);
                hoverPoint.transform.position = mousePositionInWorld;
            }

            if (firstPoint)
            {
                //if (IsHoveringOverValidSecondPoint(out var point))
                //    measureLines[measureLines.Count - 1] = new MeasureLine(firstPoint, point); //set the end point of the last line in the list
                //else
                measureLines[measureLines.Count - 1] = new MeasureLine(firstPoint, hoverPoint); //set the end point of the last line in the list

                firstPoint = null; //set that there is no longer an active first point
                print("finishing line " + measureLines.Count);
            }
            else
            {
                firstPoint = hoverPoint;
                if (isValidMeshPoint)
                    firstPoint.GetComponentInParent<SelectableMesh>().VisualizeActivePoint = false;
                CreateLine();
            }
        }
    }

    private bool IsHoveringOverValidPoint(out MeasurePoint point)
    {
        point = null;
        if (mySelectableMesh.ActivePoint)
        {
            mySelectableMesh.VisualizeActivePoint = true;
            point = mySelectableMesh.ActivePoint;
            return true;
        }
        else
        {
            foreach (var mesh in otherSelectableMeshes)
            {
                mySelectableMesh.VisualizeActivePoint = true;
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
            foreach (var otherMesh in otherSelectableMeshes)
            {
                if (otherMesh.ActivePoint)
                {
                    point = otherMesh.ActivePoint;
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

    private void CreateLine()
    {
        var line = new MeasureLine(firstPoint, null);
        measureLines.Add(line); //create the line, by setting the end point to null, the mouse position will be used as endpoint in DrawLines()
        var newLine = CreateNewMeasurement();
        newLine.DeleteButtonPressed += NewLine_DeleteButtonPressed;
        lines.Add(newLine);
        numberOfLines++;
    }

    private void NewLine_DeleteButtonPressed(BuildingMeasuring source)
    {
        DeleteLine(source);
    }

    public void SetAllowMeasurement(bool allowed)
    {
        //DrawDistanceActive = allowed && !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall;
        measureToolActive = allowed && !ServiceLocator.GetService<T3DInit>().HTMLData.SnapToWall;

        if (measureToolActive)
        {
            foreach (var mesh in otherSelectableMeshes)
            {
                mesh.SelectVertices();
            }

            mySelectableMesh.SelectVertices();
        }
        else
        {
            foreach (var mesh in otherSelectableMeshes)
            {
                mesh.DeselectVertices();
            }

            mySelectableMesh.DeselectVertices();
        }
    }

    void DeleteLine(int index)
    {
        Destroy(lines[index]);
        lines.RemoveAt(index);
        measureLines.RemoveAt(index);
        numberOfLines--;
    }

    void DeleteLine(MeasureLine line)
    {
        var index = measureLines.IndexOf(line);
        DeleteLine(index);
    }

    void DeleteLine(BuildingMeasuring line)
    {
        var index = lines.IndexOf(line);
        DeleteLine(index);
    }
}
