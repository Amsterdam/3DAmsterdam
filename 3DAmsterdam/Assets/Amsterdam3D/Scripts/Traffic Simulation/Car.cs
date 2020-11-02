using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Car : MonoBehaviour
{
    private Transform thisCar = default;
    public RoadObject currentRoad;
    private RoadObject lastRoad = null;
    private RoadObject nextRoad = null;
    private int currentRoadIndex = 0;
    public int speed = 20;
    public int maxCarRenderDistance = 1500;

    public bool startCar = false;

    public GameObject[] cars;

    public bool debugCar = false;
    private int frames = 0;

    private float carResetTimeStamp = 0.0f;
    private float maxWaitingTime = 5f;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = currentRoad.roadPoints[0].pointCoordinates;
        currentRoadIndex++;
        // disables all car objects
        foreach (GameObject car in cars)
        {
            car.SetActive(false);
        }
        // Chooses a random car out of all the car objects.
        cars[Random.Range(0, cars.Length - 1)].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            startCar = !startCar;
        }
        if (Vector3.Distance(GenerateRoads.Instance.mainCameraTransform.position, transform.position) < maxCarRenderDistance)
        {
            if (!gameObject.activeSelf && TrafficSimulator.Instance.enableBoundsSimulation == false)
            {
                gameObject.SetActive(true);
            }
            if (startCar)
            {
                if (currentRoad != null)
                {
                    if (currentRoadIndex == 0)
                    {
                        transform.position = currentRoad.roadPoints[0].pointCoordinates;
                        currentRoadIndex++;
                    }

                    RaycastHit hit;
                    Vector3 temp = transform.position;
                    temp.y = 50f;
                    if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
                    {
                        // if the map tiles are loaded beneath the car
                        MoveCar(hit.point);
                    }
                    else
                    {
                        if (currentRoad.roadPoints.Count > currentRoadIndex)
                        {
                            // if the car cant find an underground
                            MoveCar(temp);
                        }
                    }

                }
            }
        }
        else
        {
            if (gameObject.activeSelf && TrafficSimulator.Instance.enableBoundsSimulation == false)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void MoveCar(Vector3 compensationVector)
    {
        if (currentRoad.roadPoints.Count > currentRoadIndex)
        {
            // calculates the look height of the car based on the road and the cars point
            Vector3 tempLook = new Vector3(currentRoad.roadPoints[currentRoadIndex].pointCoordinates.x, compensationVector.y, currentRoad.roadPoints[currentRoadIndex].pointCoordinates.z);
            if (Vector3.Distance(transform.position, tempLook) > 1f)
            {
                // puts the car on the road
                transform.position = compensationVector;
                // looks at the point where the car is driving
                transform.LookAt(tempLook); // MAYBE U CAN PUT THIS IN THE ELSE SO ITS ONLY EXECUTED ONCE????
                // propels the car forward
                gameObject.transform.Translate(transform.forward * Time.deltaTime * speed, Space.World);
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
            frames++;
            if (frames % 10 == 0 && nextRoad == null)
            {
                transform.position = compensationVector;
                // resets the point DEMO ONLY
                foreach (RoadObject obj in GenerateRoads.Instance.shuffledRoadsList)
                {
                    // calculates distance between the car and the 1st object of the found road, this should indicate wether the road is close or not
                    float distance = Vector3.Distance(transform.position, obj.roadPoints[0].pointCoordinates);

                    // finds new road segment based on distance
                    if (distance < Mathf.Round(Random.Range(7, 13)) && currentRoad != obj && obj != lastRoad)
                    {
                        if(debugCar)
                            Debug.Log(distance);

                        // MAYBE DELETE CUZ ITS THE SAME THING AS ABOVE
                        if (Vector3.Distance(transform.position, obj.roadPoints[0].pointCoordinates) < Vector3.Distance(transform.position, obj.roadPoints[obj.roadPoints.Count - 1].pointCoordinates))
                        {
                            // checks if the point is infront of the player or behind so the car doesn't do "illegal" weird turns
                            Vector3 toTarget = (obj.roadPoints[0].pointCoordinates - transform.position).normalized;
                            if (Vector3.Dot(toTarget, transform.forward) > 0)
                            {
                                // assigns the newly found road
                                lastRoad = currentRoad;
                                currentRoad = obj;
                                nextRoad = obj;
                                break;
                            }
                        }
                    }
                    else if (Time.time > carResetTimeStamp)
                    {
                        // if the car is stuck, it will reset the position to a random road on the map
                        lastRoad = currentRoad;
                        currentRoad = obj;
                        nextRoad = obj;
                        break;
                    }
                }
            }
            if (nextRoad != null)
            {
                // moves the car to the next roads starting point
                Vector3 tempLook = new Vector3(currentRoad.roadPoints[0].pointCoordinates.x, compensationVector.y, currentRoad.roadPoints[0].pointCoordinates.z);
                if (Vector3.Distance(transform.position, tempLook) > 1f)
                {
                    transform.position = compensationVector;
                    // looks at the point where the car is driving
                    transform.LookAt(tempLook);
                    // propels the car forward
                    gameObject.transform.Translate(transform.forward * Time.deltaTime * speed, Space.World);
                }
                else
                {
                    currentRoadIndex = 0;
                    nextRoad = null;
                }

            }
        }
    }
}
