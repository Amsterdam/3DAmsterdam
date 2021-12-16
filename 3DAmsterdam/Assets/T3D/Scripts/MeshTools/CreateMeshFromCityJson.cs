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

public class CreateMeshFromCityJson
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

    private List<int> GetTriangleLists(List<Vector3> verts, JSONNode cityObject)
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
            //else if (outerRing.Count == 4) //it's a aquare, make two triangles
            //{
            //    triangles.Add(outerRing[3].AsInt);
            //    triangles.Add(outerRing[1].AsInt);
            //    triangles.Add(outerRing[0].AsInt);

            //    triangles.Add(outerRing[3].AsInt);
            //    triangles.Add(outerRing[2].AsInt);
            //    triangles.Add(outerRing[1].AsInt);
            //}
            else if (outerRing.Count > 3)
            {
                var len = outerRing.Count;

                Dictionary<Vector3, PolygonPoint> polyPoints = new Dictionary<Vector3, PolygonPoint>();
               
                foreach (var point in outerRing)
                {
                    var polypoint = new PolygonPoint();
                    polypoint.Index = point.Value.AsInt;
                    polypoint.Point = verts[polypoint.Index];
                    polyPoints.Add(polypoint.Point, polypoint);
                }

                Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
                poly.outside = polyPoints.Select(o =>  o.Value.Point).ToList();
                //poly.holes = holes;

                //Poly2Mesh takes care of calculating normals, using a right-handed coordinate system
                //Poly2Mesh.CreateMeshData(poly, out surfaceVertices, out surfaceNormals, out surfaceIndices, out surfaceUvs);
                var mesh = Poly2Mesh.CreateMesh(poly);  
                


                int[] tris = new int[mesh.triangles.Length];

                var meshtris = mesh.triangles;
                var meshVerts = mesh.vertices;

                for (int i = 0; i< meshtris.Length; i++)
                {
                    var meshvertindex = meshtris[i];
                    var meshVert = meshVerts[meshvertindex];
                    triangles.Add(polyPoints[meshVert].Index);
                }

            }


        }


        return triangles;
    }

    private List<Mesh> GetMeshes(List<Vector3> verts, JSONNode cityObject)
    {


        List<Mesh> meshes = new List<Mesh>();
        string geometrytype = cityObject["geometry"][0]["type"].Value;

        JSONNode boundariesNode = cityObject["geometry"][0]["boundaries"];
        if (geometrytype == "Solid")
        {
            boundariesNode = cityObject["geometry"][0]["boundaries"][0];
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

    private string GetObjectID(string attributeName, JSONNode cityobject)
    {
        string objectid = "";
        objectid = cityobject["attributes"][attributeName].Value.Replace("NL.IMBAG.Pand.", "");

        return objectid;
    }
}
