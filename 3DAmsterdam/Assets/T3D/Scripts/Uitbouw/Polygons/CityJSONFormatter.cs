using System.Collections;
using System.Collections.Generic;
using System.Text;
using Netherlands3D.T3D.Uitbouw;
using SimpleJSON;
using UnityEngine;

public static class CityJSONFormatter
{
    public static JSONObject RootObject { get; private set; }
    public static JSONObject CityObjects { get; private set; }

    //public static string type = "CityJSON";
    //public string version = "1.0";
    //public List<float[]> metadata = new List<float[]>();
    //public float[][] test;
    //public float[] geographicalExtent = new float[] { 0f, 1.2f };
    private static List<Vector3[]> vertList = new List<Vector3[]>();

    static CityJSONFormatter()
    {
        RootObject = new JSONObject();
        CityObjects = new JSONObject();

        RootObject["type"] = "CityJSON";
        RootObject["version"] = "1.0";
        RootObject["CityObjects"] = CityObjects;
        RootObject["vertices"] = new JSONArray();
    }

    public static void AddCityObejct(CityObject obj)
    {
        CityObjects[obj.Name] = obj.GetJsonNode();

        foreach(var geometry in obj.Geometry)
        {
            AddCityGeometry(obj, geometry);
        }
    }

    public static string GetJSON()
    {
        return RootObject.ToString();
    }

    public static void AddCityGeometry(CityObject parent, CityGeometry geometry)
    {
        Debug.Log("adding verts for: " + geometry.name + " of " + parent.Name);

        vertList.Add(geometry.Vertices);
        foreach(var vert in geometry.Vertices)
        {
            RootObject["vertices"].Add(vert);
        }
    }
}
