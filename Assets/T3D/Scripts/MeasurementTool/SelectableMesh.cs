using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.Cameras;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
public class SelectableMesh : MonoBehaviour
{
    [SerializeField]
    private MeasurePoint vertexVisualizationPrefab;
    [SerializeField]
    private MeshFilter meshFilterOverride;

    private MeshFilter activeMeshFilter;
    private Mesh mesh;

    [SerializeField]
    private float pointScale = 1.0f;
    private MeasurePoint[] points = new MeasurePoint[0];
    public MeasurePoint ActivePoint { get; private set; }
    public bool VisualizeActivePoint { get; set; } = true;

    private bool arePointsGenerated = false;

    [SerializeField]
    private bool limitVerticesToGroundPlane;
    private Plane vertexLimitPlane;
    [SerializeField]
    private float vertexLimitTolerance = 0.1f;

    public void GenerateVertPoints()
    {
        if (meshFilterOverride)
            activeMeshFilter = meshFilterOverride;
        else
            activeMeshFilter = GetComponent<MeshFilter>();

        mesh = activeMeshFilter.mesh;

        var pos = activeMeshFilter.transform.position + activeMeshFilter.transform.rotation * activeMeshFilter.mesh.bounds.min;
        //vertexLimitPlane = new Plane(Vector3.up, activeMeshFilter.mesh.bounds.center - activeMeshFilter.mesh.bounds.min);
        vertexLimitPlane = new Plane(Vector3.up, pos);

        var pointList = new List<MeasurePoint>();
        var verts = mesh.vertices; //cache a copy
        for (int i = 0; i < verts.Length; i++)
        {
            var transformedPosition = (activeMeshFilter.transform.rotation * verts[i].Multiply(activeMeshFilter.transform.lossyScale)) + activeMeshFilter.transform.position;
            var conformsToLimit = !limitVerticesToGroundPlane || IsWithinGroundPlaneLimit(transformedPosition);

            if (conformsToLimit)
            {
                var point = Instantiate(vertexVisualizationPrefab, transformedPosition, Quaternion.identity, transform);
                point.ChangeShape(MeasurePoint.Shape.NONE);
                point.SetSelectable(true);
                pointList.Add(point);
            }
        }
        points = pointList.ToArray();
    }

    private bool IsWithinGroundPlaneLimit(Vector3 transformedPosition)
    {
        return vertexLimitPlane.GetDistanceToPoint(transformedPosition) <= vertexLimitTolerance;
    }

    public void SelectVertices()
    {
        if (arePointsGenerated)
        {
            foreach (var point in points)
            {
                point.gameObject.SetActive(true);
            }
        }
        else
        {
            GenerateVertPoints();
        }
    }

    public void DeselectVertices()
    {
        foreach (var point in points)
        {
            point.gameObject.SetActive(false);
        }
        //points = new MeasurePoint[0];
    }

    private void Update()
    {
        ProcessUserInput();
    }

    private void ProcessUserInput()
    {
        var ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
        var cast = Physics.Raycast(ray, out var hitinfo, Mathf.Infinity, LayerMask.GetMask("SelectionPoints"));
        if (cast)
        {
            var point = hitinfo.collider.GetComponent<MeasurePoint>();
            if (point && point != ActivePoint)
            {
                if (points.Contains(point))
                {
                    DeselectActivePoint();
                    SelectPoint(point, VisualizeActivePoint);
                }
            }
            else if (!point || !points.Contains(point))
            {
                DeselectActivePoint();
            }
        }
        else
        {
            DeselectActivePoint();
        }
    }

    private void DeselectActivePoint()
    {
        if (ActivePoint)
        {
            ActivePoint.ChangeShape(MeasurePoint.Shape.NONE);
            ActivePoint = null;
        }
    }

    private void SelectPoint(MeasurePoint newPoint, bool visualize)
    {
        ActivePoint = newPoint;
        if (visualize)
            ActivePoint.ChangeShape(MeasurePoint.Shape.POINT);
    }
}
