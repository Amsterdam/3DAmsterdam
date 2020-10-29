using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSimulator : MonoBehaviour
{
    public List<Car> allCars = new List<Car>();
    public GameObject carPrefab;
    public static TrafficSimulator Instance = null;
    private int frames;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.O))
        {
            Bounds boundsTest = new Bounds(new Vector3(1471.478f, 14.64141f, -3588.487f), new Vector3(757f, 132f, -3170f));
            SimulateInBounds(boundsTest);
        }
    }

    public void PlaceCar(RoadObject currentRoadObject)
    {
        GameObject tempCar = Instantiate(carPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        tempCar.GetComponent<Car>().currentRoad = currentRoadObject;
        allCars.Add(tempCar.GetComponent<Car>());
    }

    public void SimulateInBounds(Bounds bound)
    {
        frames++;
        if (frames % 10 == 0)
        {
            foreach (Car car in allCars)
            {
                if (!bound.Contains(car.transform.position))
                {
                    car.gameObject.SetActive(false);
                }

            }
        }
    }

    public void StartSimulation()
    {
        foreach (Car car in allCars)
        {
            car.startCar = !car.startCar;
        }
    }
}
