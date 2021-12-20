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

    private List<Vector3> CreateClippingPolygon(RDBoundingBox boundingBox)
    {
        List<Vector3> output = new List<Vector3>(4);
        output.Add(new Vector3(0, 0, 0));
        output.Add(new Vector3((float)boundingBox.width, 0, 0));
        output.Add(new Vector3((float)boundingBox.width,0,(float)boundingBox.height));
        output.Add(new Vector3(0,0,(float)boundingBox.height));
        return output;
    }

    private trianglePosition GetTrianglePosition(Vector3 point1, Vector3 point2, Vector3 point3, RDBoundingBox boundingBox)
    {
        int counter = 0;
        if (PointISInsideArea(point1,boundingBox)){counter++;}
        if (PointISInsideArea(point2, boundingBox)) { counter++; }
        if (PointISInsideArea(point3, boundingBox)) { counter++; }

        if (counter ==0)
        {
            return trianglePosition.outside;
        }
        else if(counter ==3)
        {
            return trianglePosition.inside;
        }
        else
        {
            return trianglePosition.overlap;
        }


    }

    private bool PointISInsideArea(Vector3 vector, RDBoundingBox boundingBox)
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
