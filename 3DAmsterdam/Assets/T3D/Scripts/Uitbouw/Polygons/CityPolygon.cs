using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw
{
    public abstract class CityPolygon : CityObject
    {
        public abstract Vector3[] Boundaries { get; }

        void WriteMetadata()
        {
            var cityJson = new JSONObject();

            var type = new JSONString("CityJSON");
     
            var version = new JSONString("1.0");
            var metaData = new JSONObject();

            //var geoGraphicalExtent = new SimpleJSON.JSONArray();
            var jsonNodeDirect = new SimpleJSON.JSONObject();
            var geographicalExtent =
            new float[] {
                300578.235f,
                5041258.061f,
                13.688f,
                300618.138f,
                5041289.394f,
                29.45f
            };

            var test = new JSONArray();
            test.Add(new JSONNumber(300f));
            test[1] = 400f;

            metaData["geographicalExtent"] = (test);

            cityJson["type"] = type;
            cityJson["version"] = version;
            cityJson["metadata"] = metaData;

            print(cityJson.ToString());
        }

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
