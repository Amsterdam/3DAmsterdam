using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    static CityJSONFormatter()
    {
        RootObject = new JSONObject();
        CityObjects = new JSONObject();

        RootObject["type"] = "CityJSON";
        RootObject["version"] = "1.0";
        RootObject["CityObjects"] = CityObjects;
        RootObject["vertices"] = new JSONArray();
    }

    public static void AddCityObejct(string name)
    {
        CityObjects[name] = new JSONObject();
    }

    public static string GetJSON()
    {
        return RootObject.ToString();
    }
}
