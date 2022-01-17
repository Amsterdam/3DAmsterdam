using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;
using Netherlands3D.Core;

public class TestMeshClipper : MonoBehaviour
{
    public double minX;
    public double minY;
    public double maxX;
    public double maxY;
    public GameObject go;

    // Start is called before the first frame update
    void Start()
    {
        //preparation setup the boundingBox
        MeshClipper.RDBoundingBox bbox = new MeshClipper.RDBoundingBox(minX, minY, maxX, maxY);

        // start the new meshclipper
        MeshClipper clipper = new MeshClipper(); 
        // tell the clipper which gameObject the mesh is attached to.
        // at this point the clipper will translate the unityCoordinates (mesh + gameObject.origin) to RD-coordinates
        clipper.SetGameObject(go);

        // clip the desired submesh
        clipper.ClipSubMesh(bbox, 0);

        // after clipping the clipper contains a list of Vector3RD coordinates. each set of 3 coordinates describes a triangle (orientation is counterClockwise
        CreateMesh(clipper.clippedVerticesRD);

        // it is now possible to clip the next submesh ( if there is one) without having to translate coordinates
    }

    // for debugging, create a mesh from the triangleList
    private void CreateMesh(List<Vector3RD> points)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        for (int i = 0; i < points.Count; i++)
        {
            vertices.Add(CoordConvert.RDtoUnity(points[i]));
            indices.Add(i);
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
