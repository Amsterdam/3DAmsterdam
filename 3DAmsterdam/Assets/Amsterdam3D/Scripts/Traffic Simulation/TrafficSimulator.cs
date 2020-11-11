using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSimulator : MonoBehaviour
{
    public List<Car> allCars = new List<Car>();
    public GameObject carPrefab;
    public static TrafficSimulator Instance = null;
    private int frames;

    public int carSpeed = 20;

    public bool enableBoundsSimulation = false;
    [SerializeField] private Bounds boundsSimulation = new Bounds(new Vector3(0,0,0), new Vector3(0,0,0));
    private Amsterdam3D.SelectionTools.SelectionToolBehaviour selectionTool = default;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        selectionTool = FindObjectOfType<Amsterdam3D.SelectionTools.SelectionToolBehaviour>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            enableBoundsSimulation = !enableBoundsSimulation;
        }
        if (enableBoundsSimulation == true)
        {
            if (Input.GetMouseButtonUp(0))
            {
                boundsSimulation = selectionTool.GetBounds();
            }
            
            if (boundsSimulation.size != new Vector3(0,0,0)) // 0,0,0 is actually the "null" for the vector in this case
            {
                SimulateInBounds(boundsSimulation);
            }
        }
    }
    public void PlaceCar(RoadObject currentRoadObject)
    {
        GameObject tempCarObject = Instantiate(carPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
        Car tempCar = tempCarObject.GetComponent<Car>();
        tempCar.currentRoad = currentRoadObject;
        tempCarObject.transform.SetParent(this.transform);
        allCars.Add(tempCar);
    }

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

    public void StartSimulation(bool value)
    {
        foreach (Car car in allCars)
        {
            car.startCar = value;
        }
    }

    public void UpdateCarSpeed(int speed)
    {
        carSpeed = speed;
    }
}
