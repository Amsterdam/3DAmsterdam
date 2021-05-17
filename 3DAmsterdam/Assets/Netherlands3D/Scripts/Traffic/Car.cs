using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Netherlands3D.Cameras;
using Netherlands3D.LayerSystem;
using System.CodeDom.Compiler;
using System;

namespace Netherlands3D.Traffic
{
    public class Car : MonoBehaviour
    {
        public RoadObject currentRoad;
        private RoadObject lastRoad = null;
        public RoadObject nextRoad = null;
        public int currentRoadIndex = 0;
        public int speed = 20;

        public GameObject[] cars;

        public bool debugCar = false;
        private int findRoadLoopFrames = 0;

        private int updateCarFrames = 0;
        private float vehicleFrameSpeedCompensator = 1f;

        private float carResetTimeStamp = 0.0f;
        private float maxWaitingTime = 5f;

        private float lastRecordedHeight = 0.0f;
        private RaycastHit hit;

        private AssetbundleMeshLayer terrainLayer;

        public bool needToStop = false;

        private GameObject carType;

        private VehicleProperties vehicleProperties;

        void Start()
        {
            maxWaitingTime = UnityEngine.Random.Range(3f, 7f);
            transform.position = currentRoad.roadPoints[0].pointCoordinates;
            currentRoadIndex++;
            // disables all car objects
            foreach (GameObject car in cars)
            {
                car.SetActive(false);
            }
            // Chooses a random car out of all the car objects.

            carType = cars[UnityEngine.Random.Range(0, cars.Length)];
            vehicleProperties = carType.GetComponent<VehicleProperties>();
            if (carType.name == "BasicCar")
            {
                ApplySettings(carType);
            }
            carType.SetActive(true);
            speed = TrafficSimulator.Instance.carSpeed;
        }

        private void ApplySettings(GameObject car)
        {
            float colorPercentage = UnityEngine.Random.Range(0.0f, 1.0f);
            car.GetComponent<Renderer>().material.color = GenerateColor(colorPercentage);
        }

        private Color32 GenerateColor(float colorPercentage)
        {
            if(colorPercentage > 0.66f)
            {
                return new Color32(145, 145, 145, 255); //gray 34%
            }
            if(colorPercentage <= 0.66f && colorPercentage > 0.42f)
            {
                return new Color32(30, 30, 30, 255); //black 24%
            }
            if(colorPercentage <= 0.42f && colorPercentage > 0.28f)
            {
                return new Color32(57, 71, 245, 255); //blue 14%
            }
            if(colorPercentage <= 0.28f && colorPercentage > 0.15f)
            {
                return new Color32(220, 220, 220, 255); //white 13%
            }
            if(colorPercentage <= 0.15f && colorPercentage > 0.08f)
            {
                return new Color32(200, 10, 10, 255); //red 7%
            } 
            if(colorPercentage <= 0.08f && colorPercentage > 0.05f)
            {
                return new Color32(10, 160, 10, 255); //green 3%
            }  
            if(colorPercentage <= 0.05f && colorPercentage > 0.03f)
            {
                return new Color32(100, 35, 15, 255); //brown 2%
            }    
            if(colorPercentage <= 0.03f && colorPercentage > 0.02f)
            {
                return new Color32(244, 226,198, 255); //beige 1%
            } 
            if(colorPercentage <= 0.02f && colorPercentage > 0.01f)
            {
                return new Color32(255, 223, 0, 255); //yellow 1%
            }  
            return new Color32(255, 94, 19, 255); //orange 1%
        }

        // Update is called once per frame
        void Update()
        {
            if (TrafficSimulator.Instance.enableCarSimulation && speed != TrafficSimulator.Instance.carSpeed)
            {
                speed = TrafficSimulator.Instance.carSpeed;
            }
            updateCarFrames++;
            float distanceToCar = Vector3.Distance(CameraModeChanger.Instance.ActiveCamera.transform.position, transform.position);
            if (distanceToCar < TrafficSimulator.Instance.minimumCarRenderDistance)
                vehicleFrameSpeedCompensator = 1f;
            else if (distanceToCar < TrafficSimulator.Instance.mediumCarRenderDistance && distanceToCar > TrafficSimulator.Instance.minimumCarRenderDistance && updateCarFrames % 5 == 0)
                vehicleFrameSpeedCompensator = 5f;
            else if (distanceToCar > TrafficSimulator.Instance.mediumCarRenderDistance && updateCarFrames % 10 == 0)
                vehicleFrameSpeedCompensator = 10f;

            if (distanceToCar < TrafficSimulator.Instance.minimumCarRenderDistance ||
                 distanceToCar < TrafficSimulator.Instance.mediumCarRenderDistance && distanceToCar > TrafficSimulator.Instance.minimumCarRenderDistance && updateCarFrames % 5 == 0 ||
                distanceToCar > TrafficSimulator.Instance.mediumCarRenderDistance && updateCarFrames % 10 == 0)
            {
                if (!gameObject.activeSelf && TrafficSimulator.Instance.enableBoundsSimulation == false)
                {
                    gameObject.SetActive(true);
                }
                if (TrafficSimulator.Instance.enableCarSimulation)
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
                            if (hit.collider.gameObject.name == "BasicTruck" || hit.collider.gameObject.name == "BasicCar")
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
                                if (hit.collider.gameObject.name != "BasicTruck" && hit.collider.gameObject.name != "BasicCar")
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
                    transform.LookAt(tempLook); // MAYBE U CAN PUT THIS IN THE ELSE SO ITS ONLY EXECUTED ONCE????
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

        public void SetTerrainLayer(AssetbundleMeshLayer layer)
        {
            terrainLayer = layer;
        }

    }
}