using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using Netherlands3D.T3D.Uitbouw;

public class MeshToCityJSONConverter : CityObject
{
    //public static JSONObject
    //private MeshFilter meshFilter;
    private Vector3[] verts;
    private int[] tris;

    public override CitySurface[] GetSurfaces()
    {
        var meshFilter = GetComponent<MeshFilter>();

        var mesh = meshFilter.mesh;
        verts = mesh.vertices;
        tris = mesh.triangles;

        List<CitySurface> citySurfaces = new List<CitySurface>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            var tri = new int[] { tris[i], tris[i + 1], tris[i + 2] };
            var triVerts = new Vector3[] { transform.position + verts[tri[0]], transform.position + verts[tri[1]], transform.position + verts[tri[2]] };
            var polygon = new CityPolygon(triVerts, tri);
            citySurfaces.Add(new CitySurface(polygon));
        }

        return citySurfaces.ToArray();
    }

    //public override JSONObject GetGeometryNode()
    //{
    //    var node = new JSONObject();
    //    node["type"] = "MultiSurface"; //todo support other types?
    //    node["lod"] = Lod;
    //    var boundaries = new JSONArray();

    //    var mesh = meshFilter.mesh;
    //    var tris = mesh.triangles;
    //    var verts = mesh.vertices;

    //    for (int i = 0; i < tris.Length; i += 3)
    //    {
    //        var surfaceArray = TriangleToJSON();
    //        //var surfaceArray = Surfaces[i].GetJSONPolygons();
    //        boundaries.Add(surfaceArray);
    //    }
    //    node["boundaries"] = boundaries;

    //    return node;
    //}

    //public JSONArray TriangleToJSON(int a, int b, int c)
    //{
    //    var surfaceArray = new JSONArray(); //defines the entire surface with holes

    //    // the following line and loop could be replaced by 1 loop through all the polygons of the surface, but separating them makes it clearer how the structure of the array works

    //    // add surface
    //    //surfaceArray.Add(Surface.SolidSurfacePolygon.GetJSONPolygon());
    //    return surfaceArray;
    //}
}
