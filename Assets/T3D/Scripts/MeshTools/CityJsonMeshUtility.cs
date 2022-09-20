using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
using UnityEngine;

public class CityJsonMeshUtility
{
    struct PolygonPoint
    {
        public int Index;
        public Vector3 Point;
    }

    public Mesh[] CreateBoundaryMeshes(CityJsonModel cityModel, JSONNode geometry, bool flipYZ)
    {
        List<Vector3> verts = GetVerts(cityModel, flipYZ);
        var meshes = BoundariesToMeshes(verts, geometry);
        return meshes.ToArray();
    }

    public Dictionary<CityObjectIdentifier, Mesh> CreateMeshes(string cityObjectKey, Matrix4x4 localToWorldMatrix, CityJsonModel cityModel, JSONNode cityObject, bool flipYZ) //dictionary of <LOD, Mesh>city
    {
        var meshes = new Dictionary<CityObjectIdentifier, Mesh>();

        var lods = cityObject["geometry"].Count;
        for (int i = 0; i < lods; i++)
        {
            var boundaryMeshes = CreateBoundaryMeshes(cityModel, cityObject["geometry"][i], flipYZ);
            //combine the boundary meshes
            CombineInstance[] combineInstanceArray = new CombineInstance[boundaryMeshes.Length];
            for (int j = 0; j < boundaryMeshes.Length; j++)
            {
                combineInstanceArray[j].mesh = boundaryMeshes[j];
                combineInstanceArray[j].transform = localToWorldMatrix;
            }
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(combineInstanceArray);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            var lod = cityObject["geometry"][i]["lod"].AsInt;
            var identifier = new CityObjectIdentifier(cityObjectKey, cityObject["geometry"][i], lod, flipYZ);
            meshes.Add(identifier, mesh);
        }
        //mesh.RecalculateBounds();
        //mesh.vertices = TransformVertices(mesh, -mesh.bounds.center, Quaternion.identity, Vector3.one); 
        //offset the vertices so that the center of the mesh bounding box of the selected mesh LOD is at (0,0,0)


        return meshes;
    }

    public static Vector3[] TransformVertices(Mesh mesh, Vector3 translaton, Quaternion rotation, Vector3 scale)
    {
        Vector3[] vertices = mesh.vertices;
        Matrix4x4 matrix = Matrix4x4.TRS(translaton, rotation, scale);
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = matrix.MultiplyPoint(vertices[i]);
        }
        return vertices;
    }

    private List<Vector3> GetVerts(CityJsonModel cityModel, bool flipYZ)
    {
        if (flipYZ)
        {
            return cityModel.vertices.Select(o => new Vector3((float)o.x, (float)o.z, (float)o.y)).ToList();
        }
        else
        {
            return cityModel.vertices.Select(o => new Vector3((float)o.x, (float)o.y, (float)o.z)).ToList();
        }
    }

    private List<Mesh> BoundariesToMeshes(List<Vector3> verts, JSONNode geometry)
    {
        //var geometries = cityObject["geometry"].AsArray;
        //int highestLodIndex = 0;

        //int highestLod = 0;

        //for (int i = 0; i < geometries.Count; i++)
        //{
        //    var lod = geometries[i]["lod"].AsInt;
        //    if (lod > highestLod)
        //    {
        //        highestLod = lod;
        //        highestLodIndex = i;
        //    }
        //}

        var meshes = new List<Mesh>();

        //ignore LOD0 geometry
        //if (geometry["lod"] == 0)
        //{
        //    return meshes;
        //}
        string geometrytype = geometry["type"].Value;

        JSONNode boundariesNode = geometry["boundaries"];
        if (geometrytype == "Solid")
        {
            boundariesNode = geometry["boundaries"][0]; //todo: add support for multiple shells
        }

        // End if no BoundariesNode
        if (boundariesNode is null)
        {
            return meshes;
        }

        foreach (JSONNode boundary in boundariesNode)
        {
            JSONNode outerRing = boundary[0];

            List<List<Vector3>> holes = new List<List<Vector3>>();

            //if (boundary.Count > 1)
            //{
            for (int i = 1; i < boundary.Count; i++)
            {
                var innerRing = boundary[i];
                holes.Add(GetBounderyVertices(verts, innerRing));
            }
            //}

            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

            poly.outside = GetBounderyVertices(verts, outerRing);
            poly.holes = holes;

            if (outerRing.Count == 3)
            {
                meshes.Add(Poly2Mesh.CreateTriangle(poly));
            }
            else
            {
                meshes.Add(Poly2Mesh.CreateMesh(poly));
            }
        }

        return meshes;
    }

    List<Vector3> GetBounderyVertices(List<Vector3> verts, JSONNode boundery)
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (var vertIndex in boundery)
        {
            var index = vertIndex.Value.AsInt;
            vertices.Add(verts[index]);
        }
        vertices.Reverse();
        return vertices;
    }
}
