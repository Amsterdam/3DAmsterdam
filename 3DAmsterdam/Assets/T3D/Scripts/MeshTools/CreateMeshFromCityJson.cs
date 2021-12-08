using ConvertCoordinates;
using Netherlands3D.LayerSystem;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3D.LoadData;
using UnityEngine;

public class CreateMeshFromCityJson
{


    public Mesh CreateMesh(CityJsonModel cityModel)
    {


        List<Vector3> verts;
        Dictionary<string, List<int>> triangleLists;

        verts = GetVerts(cityModel);
        List<Vector3> newVerts = new List<Vector3>();
        triangleLists = GetTriangleLists(cityModel);

        Vector2Int textureSize = ObjectIDMapping.GetTextureSize(triangleLists.Count);
        Debug.Log(textureSize);

        Mesh mesh = new Mesh();

        List<int> triangles = new List<int>();
        List<string> objectIDs = new List<string>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> vectorIDs = new List<int>();
        List<int> triangleCount = new List<int>();
        int objectIDNumber = 0;
        int vertexCounter = 0;
        foreach (var item in triangleLists)
        {
            vertexCounter = 0;
            Vector2 uv = ObjectIDMapping.GetUV(objectIDNumber, textureSize);
            foreach (int vertexIndex in item.Value)
            {
                newVerts.Add(verts[vertexIndex]);
                uvs.Add(uv);
                vectorIDs.Add(objectIDNumber);
                triangles.Add(newVerts.Count - 1);
                vertexCounter++;
            }
            triangleCount.Add(vertexCounter);
            objectIDs.Add(item.Key);
            objectIDNumber++;
        }

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = newVerts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        return mesh;
    }

    private List<Vector3> GetVerts(CityJsonModel cityModel)
    {
        return cityModel.vertices.Select(o=> new Vector3((float)o.x, (float)o.y, (float)o.z )).ToList();        
    }

    private Dictionary<string, List<int>> GetTriangleLists(CityJsonModel cityModel)
    {
        Dictionary<string, List<int>> triangleList = new Dictionary<string, List<int>>();
        // string = objectID
        // List<int> = triangles
        string cityObjectType = "";

        foreach (JSONNode cityObject in cityModel.cityjsonNode["CityObjects"])
        {
            cityObjectType = cityObject["type"];

            // Skip non-Buildings
            string objectID = GetObjectID("identificatie", cityObject);
            if (objectID == "")
            {
                objectID = GetObjectID("name", cityObject);
            }

            if (triangleList.ContainsKey(objectID) == true)
            {
                triangleList[objectID].AddRange(ReadTriangles(cityObject));
            }
            else
            {
                triangleList.Add(objectID, ReadTriangles(cityObject));
            }
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
            triangles.Add(outerRing[2].AsInt);
            triangles.Add(outerRing[1].AsInt);
            triangles.Add(outerRing[0].AsInt);
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
