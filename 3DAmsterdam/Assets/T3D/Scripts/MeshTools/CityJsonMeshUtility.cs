using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using Netherlands3D.Utilities;
using SimpleJSON;
using System;
using System.Collections;
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

    public Mesh[] CreateMeshes(CityJsonModel cityModel, JSONNode cityObject)
    {        
        List<Vector3> verts = GetVerts(cityModel);
        return GetMeshes(verts, cityObject).ToArray();
        
    }

    public Mesh CreateMesh(Transform transform, CityJsonModel cityModel, JSONNode cityObject)
    {
        var meshes = CreateMeshes(cityModel, cityObject);

        //combine the meshes
        CombineInstance[] combineInstanceArray = new CombineInstance[meshes.Length];        
        for(int i=0; i < meshes.Length; i++)
        {
            combineInstanceArray[i].mesh = meshes[i];
            combineInstanceArray[i].transform = transform.localToWorldMatrix;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combineInstanceArray);
        mesh.RecalculateNormals();
        return mesh;
    }

    private List<Vector3> GetVerts(CityJsonModel cityModel)
    {
        return cityModel.vertices.Select(o=> new Vector3((float)o.x, (float)o.y, (float)o.z )).ToList();        
    }

    private List<Mesh> GetMeshes(List<Vector3> verts, JSONNode cityObject)
    {
        var geometries = cityObject["geometry"].AsArray;
        int highestLodIndex = 0;
        for(int i = 0; i<geometries.Count; i++)
        {
            var lod = geometries[i]["lod"].AsInt;
            if (lod > highestLodIndex) highestLodIndex = i;                            
        }

        List<Mesh> meshes = new List<Mesh>();
        string geometrytype = cityObject["geometry"][highestLodIndex]["type"].Value;

        JSONNode boundariesNode = cityObject["geometry"][highestLodIndex]["boundaries"];
        if (geometrytype == "Solid")
        {
            boundariesNode = cityObject["geometry"][highestLodIndex]["boundaries"][0];
        }

        // End if no BoundariesNode
        if (boundariesNode is null)
        {
            return null;
        }

        foreach (JSONNode boundary in boundariesNode)
        {
            JSONNode outerRing = boundary[0];

            List<List<Vector3>> holes = new List<List<Vector3>>();

            if(boundary.Count > 1)
            {
                for(int i = 1; i<boundary.Count; i++)
                {
                    var innerRing = boundary[i];                    
                    holes.Add(GetBounderyVertices(verts, innerRing));
                }
            }

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
