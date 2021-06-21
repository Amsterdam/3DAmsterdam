using Netherlands3D.LayerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    public class TrafficSimulator : MonoBehaviour
    {
        public List<Car> allCars = new List<Car>();
        public Dictionary<Color32, int> carColors;
        public GameObject carPrefab;
        public static TrafficSimulator Instance = null;
        private int frames;

        public int carSpeed = 20;

        public bool enableCarSimulation = false;
        public bool enableBoundsSimulation = false;

        public int minimumCarRenderDistance = 500;
        public int mediumCarRenderDistance = 1000;
        public int maximumCarRenderDistance = 1500;

        [SerializeField]
        private AssetbundleMeshLayer terrainLayer;

        public int totalWeight;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            // Dictionary created based on the color distribution of cars within the Netherlands
            // The number is based on how common the car is out of 100
            carColors = new Dictionary<Color32, int>();
            carColors.Add(new Color32(145, 145, 145, 255),34); //gray
            carColors.Add(new Color32(30, 30, 30, 255),24); //black
            carColors.Add(new Color32(57, 71, 245, 255),14); //blue
            carColors.Add(new Color32(220, 220, 220, 255),13); //white
            carColors.Add(new Color32(200, 10, 10, 255),7); //red
            carColors.Add(new Color32(10, 160, 10, 255),3); //green
            carColors.Add(new Color32(100, 35, 15, 255),2); //brown
            carColors.Add(new Color32(244, 226, 198, 255),1); //beige
            carColors.Add(new Color32(255, 223, 0, 255),1); //yellow
            carColors.Add(new Color32(255, 94, 19, 255),1); //orange

            foreach (KeyValuePair<Color32,int> carColor in carColors)
            {
                totalWeight += carColor.Value;
            }
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
            tempCar.SetTerrainLayer(terrainLayer);
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
            foreach (Car car in allCars)
            {
                Destroy(car.gameObject);
            }
            allCars.Clear();
            enableCarSimulation = value;
            gameObject.SetActive(value);
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