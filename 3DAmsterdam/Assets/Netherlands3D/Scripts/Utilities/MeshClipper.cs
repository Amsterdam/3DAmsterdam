using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;

public class MeshClipper
{
    private GameObject tile;
    private Vector3 tileOrigin;
    private Vector3RD[] rdVertices;
    private Mesh mesh;
    private Vector3RD temporaryOrigin;
    public List<Vector3RD> clippedVerticesRD;
    public void SetGameObject(GameObject tileGameObject)
    {
        tile = tileGameObject;
        mesh = tile.GetComponent<MeshFilter>().sharedMesh;
        tileOrigin = tile.transform.position;
        ReadVertices();
    }
   
    public void ClipSubMesh(RDBoundingBox boundingBox, int subMeshNumber)
    {
        clippedVerticesRD = new List<Vector3RD>();
        if (subMeshNumber>=mesh.subMeshCount)
        {
            return;
        }

        int[] indices = mesh.GetIndices(subMeshNumber);
        temporaryOrigin = new Vector3RD(boundingBox.minX, boundingBox.minY, 0);
        Vector3 point1;
        Vector3 point2;
        Vector3 point3;
        List<Vector3> clippingPolygon = CreateClippingPolygon(boundingBox);
        List<Vector3> clippingVectorList = new List<Vector3>();
        clippingVectorList.Capacity = 3;
        for (int i = 0; i < indices.Length; i+=3)
        {
            point1 = OffsetRDVertex(rdVertices[indices[i]], temporaryOrigin);
            point2 = OffsetRDVertex(rdVertices[indices[i+1]], temporaryOrigin);
            point3 = OffsetRDVertex(rdVertices[indices[i+2]], temporaryOrigin);
            trianglePosition position = GetTrianglePosition(point1, point2, point3, boundingBox);
            if (position == trianglePosition.inside)
            {
                clippedVerticesRD.Add(RestoreRDVertex(point1,temporaryOrigin));
                clippedVerticesRD.Add(RestoreRDVertex(point2, temporaryOrigin));
                clippedVerticesRD.Add(RestoreRDVertex(point3, temporaryOrigin));
            }
            else if (position == trianglePosition.overlap)
            {
                clippingVectorList.Clear();
                clippingVectorList.Add(point1);
                clippingVectorList.Add(point2);
                clippingVectorList.Add(point3);

                List<Vector3>clippedTriangle = Netherlands3D.Utilities.TriangleClipping.SutherlandHodgman.ClipPolygon(clippingVectorList, clippingPolygon);

                int vertexcount = clippedTriangle.Count;
                if (vertexcount<3)
                {
                    continue;
                }
                clippedVerticesRD.Add(RestoreRDVertex(clippedTriangle[0], temporaryOrigin));
                clippedVerticesRD.Add(RestoreRDVertex(clippedTriangle[1], temporaryOrigin));
                clippedVerticesRD.Add(RestoreRDVertex(clippedTriangle[2], temporaryOrigin));
                // add extra vectors. vector makes a triangle with the first and the previous vector.
                for (int j = 3; j < vertexcount; j++)
                {
                    clippedVerticesRD.Add(RestoreRDVertex(clippedTriangle[0], temporaryOrigin));
                    clippedVerticesRD.Add(RestoreRDVertex(clippedTriangle[j-1], temporaryOrigin));
                    clippedVerticesRD.Add(RestoreRDVertex(clippedTriangle[j], temporaryOrigin));
                }
            }
        }
    }

    public static List<Vector3> CreateClippingPolygon(RDBoundingBox boundingBox)
    {
        List<Vector3> output = new List<Vector3>(4);
        output.Add(new Vector3(0, 0, 0));
        output.Add(new Vector3((float)boundingBox.width, 0, 0));
        output.Add(new Vector3((float)boundingBox.width,0,(float)boundingBox.height));
        output.Add(new Vector3(0,0,(float)boundingBox.height));
        return output;
    }

    /// <summary>
    /// Returns if triangle is inside, outside or overlapping with boundingbox
    /// </summary>
    /// <param name="point1">Triangle point position using boundingbox bottomleft as origin</param>
    /// <param name="point2">Triangle point position using boundingbox bottomleft as origin</param>
    /// <param name="point3">Triangle point position using boundingbox bottomleft as origin</param>
    /// <param name="boundingBox">RD boundingbox</param>
    /// <returns></returns>
    public static trianglePosition GetTrianglePosition(Vector3 point1, Vector3 point2, Vector3 point3, RDBoundingBox boundingBox)
    {
        int counter = 0;
        if (PointIsInsideArea(point1, boundingBox)) { counter++; }
        if (PointIsInsideArea(point2, boundingBox)) { counter++; }
        if (PointIsInsideArea(point3, boundingBox)) { counter++; }

        if (counter == 0)
        {
            var bbox1 = new Vector3(0, 0, 0);
            var bbox2 = new Vector3(0, 0, (float)boundingBox.height);
            var bbox3 = new Vector3((float)boundingBox.width, 0, (float)boundingBox.height);
            var bbox4 = new Vector3((float)boundingBox.width, 0, 0);

            if (PointIsInTriangle(bbox1, point1, point2, point3)) return trianglePosition.overlap;
            if (PointIsInTriangle(bbox2, point1, point2, point3)) return trianglePosition.overlap;
            if (PointIsInTriangle(bbox3, point1, point2, point3)) return trianglePosition.overlap;
            if (PointIsInTriangle(bbox4, point1, point2, point3)) return trianglePosition.overlap;

            return trianglePosition.outside;
        }
        else if(counter == 3)
        {
            return trianglePosition.inside;
        }
        else
        {
            return trianglePosition.overlap;
        }
    }

    public static bool PointIsInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        var a = .5f * (-p1.z * p2.x + p0.z * (-p1.x + p2.x) + p0.x * (p1.z - p2.z) + p1.x * p2.z);
        var sign = a < 0 ? -1 : 1;
        var s = (p0.z * p2.x - p0.x * p2.z + (p2.z - p0.z) * p.x + (p0.x - p2.x) * p.z) * sign;
        var t = (p0.x * p1.z - p0.z * p1.x + (p0.z - p1.z) * p.x + (p1.x - p0.x) * p.z) * sign;

        return s > 0 && t > 0 && (s + t) < 2 * a * sign;
    }

    public static bool PointIsInsideArea(Vector3 vector, RDBoundingBox boundingBox)
    {
        if (vector.x < 0 || vector.x > boundingBox.width)
        {
            return false;
        }
        if (vector.z < 0 || vector.z > boundingBox.height)
        {
            return false;
        }
        return true;
    }

    private void ReadVertices()
    {
        Vector3[] verts = mesh.vertices;
        rdVertices = new Vector3RD[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            rdVertices[i] = CoordConvert.UnitytoRD((verts[i]+tileOrigin));
        }
    }

    private Vector3 OffsetRDVertex(Vector3RD vector, Vector3RD temporaryOrigin)
    {
        Vector3 output = new Vector3();
        output.x = (float)(vector.x - temporaryOrigin.x);
        output.y = (float)(vector.z);
        output.z = (float)(vector.y - temporaryOrigin.y);
        return output;
    }

    private Vector3RD RestoreRDVertex(Vector3 vector, Vector3RD temporaryOrigin)
    {
        Vector3RD output = new Vector3RD();
        output.x = vector.x + temporaryOrigin.x;
        output.y = vector.z + temporaryOrigin.y;
        output.z = vector.y + temporaryOrigin.z;
        return output;
    }
    public struct RDBoundingBox
    {
        public double minX;
        public double minY;
        public double maxX;
        public double maxY;
        public double height;
        public double width;

    public RDBoundingBox(double bottomLeftX, double bottomLeftY, double topRightX, double topRightY)

    {
            minX = bottomLeftX;
            minY = bottomLeftY;
            maxX = topRightX;
            maxY = topRightY;
            height = maxY - minY;
            width = maxX - minX;
    }
}
    public enum trianglePosition
    {
        outside,
        overlap,
        inside
    }

   
}
