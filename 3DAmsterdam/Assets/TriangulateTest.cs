using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poly2Tri;
using Netherlands3D.Utilities;
using System.Linq;

public class TriangulateTest : MonoBehaviour
{
    public List<Vector3> boundaryVerts;
    public Material MeshMaterial;

    void Start()
    {
        TestPoy2Mesh();
        //TestGeometryCalculator(); 
        //TestGeometryCalculatorSquare();
    }

    void TestGeometryCalculator()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = boundaryVerts.ToArray();
        var points = boundaryVerts.Select(o => new Vector2(o.x, o.y)).ToArray();
        mesh.triangles = GeometryCalculator.Triangulate(points);
        mesh.RecalculateNormals();
        AddMeshGameObject("GeometryCalculator", mesh);
    }

    void TestGeometryCalculatorSquare()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(1, 1));
        points.Add(new Vector2(1, 0));
        points.Add(new Vector2(0, 0));
        points.Add(new Vector2(0, 1));

        Mesh mesh = new Mesh();
        mesh.vertices = points.Select(o => new Vector3(o.x, o.y, 0)).ToArray();        
        mesh.triangles = GeometryCalculator.Triangulate(points.ToArray());
        mesh.RecalculateNormals();
        AddMeshGameObject("GeometryCalculatorSquare", mesh);
    }

    void TestPoy2Mesh()
    {
        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

        //find holes, a hole is an inner list with head and same tail..

        Vector3 start = boundaryVerts[1];

        var inner = boundaryVerts.Select((s, i) => new { i, s }).Where( o => o.i > 1 && o.i < boundaryVerts.Count -1 ).ToArray();
        var tail = inner.FirstOrDefault(o => o.s.Equals(start));

        var subarr = boundaryVerts.Select((s, i) => new { i, s }).Where(o => o.i > 0 && o.i < tail.i).Select(s => new Point2D(s.s.x, s.s.y)).ToList();

        var windingorder = PolygonUtil.CalculateWindingOrder(subarr);

        poly.outside = boundaryVerts;

        var mesh = Poly2Mesh.CreateMesh(poly);
        AddMeshGameObject("test", mesh);

        for (int i = 0; i < boundaryVerts.Count; i++)
        {
            addBoundaryPoint(boundaryVerts[i]);
        }
    }

    void AddMeshGameObject(string name, Mesh mesh)
    {
        GameObject gam = new GameObject(name);
        var meshfilter = gam.AddComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;
        var meshrenderer = gam.AddComponent<MeshRenderer>();
        meshrenderer.material = MeshMaterial;
        gam.transform.SetParent(transform);
        

        
    }

    void addBoundaryPoint(Vector3 location)
    {
        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DestroyImmediate(point.GetComponent<SphereCollider>());
        point.transform.position = location;
        point.transform.localScale = Vector3.one * 0.1f;
       // point.transform.SetParent(uitbouw.transform);

    }


}
