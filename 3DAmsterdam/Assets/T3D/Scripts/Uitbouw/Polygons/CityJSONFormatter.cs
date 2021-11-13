using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using UnityEngine;

public static class CityJSONFormatter
{
    private static JSONObject RootObject;
    private static JSONObject Metadata;
    private static JSONObject cityObjects;
    private static JSONArray Vertices;
    // In CityJSON verts are stored in 1 big array, while boundaries are stored per geometry.
    // In Unity Verts and boundaries are stored per geometry. These helper variables are used to convert one to the other
    private static JSONArray geographicalExtent;
    public static List<CityObject> CityObjects { get; private set; } = new List<CityObject>();

    private static bool swapYZ = true; //swap y and z coordinates of vertices?

    public static string GetJSON()
    {
        RootObject = new JSONObject();
        cityObjects = new JSONObject();
        Vertices = new JSONArray();
        Metadata = new JSONObject();

        RootObject["type"] = "CityJSON";
        RootObject["version"] = "1.0";
        RootObject["metadata"] = Metadata;
        RootObject["CityObjects"] = cityObjects;
        RootObject["vertices"] = Vertices;

        foreach (var obj in CityObjects)
        {
            AddCityObejctToJSONData(obj);
        }

        return RootObject.ToString();
    }

    //register city object to be added to the JSON when requested
    public static void AddCityObejct(CityObject obj)
    {
        CityObjects.Add(obj);
    }

    // Called when a CityObject is created todo: remove when cityObject is destroyed
    private static void AddCityObejctToJSONData(CityObject obj)
    {
        obj.UpdateSurfaces(); // update latest changes

        foreach (var surface in obj.Surfaces)
        {
            AddCityGeometry(obj, surface); //adds the verts to 1 array and sets the mapping of the local boundaries of each CityPolygon to this new big array
        }
        RecalculateGeographicalExtents();
        cityObjects[obj.Name] = obj.GetJsonObject();
    }

    // geometry needs a parent, so it is called when adding a CityObject. todo: remove when cityGeometry is destroyed
    private static void AddCityGeometry(CityObject parent, CitySurface surface)
    {
        //Debug.Log("adding verts for: " + surface.name + " of " + parent.Name);
        for (int i = 0; i < surface.Polygons.Count; i++)
        {
            var polygon = surface.Polygons[i];
            polygon.LocalToAbsoluteBoundaryConverter = new Dictionary<int, int>(); //reset the dictionary if a previous export occurred

            for (int j = 0; j < polygon.Vertices.Length; j++)
            {
                Vector3 vert = polygon.Vertices[j];
                if (swapYZ)
                    Vertices.Add(new Vector3(vert.x, vert.z, vert.y));
                else
                    Vertices.Add(vert);

                polygon.LocalToAbsoluteBoundaryConverter.Add(j, Vertices.Count - 1);
                polygon.BoundaryConverterIsSet = true; // mark the converter as set to later in the export function it can reliably be used
            }
        }
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

        //Debug.Log(geographicalExtent.Count);
        Metadata["geographicalExtent"] = geographicalExtent;
    }
}
