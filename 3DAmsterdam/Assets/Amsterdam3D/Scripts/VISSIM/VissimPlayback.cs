using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VissimPlayback : MonoBehaviour
{
    public Dictionary<int, VissimCar> vehicles = new Dictionary<int, VissimCar>();
    [SerializeField] private ConvertZFP fileConverter = default;

    public float timeCounter;
    public int loopCounter = 0;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] vissimCarPrefab = default; // REMOVE THIS LATER AFTER YOU GOT MORE MODELS
    // Start is called before the first frame update
    void Start()
    {
        timeCounter = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(fileConverter.finishedLoadingData) 
        {
            SendCommand(fileConverter.allVissimData);
        }
    }

    public void SendCommand(List<VissimData> dataList)
    {
        if (Time.time > timeCounter)
        {
            timeCounter = Time.time + fileConverter.timeBetweenFrames; // runs the simulation at the imported simspeed
            fileConverter.frameCounter++;
           
            for (int i = loopCounter; i < dataList.Count; i++)
            {
                if(fileConverter.frameCounter != dataList[i].simsec)
                {
                    loopCounter = i;
                    break;
                }
                else
                {
                    if (vehicles.ContainsKey(dataList[i].id))
                    {
                        // send vehicle command
                        vehicles[dataList[i].id].ExecuteVISSIM(dataList[i]);
                    }
                    else
                    {
                        // change 0 with i when you have more models
                        GameObject tempObject = Instantiate(fileConverter.vehicleTypes[dataList[i].vehicleType][Random.Range(0, fileConverter.vehicleTypes[dataList[i].vehicleType].Length )], transform.position, transform.rotation);
                        tempObject.transform.SetParent(this.transform);
                        VissimCar carInstance = tempObject.GetComponent<VissimCar>();
                        vehicles.Add(dataList[i].id, carInstance);
                        carInstance.ExecuteVISSIM(dataList[i]);
                    }
                }
            }
        }
    }
}
