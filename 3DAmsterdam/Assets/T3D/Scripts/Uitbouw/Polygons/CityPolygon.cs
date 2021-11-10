using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public abstract class CityPolygon : MonoBehaviour
    {
        public abstract int[] LocalBoundaries { get; }
        public abstract Vector3[] Vertices { get; } // used by the CityJSONFormatter to add to the total vertices object
        public int VertOffset { get; set; } //offset to convert LocalBoundaries to AbsoluteBoundaries in the CityJSON object. Must be set while generating JSON since vertices are dynamically collapsed to a single array at that time

        public JSONNode GetJSONPolygon()
        {
            //these absoluteboundaries are only valid when the JSON object is being created
            int[] absoluteBoundaries = CityJSONFormatter.AbsoluteBoundaries[this];
            //int[] absoluteBoundaries = CityJSONFormatter.ConvertBoundaryIndices(LocalBoundaries, VertOffset);

            var boundaryArray = new JSONArray(); // defines a polygon (1st is surface, 2+ is holes in first surface)
            for (int i = 0; i < absoluteBoundaries.Length; i++)
            {
                boundaryArray.Add(absoluteBoundaries[i]);
            }
            return boundaryArray;
        }
        //public JSONNode GetJsonNode()
        //{
        //    var node = new JSONObject();

        //    node["lod"] = Lod;
        //    node["boundaries"] = Boundaries.ToString();

        //    return node;
        //}

        //void WriteMetadata()
        //{
        //    var cityJson = new JSONObject();

        //    var type = new JSONString("CityJSON");

        //    var version = new JSONString("1.0");
        //    var metaData = new JSONObject();

        //    //var geoGraphicalExtent = new SimpleJSON.JSONArray();
        //    var jsonNodeDirect = new SimpleJSON.JSONObject();
        //    var geographicalExtent =
        //    new float[] {
        //        300578.235f,
        //        5041258.061f,
        //        13.688f,
        //        300618.138f,
        //        5041289.394f,
        //        29.45f
        //    };

        //    var test = new JSONArray();
        //    test.Add(new JSONNumber(300f));
        //    test[1] = 400f;

        //    metaData["geographicalExtent"] = (test);

        //    cityJson["type"] = type;
        //    cityJson["version"] = version;
        //    cityJson["metadata"] = metaData;

        //    print(cityJson.ToString());
        //}

        //private void Update()
        //{
        //    //if (Input.GetKeyDown(KeyCode.K))
        //    //{
        //    //    //WriteMetadata();
        //    //    CityJSONFormatter.AddCityObejct(gameObject.name);
        //    //    print(CityJSONFormatter.GetJSON());
        //    //}
        //}
    }
}
