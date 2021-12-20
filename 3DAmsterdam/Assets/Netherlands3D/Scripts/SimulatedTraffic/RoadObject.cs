using Netherlands3D.Core;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    public class RoadObject : MonoBehaviour
    {
        public GameObject roadPointObject;
        public string roadName;
        public List<RoadPoint> roadPoints = new List<RoadPoint>();

        /// <summary>
        /// Creates a roadobject and loads in all the roadpoints
        /// </summary>
        /// <param name="tempRoad"></param>
        public void CreateRoad(JSONNode tempRoad)
        {
            Vector3 tempCoordinates = CoordConvert.WGS84toUnity(tempRoad["geometry"]["lon"], tempRoad["geometry"]["lat"]);
            tempCoordinates.y = 45f; // raycast naar de grond om te kijken of hij er is
            name = tempRoad["tags"]["name"];
            transform.position = tempCoordinates;
            for (int i = 0; i < tempRoad["geometry"].Count; i++)
            {
                GameObject tempGameObject = Instantiate(roadPointObject, transform.position, transform.rotation);
                tempGameObject.transform.SetParent(this.transform);
                RoadPoint tempRoadPoint = tempGameObject.GetComponent<RoadPoint>();
                roadPoints.Add(tempRoadPoint);
                tempRoadPoint.Initiate(tempRoad["geometry"][i]["lon"], tempRoad["geometry"][i]["lat"]);

                //if (i == 0)
                //{
                //    BoxCollider collider = tempGameObject.AddComponent<BoxCollider>();
                //    collider.isTrigger = true;
                //}
            }
            // Checks if there x amount of coördinates in road to deem the road segment big enough to generate a car (it's basically a small performance tweak)
            if (tempRoad["geometry"].Count > 2)
            {
                TrafficSimulator.Instance.PlaceCar(this);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            for (int i = 0; i < roadPoints.Count; i++)
            {
                Gizmos.color = Color.red;
                if (i + 1 < roadPoints.Count)
                {
                    Gizmos.DrawLine(roadPoints[i].pointCoordinates, roadPoints[i + 1].pointCoordinates);
                }
            }
        }
    }
}