using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VissimCar : MonoBehaviour
{
    public VissimData vehicleCommandData;
    public RaycastHit hit;
    private Vector3 lastRecordedHeight = default;
    public void ExecuteVISSIM(VissimData commandData)
    {
        Vector3 temp = transform.position;
        temp.y = 50f;

        if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
        {
            lastRecordedHeight = hit.point; // stores the last height position
            vehicleCommandData = commandData;
            
            // puts the car on the road
            commandData.coordRear.y = hit.point.y;
            commandData.coordFront.y = hit.point.y; // misschien hier 2e raycast waarbij de auto naar dit punt kijkt? Alleen bij een weg die naar beneden gaat en hij nog boven staat krijg je dan rare artefacts
            transform.position = commandData.coordRear;
            transform.LookAt(commandData.coordFront);
        }
        else
        {
            vehicleCommandData = commandData;
            
            // puts the car on the road
            // maak hier functie van
            commandData.coordRear.y = lastRecordedHeight.y;
            commandData.coordFront.y = lastRecordedHeight.y;
            transform.position = commandData.coordRear;
            transform.LookAt(commandData.coordFront);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if time since last execute order > 2, delete/disable object
    }
}
