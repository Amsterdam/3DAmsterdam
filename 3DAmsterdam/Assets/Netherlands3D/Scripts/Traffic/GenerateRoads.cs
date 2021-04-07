using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Linq;
using UnityEngine.Networking;

namespace Netherlands3D.Traffic
{
    public class GenerateRoads : MonoBehaviour
    {
        [SerializeField] public Roads roads;
        public GameObject roadObject;

        public List<RoadObject> allLoadedRoads = new List<RoadObject>();
        public List<RoadObject> shuffledRoadsList = new List<RoadObject>();
        public static GenerateRoads Instance = null;

        public const string roadsFileName = "traffic/road_line.geojson";

        public Transform mainCameraTransform = default;

        private int shuffleFrameCount;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }


        void Start()
        {
            StartCoroutine(GetRoadsJson(Config.activeConfiguration.webserverRootPath + roadsFileName));
            mainCameraTransform = Camera.main.transform;
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
            // send http request
            var request = UnityWebRequest.Get(apiUrl);
            {
                yield return request.SendWebRequest();

                if (request.isDone && !request.isHttpError)
                {
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
            roads = JsonUtility.FromJson<Roads>(jsonString);

            JSONNode N = JSON.Parse(jsonString);
            List<LongitudeLatitude> positions = new List<LongitudeLatitude>();
            // adds coordinates of the street with the index
            for (int i = 0; i < N["features"].Count; i++)
            {
                AddCoordinates(i, N);
                GenerateRoad(roads.features[i]);
            }
        }

        /// <summary>
        /// Generate the road based on ID
        /// </summary>
        /// <param name="road"></param>
        public void GenerateRoad(RoadItem road)
        {
            if (road.properties.highway != "cycleway" && road.properties.highway != "pedestrian" && road.properties.highway != "service")
            {
                GameObject temp = Instantiate(roadObject, new Vector3(0f, 0f, 0f), Quaternion.identity);
                temp.transform.SetParent(this.transform);
                temp.name = road.properties.name;
                RoadObject tempRoadObject = temp.GetComponent<RoadObject>();
                tempRoadObject.Intiate(road);
                allLoadedRoads.Add(tempRoadObject);
            }
        }

        /// <summary>
        /// Adds all the roads coördinates to the chosen road index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="N"></param>
        public void AddCoordinates(int index, JSONNode N)
        {
            List<LongitudeLatitude> positions = new List<LongitudeLatitude>();
            for (int i = 0; i < N["features"][index]["geometry"]["coordinates"].Count; i++)
            {
                LongitudeLatitude tempCoordinates = new LongitudeLatitude();

                // adds a comma to the coordiantes so it can be parsed to a double
                tempCoordinates.longitude = double.Parse(N["features"][index]["geometry"]["coordinates"][i][0].Value.Insert(1, ","));
                // adds a comma to the coordiantes so it can be parsed to a double
                tempCoordinates.latitude = double.Parse(N["features"][index]["geometry"]["coordinates"][i][1].Value.Insert(2, ","));
                // adds positions to the object
                positions.Add(tempCoordinates);

                /*
                tempCoordinates.longitude = N["features"][0]["geometry"]["coordinates"][i][0].Value;
                tempCoordinates.longitude = tempCoordinates.longitude.Insert(1, ",");
                tempCoordinates.latitude = N["features"][0]["geometry"]["coordinates"][i][1].Value;
                tempCoordinates.latitude = tempCoordinates.latitude.Insert(2, ",");
                */
            }
            roads.features[index].geometry.coordinates = positions;
        }
    }
}