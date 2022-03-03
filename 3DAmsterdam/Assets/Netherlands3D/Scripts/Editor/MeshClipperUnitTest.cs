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
        Vector2 p1 = new Vector2(0,0);
        Vector2 p2 = new Vector2(0,1);
        Vector2 p3 = new Vector2(1,0);

        Vector2 outsidePoint = new Vector2(-0.5f,0.4f);
        Vector2 insidePoint = new Vector2(0.2f,0.4f);

        var pointOutside = MeshClipper.PointIsInTriangle(outsidePoint, p1, p2, p3);
        var pointInside = MeshClipper.PointIsInTriangle(insidePoint, p1, p2, p3);

        Assert.AreEqual(false, pointOutside);
        Assert.AreEqual(true, pointInside);
    }

    [Test]
    public void TestTriangleOverlapBounds()
    {
        Vector2 p1 = new Vector2(0, 0);
        Vector2 p2 = new Vector2(0, 1);
        Vector2 p3 = new Vector2(1, 0);

        var boundingBox = new MeshClipper.RDBoundingBox(0.2f, 0.2f, 10f, 10f);

        MeshClipper.trianglePosition trianglePosition = MeshClipper.GetTrianglePosition(p1, p2, p3, boundingBox);
        Assert.AreEqual(MeshClipper.trianglePosition.overlap, trianglePosition);
    }
}
