using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Poly2Tri;
using Netherlands3D.Utilities;
using System.Linq;
using System;

public class TriangulateTest : MonoBehaviour
{
    public List<Vector3> boundaryVerts;
    public Material MeshMaterial;

    void Start()
    {
        TestPoly2Mesh();
        //TestGeometryCalculator(); 
        //TestGeometryCalculatorSquare();
        //TestPolyTrangulator();
        //TestEarClippingTrangulator();
    }

    //geeft Erro triangulating mesh. Aborted
    private void TestEarClippingTrangulator()
    {
        //var pointsVector2 = GetSquare();
        //var pointsVector2 = GetL();
        var pointsVector2 = GetNienkePattern();

        //pointsVector2.Reverse();
        //var pointsVector2 = GetO();

        var polygon = new Sebastian.Geometry.Polygon(pointsVector2.ToArray());
        
        var triangulator = new Sebastian.Geometry.Triangulator(polygon);
        var tris = triangulator.Triangulate();
        

        Mesh mesh = new Mesh();
        mesh.vertices = pointsVector2.Select(o => new Vector3(o.x, o.y)).ToArray();
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        AddMeshGameObject("TestEarClippingTrangulator", mesh);


    }

    //deze crasht bij intersected vertices
    private void TestPolyTrangulator()
    {
        //var pointsVector2 = GetSquare();
        //var pointsVector2 = GetL();
        //var pointsVector2 = GetC();
        //var pointsVector2 = GetO();
        var pointsVector2 = GetNienkePattern();

        addBoundaryPoints(pointsVector2);
        
        var points = pointsVector2.Select(o => new PointF() { X = o.x, Y = o.y }).ToList();
        var tris = PolygonTriangulator.Triangulate(points, true);
        var triangles = tris[0].Select(o => o.Index);

        Mesh mesh = new Mesh();
        mesh.vertices = points.Select(o => new Vector3(o.X, o.Y)).ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        AddMeshGameObject("TestPolyTrangulator", mesh);

    }

    //deze gaat al fout bij een L vorm
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
        //List<Vector2> points = GetSquarePoints();
        List<Vector2> points = GetL();
        //List<Vector2> points = GetNienkePattern();

        addBoundaryPoints(points);

        Mesh mesh = new Mesh();
        mesh.vertices = points.Select(o => new Vector3(o.x, o.y, 0)).ToArray();        
        mesh.triangles = GeometryCalculator.Triangulate(points.ToArray());
        mesh.RecalculateNormals();
        AddMeshGameObject("GeometryCalculatorSquare", mesh);
    }

    void TestPoly2Mesh()
    {
        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

        //poly.outside = boundaryVerts;
        //poly.outside = GetNienkePattern().Select(o => new Vector3(o.x, o.y, 0)).ToList();

        //var voorkant = GetVoorkant();
        //poly.outside = voorkant.Select(o => new Vector3(o.x, o.y, 0)).ToList();

        var outerAndInner = CityJsonMeshUtility.GetOuterAndInnerPolygons(GetVoorkant());

        
        poly.outside = outerAndInner[0].Select(o => new Vector3(o.x, o.y, 0)).ToList();

        for (int i = 1; i < outerAndInner.Count; i++)
        {
            poly.holes.Add(outerAndInner[i].Select(o => new Vector3(o.x, o.y, 0)).ToList());
        }


        var mesh = Poly2Mesh.CreateMesh(poly);
        AddMeshGameObject("test", mesh);

        //addBoundaryPoints(voorkant, Color.green);

        addBoundaryPoints(outerAndInner[0], Color.green);
        for (int i = 1; i < outerAndInner.Count; i++)
        {
            addBoundaryPoints(outerAndInner[i], Color.red);
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

    void addBoundaryPoints(List<Vector2> points, Color color)
    {
        for(int i = 0;i<points.Count; i++)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(point.GetComponent<SphereCollider>());
            point.name = i.ToString();
            point.transform.position = points[i] ;
            point.transform.localScale = Vector3.one * 0.1f;
            point.transform.SetParent(transform);
            point.GetComponent<MeshRenderer>().material.color = color;
        }
    }

    void addBoundaryPoints(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(point.GetComponent<SphereCollider>());
            point.name = i.ToString();
            point.transform.position = points[i];
            point.transform.localScale = Vector3.one * 0.1f;
            point.transform.SetParent(transform);
            point.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    List<Vector2> GetSquare()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(1, 1));
        points.Add(new Vector2(1, 0));
        points.Add(new Vector2(0, 0));
        points.Add(new Vector2(0, 1));
        return points;
    }

    List<Vector2> GetL()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(1, 0.25f));
        points.Add(new Vector2(1, 0));
        points.Add(new Vector2(0, 0));
        points.Add(new Vector2(0, 1));
        points.Add(new Vector2(0.25f, 1));
        points.Add(new Vector2(0.25f, 0.25f));
        points.Add(new Vector2(1, 0.25f));
        return points;
    }

    List<Vector2> GetC()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(1, 0.25f));
        points.Add(new Vector2(1, 0));
        points.Add(new Vector2(0, 0));
        points.Add(new Vector2(0, 1));
        points.Add(new Vector2(1, 1));
        points.Add(new Vector2(1, 0.75f));
        points.Add(new Vector2(0.25f, 0.75f));
        points.Add(new Vector2(0.25f, 0.25f));
        points.Add(new Vector2(1, 0.25f));
        return points;
    }

    List<Vector2> GetO()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(1, 0.25f));
        points.Add(new Vector2(1, 0));
        points.Add(new Vector2(0, 0));
        points.Add(new Vector2(0, 1));
        points.Add(new Vector2(1, 1));

        points.Add(new Vector2(1, 0.3f));
        points.Add(new Vector2(0.75f, 0.3f));

        points.Add(new Vector2(0.75f, 0.75f));
        points.Add(new Vector2(0.25f, 0.75f));
        points.Add(new Vector2(0.25f, 0.25f));
        points.Add(new Vector2(1, 0.25f));


        return points;
    }

    List<Vector2> GetNienkePattern()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(1, 0.25f));
        points.Add(new Vector2(1, 0));
        points.Add(new Vector2(0, 0));
        points.Add(new Vector2(0, 0.25f));
        points.Add(new Vector2(0.25f, 0.25f));
        points.Add(new Vector2(0.75f, 0.25f));
        points.Add(new Vector2(0.75f, 0.75f));
        points.Add(new Vector2(0.25f, 0.75f));
        points.Add(new Vector2(0.25f, 0.25f));
        points.Add(new Vector2(0, 0.25f));
        points.Add(new Vector2(0, 1));
        points.Add(new Vector2(1, 1));
        points.Add(new Vector2(1, 0.25f));
        return points;
    }

    List<Vector2> GetVoorkant()
    {
        List<Vector2> testdata = new List<Vector2>()
        {
            new Vector2(2.6f, 0.2f),//0
            new Vector2(2.7f, 0.2f),//1
            new Vector2(2.7f, 0),//2
            new Vector2(-2.7f, 0),//3
            new Vector2(-2.7f, 0.2f),//4
            new Vector2(-2.6f, 0.2f),//5
            new Vector2(-2.2f, 0.2f),//6
            new Vector2(-0.2f, 0.199f),//7
            new Vector2(-0.2f, 2.3f),//8
            new Vector2(-2.2f, 2.3f),//9
            new Vector2(-2.2f, 0.2f),//10
            new Vector2(-2.6f, 0.2f),//11
            new Vector2(-2.7f, 0.2f),//12
            new Vector2(-2.7f, 2.849f),//13
            new Vector2(-2.6f, 2.849f),//14
            new Vector2(-2.2f, 2.849f),//15
            new Vector2(2.2f, 2.849f),//16
            new Vector2(2.6f, 2.849f),//17
            new Vector2(2.7f, 2.849f),//18
            new Vector2(2.7f, 0.2f),//19
            new Vector2(2.6f, 0.2f),//20
            new Vector2(2.2f, 0.2f),//21
            new Vector2(2.2f, 2.3f),//22
            new Vector2(0.212f, 2.3f),//23
            new Vector2(0.212f, 0.199f),//24
            new Vector2(2.2f, 0.2f)//25
        };

        return testdata;
    }

    



}
