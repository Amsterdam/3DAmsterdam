using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    public class TrafficSimulator : MonoBehaviour
    {
        public List<Car> allCars = new List<Car>();
        public GameObject carPrefab;
        public static TrafficSimulator Instance = null;
        private int frames;

        public int carSpeed = 20;

        public bool enableCarSimulation = false;
        public bool enableBoundsSimulation = false;

        public int minimumCarRenderDistance = 500;
        public int mediumCarRenderDistance = 1000;
        public int maximumCarRenderDistance = 1500;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            StartSimulation(true);
        }

        private void Update()
        {

        }
        /// <summary>
        /// Places a car on a roadobject
        /// </summary>
        /// <param name="currentRoadObject"></param>
        public void PlaceCar(RoadObject currentRoadObject)
        {
            GameObject tempCarObject = Instantiate(carPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            Car tempCar = tempCarObject.GetComponent<Car>();
            tempCar.currentRoad = currentRoadObject;
            tempCarObject.transform.SetParent(this.transform);
            allCars.Add(tempCar);
        }
        /// <summary>
        /// Simulates all cars inside the bounds
        /// </summary>
        /// <param name="bound"></param>
        public void SimulateInBounds(Bounds bound)
        {
            frames++;
            if (frames % 10 == 0)
            {
                Bounds correctedBounds = bound;
                correctedBounds.min = new Vector3(correctedBounds.min.x, -60f, correctedBounds.min.z);
                correctedBounds.max = new Vector3(correctedBounds.max.x, 60f, correctedBounds.max.z);
                Debug.Log(correctedBounds.min + " " + correctedBounds.max);

                foreach (Car car in allCars)
                {
                    if (car.debugCar)
                    {
                        Debug.Log(bound.Contains(car.transform.position));
                    }
                    if (!bound.Contains(car.gameObject.transform.position))
                    {
                        if (car.debugCar)
                        {
                            Debug.Log(car.transform.position);
                        }
                        car.gameObject.SetActive(false);
                    }
                }
            }
        }
        /// <summary>
        /// Enables/Disables all the cars from driving
        /// </summary>
        /// <param name="value"></param>
        public void StartSimulation(bool value)
        {
            enableCarSimulation = value;
        }
        /// <summary>
        /// Updates the car speed of all cars
        /// </summary>
        /// <param name="speed"></param>
        public void UpdateCarSpeed(int speed)
        {
            carSpeed = speed;
        }
    }
}