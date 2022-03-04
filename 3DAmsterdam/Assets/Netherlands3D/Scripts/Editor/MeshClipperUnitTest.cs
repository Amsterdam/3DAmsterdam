using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.IO;
using UnityEngine.TestTools;
using Netherlands3D.WebGL;

public class MeshClipperUnitTest 
{
    [Test]
    public void TestPointsInTriangle()
    {
        //meshclipper existing methods seem to use x and z for vertex input too
        Vector3 p1 = new Vector3(0, 0, 0);
        Vector3 p2 = new Vector3(0, 0, 1);
        Vector3 p3 = new Vector3(1, 0, 0);

        Vector3 outsidePoint = new Vector3(-0.5f,0 , 0.4f);
        Vector3 insidePoint = new Vector3(0.2f,0, 0.4f);

        var pointOutside = MeshClipper.PointIsInTriangle(outsidePoint, p1, p2, p3);
        var pointInside = MeshClipper.PointIsInTriangle(insidePoint, p1, p2, p3);

        Assert.AreEqual(false, pointOutside);
        Assert.AreEqual(true, pointInside);
    }
  
    [Test]
    public void TestTriangleOverlapBounds()
    {
        Vector3 p1Overlap = new Vector3(-0.5f, 0, 0);
        Vector3 p2Overlap = new Vector3(-0.5f, 0, 1);
        Vector3 p3Overlap = new Vector3(1.5f, 0, 0);

        Vector3 p1Outside = new Vector3(-10, 0, 0);
        Vector3 p2Outside = new Vector3(-10, 0, 1);
        Vector3 p3Outside = new Vector3(-3, 0, 0);

        var boundingBox = new MeshClipper.RDBoundingBox(0, 0, 10f, 10f);

        MeshClipper.trianglePosition trianglePositionOverlap = MeshClipper.GetTrianglePosition(p1Overlap, p2Overlap, p3Overlap, boundingBox);
        MeshClipper.trianglePosition trianglePositionOutside = MeshClipper.GetTrianglePosition(p1Outside, p2Outside, p3Outside, boundingBox);

        Assert.AreEqual(MeshClipper.trianglePosition.overlap, trianglePositionOverlap);
        Assert.AreEqual(MeshClipper.trianglePosition.outside, trianglePositionOutside);
    }

    [Test]
    public void TestPolygonClippping()
    {
        Vector3 p1Overlap = new Vector3(-0.5f, 0, -0.1f);
        Vector3 p2Overlap = new Vector3(-0.5f, 0, 1);
        Vector3 p3Overlap = new Vector3(1.5f, 0, -0.5f);

        Vector3 p1Outside = new Vector3(-10, 0, 0);
        Vector3 p2Outside = new Vector3(-10, 0, 1);
        Vector3 p3Outside = new Vector3(-3, 0, 0);

        List<Vector3> triangleA = new List<Vector3>();
        triangleA.Add(p1Overlap);
        triangleA.Add(p2Overlap);
        triangleA.Add(p3Overlap);

        List<Vector3> triangleB = new List<Vector3>();
        triangleA.Add(p1Outside);
        triangleA.Add(p2Outside);
        triangleA.Add(p3Outside);

        var boundingBox = new MeshClipper.RDBoundingBox(0, 0, 10f, 10f);

        List<Vector3> clippingPolygon = MeshClipper.CreateClippingPolygon(boundingBox);

        List<Vector3> clippedTriangleA = Netherlands3D.Utilities.TriangleClipping.SutherlandHodgman.ClipPolygon(triangleA, clippingPolygon);
        List<Vector3> clippedTriangleB = Netherlands3D.Utilities.TriangleClipping.SutherlandHodgman.ClipPolygon(triangleB, clippingPolygon);

        Assert.AreEqual(3, clippedTriangleA.Count);
		for (int i = 0; i < clippedTriangleA.Count; i++)
		{
            Debug.Log($"X {clippedTriangleA[i].x}");
            Debug.Log($"Y {clippedTriangleA[i].y}");
            Debug.Log($"Z {clippedTriangleA[i].z}");
            Debug.Log($"-------------------------");
		}

        //Assert.AreEqual(0, clippedTriangleB.Count);
    }
}
