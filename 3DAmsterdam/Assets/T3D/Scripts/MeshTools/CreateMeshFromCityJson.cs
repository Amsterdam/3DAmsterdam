using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
using UnityEngine;

public class CreateMeshFromCityJson2
{


    public Mesh CreateMesh(CityJsonModel cityModel)
    {
        List<Vector3> verts;
        Dictionary<string, List<int>> triangleLists;

        verts = GetVerts(cityModel);
        List<Vector3> newVerts = new List<Vector3>();
        triangleLists = GetTriangleLists(cityModel);

        Mesh mesh = new Mesh();
        List<int> triangles = new List<int>();
        
        foreach (var item in triangleLists)
        {               
            foreach (int vertexIndex in item.Value)
            {
                newVerts.Add(verts[vertexIndex]);
                triangles.Add(newVerts.Count - 1);            
            }            
        }

        //TODO make this optional?
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = newVerts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        
        return mesh;
    }

    private List<Vector3> GetVerts(CityJsonModel cityModel)
    {
        return cityModel.vertices.Select(o=> new Vector3((float)o.x, (float)o.y, (float)o.z )).ToList();        
    }

    private Dictionary<string, List<int>> GetTriangleLists(CityJsonModel cityModel)
    {
        Dictionary<string, List<int>> triangleList = new Dictionary<string, List<int>>();
        foreach (KeyValuePair<string, JSONNode> cityObject in cityModel.cityjsonNode["CityObjects"])
        {
            var key = cityObject.Key;
            triangleList.Add(key, ReadTriangles(cityObject.Value));            
        }

        return triangleList;
    }

    private List<int> ReadTriangles(JSONNode cityObject)
    {
        List<int> triangles = new List<int>();
        string geometrytype = cityObject["geometry"][0]["type"].Value;

        JSONNode boundariesNode = cityObject["geometry"][0]["boundaries"];
        if (geometrytype == "Solid")
        {
            boundariesNode = cityObject["geometry"][0]["boundaries"][0];
        }

        // End if no BoundariesNode
        if (boundariesNode is null)
        {
            return triangles;
        }

        foreach (JSONNode boundary in boundariesNode)
        {
            JSONNode outerRing = boundary[0];
            
            if (outerRing.Count == 3)
            {
                triangles.Add(outerRing[2].AsInt);
                triangles.Add(outerRing[1].AsInt);
                triangles.Add(outerRing[0].AsInt);
            }
            else if (outerRing.Count == 4) //it's a aquare, make two triangles
            {
                triangles.Add(outerRing[3].AsInt);
                triangles.Add(outerRing[1].AsInt);
                triangles.Add(outerRing[0].AsInt);

                triangles.Add(outerRing[3].AsInt);
                triangles.Add(outerRing[2].AsInt);
                triangles.Add(outerRing[1].AsInt);
            }
            else if(outerRing.Count > 4)
            {
                Debug.LogError("polygon detected, however this is not implemented yet..");
            }

            //TODO support polygons using triangulating
        }

        return triangles;
    }

    private string GetObjectID(string attributeName, JSONNode cityobject)
    {
        string objectid = "";
        objectid = cityobject["attributes"][attributeName].Value.Replace("NL.IMBAG.Pand.", "");

        return objectid;
    }
}
