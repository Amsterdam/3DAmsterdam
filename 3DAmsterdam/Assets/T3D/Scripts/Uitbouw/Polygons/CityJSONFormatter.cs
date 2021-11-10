using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using UnityEngine;

public static class CityJSONFormatter
{
    public static JSONObject RootObject { get; private set; }
    public static JSONObject Metadata { get; private set; }
    public static JSONObject CityObjects { get; private set; }
    public static JSONArray Vertices { get; private set; }

    // In CityJSON verts are stored in 1 big array, while boundaries are stored per geometry.
    // In Unity Verts and boundaries are stored per geometry. These helper variables are used to convert one to the other
    public static Dictionary<CityPolygon, int[]> AbsoluteBoundaries = new Dictionary<CityPolygon, int[]>();
    private static List<int> vertIndexOffsets = new List<int>();
    private static List<Vector3[]> vertList = new List<Vector3[]>();
    private static JSONArray geographicalExtent = new JSONArray();

    static CityJSONFormatter()
    {
        RootObject = new JSONObject();
        CityObjects = new JSONObject();
        Vertices = new JSONArray();
        Metadata = new JSONObject();

        RootObject["type"] = "CityJSON";
        RootObject["version"] = "1.0";
        RootObject["metadata"] = Metadata;
        RootObject["CityObjects"] = CityObjects;
        RootObject["vertices"] = Vertices;

        vertIndexOffsets.Add(0); //first element has no offsets
    }

    private static void RecalculateGeographicalExtents()
    {
        float minx = Mathf.Infinity; 
        float miny = Mathf.Infinity;
        float minz = Mathf.Infinity;
        float maxx = Mathf.NegativeInfinity;
        float maxy = Mathf.NegativeInfinity;
        float maxz = Mathf.NegativeInfinity;

        for (int i = 0; i < Vertices.Count; i++)
        {
            Vector3 vert = Vertices[i];
            if (vert.x < minx)
                minx = vert.x;
            else if (vert.x > maxx)
                maxx = vert.x;

            if (vert.y < miny)
                miny = vert.y;
            else if (vert.y > maxy)
                maxy = vert.y;

            if (vert.z < minz)
                minz = vert.z;
            else if (vert.z > maxz)
                maxz = vert.z;
        }

        //Debug.Log(minx + "\t" + miny + "\t" + minz);
        //Debug.Log(maxx + "\t" + maxy+ "\t" + maxz);

        geographicalExtent = new JSONArray();
        geographicalExtent.Add(minx);
        geographicalExtent.Add(miny);
        geographicalExtent.Add(minz);
        geographicalExtent.Add(maxx);
        geographicalExtent.Add(maxy);
        geographicalExtent.Add(maxz);

        Debug.Log(geographicalExtent.Count);
        Metadata["geographicalExtent"] = geographicalExtent;
    }

    public static string GetJSON()
    {
        return RootObject.ToString();
    }

    // Called when a CityObject is created todo: remove when cityObject is destroyed
    public static void AddCityObejct(CityObject obj)
    {
        foreach (var geometry in obj.Polygons)
        {
            AddCityGeometry(obj, geometry);
        }
        RecalculateGeographicalExtents();
        CityObjects[obj.Name] = obj.GetJsonObject();
    }

    // geometry needs a parent, so it is called when adding a CityObject. todo: remove when cityGeometry is destroyed
    private static void AddCityGeometry(CityObject parent, CityPolygon geometry)
    {
        //Debug.Log("adding verts for: " + geometry.name + " of " + parent.Name);

        var verts = geometry.Vertices;

        vertList.Add(verts);
        vertIndexOffsets.Add(vertIndexOffsets[vertList.Count - 1] + verts.Length);
        AbsoluteBoundaries.Add(geometry, ConvertBoundaryIndices(geometry.LocalBoundaries, vertList.Count - 1));

        foreach (var vert in geometry.Vertices)
        {
            Vertices.Add(vert);
        }
    }

    public static int[] ConvertBoundaryIndices(int[] boundaries, int offsetIndex)
    {
        var offsetBoundaries = new int[boundaries.Length];
        for (int i = 0; i < boundaries.Length; i++)
        {
            offsetBoundaries[i] = boundaries[i] + vertIndexOffsets[offsetIndex];
        }
        return offsetBoundaries;
    }
}
