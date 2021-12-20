using Netherlands3D.TileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    public class TrafficSimulator : MonoBehaviour
    {
        public List<Vehicle> allVehicles = new List<Vehicle>();
        public Dictionary<Color32, int> carColors;
        public static TrafficSimulator Instance = null;
        private int frames;

        public int vehicleSpeed = 20;

        public bool enableVehicleSimulation = false;
        public bool enableBoundsSimulation = false;

        public int minimumVehicleRenderDistance = 500;
        public int mediumVehicleRenderDistance = 1000;
        public int maximumVehicleRenderDistance = 1500;

        [SerializeField]
        private BinaryMeshLayer terrainLayer;

        [Serializable]
        public struct VehicleType
        {
            public GameObject vehicleType;
            public int vehicleFrequency;
        }

        public List<VehicleType> vehicles;

        public int totalColorWeight;

        public int totalVehicleTypeWeight;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            foreach (VehicleType vehicle in vehicles)
            {
                totalVehicleTypeWeight += vehicle.vehicleFrequency;
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

            foreach (KeyValuePair<Color32,int> vehicleColor in carColors)
            {
                totalColorWeight += vehicleColor.Value;
            }
        }

        /// <summary>
        /// Places a vehicle on a roadobject
        /// </summary>
        /// <param name="currentRoadObject"></param>
        public void PlaceCar(RoadObject currentRoadObject)
        {
            // Chooses a random vehicle out of all the vehicle objects.
            GameObject vehicleTypePrefab = GenerateVehicleType(UnityEngine.Random.Range(0, totalVehicleTypeWeight));

            GameObject tempCarObject = Instantiate(vehicleTypePrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            tempCarObject.name = vehicleTypePrefab.name;
            Vehicle tempCar = tempCarObject.GetComponent<Vehicle>();
            tempCar.currentRoad = currentRoadObject;
            tempCar.SetTerrainLayer(terrainLayer);
            tempCarObject.transform.SetParent(this.transform);
            allVehicles.Add(tempCar);
        }

        /// <summary>
        /// Checks the random number against each rarity of vehicle type
        /// </summary>
        /// <param name="vehiclePercentage"></param>
        /// <returns></returns>
        private GameObject GenerateVehicleType(float vehiclePercentage)
        {
            foreach (VehicleType vehicleType in vehicles)
            {
                if (vehiclePercentage < vehicleType.vehicleFrequency)
                {
                    return vehicleType.vehicleType;
                }
                else
                {
                    vehiclePercentage -= vehicleType.vehicleFrequency;
                }
            }
            return vehicles[0].vehicleType;
        }

        /// <summary>
        /// Simulates all vehicles inside the bounds
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

                foreach (Vehicle vehicle in allVehicles)
                {
                    if (vehicle.debugCar)
                    {
                        Debug.Log(bound.Contains(vehicle.transform.position));
                    }
                    if (!bound.Contains(vehicle.gameObject.transform.position))
                    {
                        if (vehicle.debugCar)
                        {
                            Debug.Log(vehicle.transform.position);
                        }
                        vehicle.gameObject.SetActive(false);
                    }
                }
            }
        }
        /// <summary>
        /// Enables/Disables all the vvehicles from driving
        /// </summary>
        /// <param name="value"></param>
        public void StartSimulation(bool value)
        {
            foreach (Vehicle vehicle in allVehicles)
            {
                Destroy(vehicle.gameObject);
            }
            allVehicles.Clear();
            enableVehicleSimulation = value;
            gameObject.SetActive(value);
        }
        /// <summary>
        /// Updates the speed of all vehicles
        /// </summary>
        /// <param name="speed"></param>
        public void UpdateVehicleSpeed(int speed)
        {
            vehicleSpeed = speed;
        }
    }
}