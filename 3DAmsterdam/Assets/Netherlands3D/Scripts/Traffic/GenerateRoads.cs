using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Linq;
using UnityEngine.Networking;
using System.Linq.Expressions;
using System.IO;
using UnityEditor;
using ConvertCoordinates;

namespace Netherlands3D.Traffic
{
    public class GenerateRoads : MonoBehaviour
    {
        public GameObject roadObject;

        public List<RoadObject> allLoadedRoads = new List<RoadObject>();
        public List<RoadObject> shuffledRoadsList = new List<RoadObject>();
        public static GenerateRoads Instance = null;

        public const string roadsFileName = "traffic/road_line.geojson";

        private int shuffleFrameCount;
        private string bbox;
        private Vector3WGS bottomLeftWGS;
        private Vector3WGS topRightWGS;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }


        public void ShowTraffic(Bounds bounds)
        {
            bottomLeftWGS = CoordConvert.UnitytoWGS84(bounds.min);
            topRightWGS = CoordConvert.UnitytoWGS84(bounds.max);
            allLoadedRoads.Clear();
            shuffledRoadsList.Clear();
            StartCoroutine(GetRoadsJson(Config.activeConfiguration.webserverRootPath + roadsFileName));
        }

        private void Update()
        {
            shuffleFrameCount++;
            if (shuffleFrameCount % 60 == 0)
            {
                if (allLoadedRoads.Count > 0)
                {
                    shuffledRoadsList = GenerateRoads.Instance.allLoadedRoads.OrderBy(x => Random.value).ToList();
                }
            }
        }

        /// <summary>
        /// retrieves the road json (FROM THE WEBSERVER, NOT OSM)
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>
        public IEnumerator GetRoadsJson(string apiUrl)
        {
            string prefixRequest = "https://overpass-api.de/api/interpreter?data=[out:json];";
            string paramRequest = "way[highway~\"motorway|trunk|primary|secondary|tertiary|motorway_link|trunk_link|primary_link|secondary_link|tertiary_link|unclassified|residential|living_street|track|road\"]";
            bbox = "(" + bottomLeftWGS.lat + "," + bottomLeftWGS.lon + "," + topRightWGS.lat + "," + topRightWGS.lon + ");";
            string suffixRequest = "out geom;";
            string fullRequest = prefixRequest + paramRequest + bbox + suffixRequest;

            // send http request
            var request = UnityWebRequest.Get(fullRequest);
            {
                yield return request.SendWebRequest();

                if (request.isDone && !request.isHttpError)
                {
                    //string path = EditorUtility.SaveFilePanel("save","","def",".json");
                    //File.WriteAllText(path, request.downloadHandler.text);
                    // catches the data
                    StartRoadGeneration(request.downloadHandler.text);
                }
            }
        }

        /// <summary>
        /// Retrieves the Json data to start the road generation
        /// </summary>
        /// <param name="jsonData"></param>
        public void StartRoadGeneration(string jsonData)
        {
            string jsonString = jsonData;
            JSONNode N = JSON.Parse(jsonString);
            // adds coordinates of the street with the index
            for (int i = 0; i < N["elements"].Count; i++)
            {
                //AddCoordinates(i, N);
                GenerateRoad(N["elements"][i]);
            }
        }

        /// <summary>
        /// Generate the road based on ID
        /// </summary>
        /// <param name="road"></param>
        public void GenerateRoad(JSONNode road)
        {
            GameObject temp = Instantiate(roadObject, new Vector3(0f, 0f, 0f), Quaternion.identity);
            temp.transform.SetParent(this.transform);
            RoadObject tempRoadObject = temp.GetComponent<RoadObject>();
            tempRoadObject.CreateRoad(road);
            allLoadedRoads.Add(tempRoadObject);
        }

        /// <summary>
        /// Adds all the roads coördinates to the chosen road index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="N"></param>
        //public void AddCoordinates(int index, JSONNode N)
        //{
        //    List<LongitudeLatitude> positions = new List<LongitudeLatitude>();
        //    for (int i = 0; i < N["elements"][index]["geometry"].Count; i++)
        //    {
        //        LongitudeLatitude tempCoordinates = new LongitudeLatitude();

        //        // adds a comma to the coordiantes so it can be parsed to a double
        //        tempCoordinates.longitude = double.Parse(N["elements"][index]["geometry"][i][0].Value.Insert(1, ","));
        //        // adds a comma to the coordiantes so it can be parsed to a double
        //        tempCoordinates.latitude = double.Parse(N["elements"][index]["geometry"][i][1].Value.Insert(2, ","));
        //        // adds positions to the object
        //        positions.Add(tempCoordinates);

        //        /*
        //        tempCoordinates.longitude = N["features"][0]["geometry"]["coordinates"][i][0].Value;
        //        tempCoordinates.longitude = tempCoordinates.longitude.Insert(1, ",");
        //        tempCoordinates.latitude = N["features"][0]["geometry"]["coordinates"][i][1].Value;
        //        tempCoordinates.latitude = tempCoordinates.latitude.Insert(2, ",");
        //        */
        //    }
        //    N["elements"][index]["geomentry"] = positions;
        //}
    }
}