using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Netherlands3D.Cameras;
using Netherlands3D.TileSystem;
using System.CodeDom.Compiler;
using System;

namespace Netherlands3D.Traffic
{
    public class Vehicle : MonoBehaviour
    {
        public RoadObject currentRoad;
        private RoadObject lastRoad = null;
        public RoadObject nextRoad = null;
        public int currentRoadIndex = 0;
        public int speed = 20;

        public bool debugCar = false;
        private int findRoadLoopFrames = 0;

        private int updateCarFrames = 0;
        private float vehicleFrameSpeedCompensator = 1f;

        private float carResetTimeStamp = 0.0f;
        private float maxWaitingTime = 5f;

        private float lastRecordedHeight = 0.0f;
        private RaycastHit hit;

        private BinaryMeshLayer terrainLayer;

        public bool needToStop = false;

        private VehicleProperties vehicleProperties;

        void Start()
        {
            maxWaitingTime = UnityEngine.Random.Range(3f, 7f);
            transform.position = currentRoad.roadPoints[0].pointCoordinates;
            currentRoadIndex++;

            vehicleProperties = gameObject.GetComponent<VehicleProperties>();
            if (gameObject.name == "BasicCar")
            {
                ApplyCarSettings(gameObject);
            }
            speed = TrafficSimulator.Instance.vehicleSpeed;
        }

        /// <summary>
        /// Applies settings specific to the car vehicle
        /// </summary>
        /// <param name="car"></param>
        private void ApplyCarSettings(GameObject car)
        {
            int colorPercentage = UnityEngine.Random.Range(0, TrafficSimulator.Instance.totalColorWeight);
            car.GetComponent<Renderer>().material.color = GenerateColor(colorPercentage);
        }

        /// <summary>
        /// Checks the random number against each rarity of car colors
        /// </summary>
        /// <param name="colorPercentage"></param>
        /// <returns></returns>
        private Color32 GenerateColor(float colorPercentage)
        {
            Color32 defaultColor = Color.gray;
            foreach (KeyValuePair<Color32,int> carColor in TrafficSimulator.Instance.carColors)
            {
                if(colorPercentage < carColor.Value)
                {
                    return carColor.Key;
                }
                else
                {
                    colorPercentage -= carColor.Value;
                }
            }
            return defaultColor;
        }

        // Update is called once per frame
        void Update()
        {
            if (TrafficSimulator.Instance.enableVehicleSimulation && speed != TrafficSimulator.Instance.vehicleSpeed)
            {
                speed = TrafficSimulator.Instance.vehicleSpeed;
            }
            updateCarFrames++;
            float distanceToCar = Vector3.Distance(CameraModeChanger.Instance.ActiveCamera.transform.position, transform.position);
            if (distanceToCar < TrafficSimulator.Instance.minimumVehicleRenderDistance)
                vehicleFrameSpeedCompensator = 1f;
            else if (distanceToCar < TrafficSimulator.Instance.mediumVehicleRenderDistance && distanceToCar > TrafficSimulator.Instance.minimumVehicleRenderDistance && updateCarFrames % 5 == 0)
                vehicleFrameSpeedCompensator = 5f;
            else if (distanceToCar > TrafficSimulator.Instance.mediumVehicleRenderDistance && updateCarFrames % 10 == 0)
                vehicleFrameSpeedCompensator = 10f;

            if (distanceToCar < TrafficSimulator.Instance.minimumVehicleRenderDistance ||
                 distanceToCar < TrafficSimulator.Instance.mediumVehicleRenderDistance && distanceToCar > TrafficSimulator.Instance.minimumVehicleRenderDistance && updateCarFrames % 5 == 0 ||
                distanceToCar > TrafficSimulator.Instance.mediumVehicleRenderDistance && updateCarFrames % 10 == 0)
            {
                if (!gameObject.activeSelf && TrafficSimulator.Instance.enableBoundsSimulation == false)
                {
                    gameObject.SetActive(true);
                }
                if (TrafficSimulator.Instance.enableVehicleSimulation)
                {
                    if (currentRoad != null)
                    {
                        if (currentRoadIndex == 0)
                        {
                            transform.position = currentRoad.roadPoints[0].pointCoordinates;
                            currentRoadIndex++;
                        }

                        Vector3 carPos = transform.position;


                        //ask wheels where their position is
                        //calculate center point
                        //rotate car body based on center point
                        carPos.y += 1.5f;
                        if (Physics.Raycast(carPos, transform.forward, out hit, 20f))
                        {
                            // if the map tiles are loaded beneath the car
                            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Traffic"))
                            {
                                if(Math.Abs(Quaternion.Dot(hit.collider.transform.parent.transform.rotation, transform.rotation)) < 0.4f)
                                {
                                    needToStop = false;
                                }
                                else
                                {
                                    needToStop = true;
                                }
                            }
                            else
                            {
                                needToStop = false;
                            }
                        }
                        else
                        {
                            needToStop = false;
                        }

                        carPos.y = 50f;

                        if (Physics.Raycast(carPos, -Vector3.up, out hit, Mathf.Infinity))
                        {
                            // if the map tiles are loaded beneath the car
                            if (!needToStop)
                            {
                                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Traffic"))
                                {
                                    MoveCar(hit.point);
                                }
                                else
                                {
                                    speed -= 10;
                                    carPos.y = lastRecordedHeight;
                                    MoveCar(carPos);
                                }
                            }
                        }
                        else
                        {
                            if (currentRoad.roadPoints.Count >= currentRoadIndex)
                            {
                                // if the car cant find an underground
                                terrainLayer.AddMeshColliders(carPos);
                                carPos.y = lastRecordedHeight;
                                MoveCar(carPos);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Moves the car and looks for new roads if the car needs a new road
        /// </summary>
        /// <param name="compensationVector"></param>
        public void MoveCar(Vector3 compensationVector)
        {
            if (currentRoad.roadPoints.Count > currentRoadIndex)
            {
                // calculates the look height of the car based on the road and the cars point
                Vector3 tempLook = new Vector3(currentRoad.roadPoints[currentRoadIndex].pointCoordinates.x, compensationVector.y, currentRoad.roadPoints[currentRoadIndex].pointCoordinates.z);
                if (Vector3.Distance(transform.position, tempLook) > 1f)
                {
                    lastRecordedHeight = compensationVector.y;
                    // puts the car on the road
                    transform.position = compensationVector;
                    // looks at the point where the car is driving
                    transform.LookAt(tempLook);
                    transform.rotation = vehicleProperties.GetNewRotation(transform.rotation);
                    // propels the car forward
                    gameObject.transform.Translate(transform.forward * Time.deltaTime * speed * vehicleFrameSpeedCompensator, Space.World);
                }
                else
                {
                    // goes to the next point on the road list
                    currentRoadIndex++;
                }

                // car stuck timer, as long as the car is moving it will add time to it
                carResetTimeStamp = Time.time + maxWaitingTime;
            }
            else
            {
                // optimization by executing once every 10 frames
                findRoadLoopFrames++;
                if (findRoadLoopFrames % 10 == 0 && nextRoad == null)
                {
                    transform.position = compensationVector;
                    // resets the point
                    foreach (RoadObject obj in GenerateRoads.Instance.shuffledRoadsList)
                    {
                        // calculates distance between the car and the 1st object of the found road, this should indicate wether the road is close or not
                        float distance = Vector3.Distance(transform.position, obj.roadPoints[0].pointCoordinates);

                        // finds new road segment based on distance
                        if (distance < Mathf.Round(UnityEngine.Random.Range(7, 13)) && currentRoad != obj && obj != lastRoad)
                        {
                            if (Vector3.Distance(transform.position, obj.roadPoints[0].pointCoordinates) < Vector3.Distance(transform.position, obj.roadPoints[obj.roadPoints.Count - 1].pointCoordinates))
                            {
                                // checks if the point is infront of the player or behind so the car doesn't do "illegal" weird turns
                                Vector3 toTarget = (obj.roadPoints[0].pointCoordinates - transform.position).normalized;
                                if (Vector3.Dot(toTarget, transform.forward) > 0)
                                {
                                    // assigns the newly found road
                                    currentRoadIndex = 0;
                                    lastRoad = currentRoad;
                                    currentRoad = obj;
                                    //nextRoad = obj;
                                    break;
                                }
                            }
                        }
                        else if (Time.time > carResetTimeStamp)
                        {
                            // if the car is stuck, it will reset the position to a random road on the map
                            lastRoad = currentRoad;
                            currentRoad = obj;
                            transform.position = obj.roadPoints[0].pointCoordinates;
                            currentRoadIndex = 0;
                            nextRoad = null;
                            break;
                        }
                    }
                }
                if (nextRoad != null)
                {
                    // moves the car to the next roads starting point
                    Vector3 tempLook = new Vector3(currentRoad.roadPoints[0].pointCoordinates.x, compensationVector.y, currentRoad.roadPoints[0].pointCoordinates.z);
                    float distanceToRoadPoint = Vector3.Distance(transform.position, tempLook);
                    if (distanceToRoadPoint > 1f)
                    {
                        transform.position = compensationVector;
                        // looks at the point where the car is driving
                        transform.LookAt(tempLook);

                        transform.rotation = vehicleProperties.GetNewRotation(transform.rotation);
                        // propels the car forward
                        gameObject.transform.Translate(transform.forward * Time.deltaTime * speed * vehicleFrameSpeedCompensator, Space.World);
                    }
                    else
                    {
                        currentRoadIndex = 0;
                        nextRoad = null;
                    }

                }
            }
        }

        public void SetTerrainLayer(BinaryMeshLayer layer)
        {
            terrainLayer = layer;
        }

    }
}