using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SelectableMesh : MonoBehaviour
{
    [SerializeField]
    private MeasurePoint prefab;
    private Mesh mesh;

    [SerializeField]
    private float pointScale = 1.0f;
    private MeasurePoint[] points = new MeasurePoint[0];
    public MeasurePoint ActivePoint { get; private set; }

    public void SelectVertices()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        points = new MeasurePoint[mesh.vertices.Length];
        var verts = mesh.vertices; //cache a copy
        for (int i = 0; i < verts.Length; i++)
        {
            var transformedPosition = transform.rotation * verts[i] + transform.position;
            var point = Instantiate(prefab, transformedPosition, Quaternion.identity, transform);
            point.ChangeShape(MeasurePoint.Shape.NONE);
            point.SetSelectable(true);

            points[i] = point;
        }
    }

    public void DeselectVertices() //todo: call this somewehre
    {
        foreach (var point in points)
        {
            Destroy(point.gameObject);
        }
        points = new MeasurePoint[0];
    }

    private void Start()
    {
        SelectVertices();//todo: move this from start to somewhere else
    }

    private void Update()
    {
        foreach (var point in points)
        {
            AutoScalePointByDistance(point.transform);
        }

        ProcessUserInput();
    }

    private void ProcessUserInput()
    {
        var ray = ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.ScreenPointToRay(Input.mousePosition);
        var cast = Physics.Raycast(ray, out var hitinfo, LayerMask.NameToLayer("BoundaryFeatures"));
        if (cast)
        {
            var point = hitinfo.collider.GetComponent<MeasurePoint>();
            if (point)
            {
                DeselectActivePoint();
                SelectPoint(point);
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
            ActivePoint.ChangeShape(MeasurePoint.Shape.NONE);
    }

    private void SelectPoint(MeasurePoint newPoint)
    {
        ActivePoint = newPoint;
        ActivePoint.ChangeShape(MeasurePoint.Shape.POINT);
    }

    private float AutoScalePointByDistance(Transform point)
    {
        var cameraDistanceToPoint = Vector3.Distance(point.position, ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position);
        point.transform.localScale = Vector3.one * cameraDistanceToPoint * pointScale;

        return cameraDistanceToPoint;
    }
}
