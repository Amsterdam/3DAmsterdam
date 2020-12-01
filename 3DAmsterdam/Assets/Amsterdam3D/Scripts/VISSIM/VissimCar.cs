using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VissimCar : MonoBehaviour
{
    public VissimData vehicleCommandData;
    public RaycastHit hit;
    private Vector3 lastRecordedHeight = default;
    /// <summary>
    /// Parses the VISSIM data to the vehicle and executes it
    /// </summary>
    /// <param name="commandData"></param>
    public void ExecuteVISSIM(VissimData commandData)
    {
        vehicleCommandData = commandData;

        Vector3 temp = transform.position;
        temp.y = 50f;

        if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
        {
            lastRecordedHeight = hit.point; // stores the last height position
            // puts the car on the road
            MoveObject(hit.point);
        }
        else
        {
            // puts the car on the road
            MoveObject(lastRecordedHeight);
        }
    }

    /// <summary>
    /// Moves the VISSIM Object
    /// </summary>
    public void MoveObject(Vector3 vectorCorrection)
    {
        // Corrects the vehicles Y position to the height of the map (Maaiveld)
        vehicleCommandData.coordRear.y = vectorCorrection.y;
        vehicleCommandData.coordFront.y = vectorCorrection.y; // misschien hier 2e raycast waarbij de auto naar dit punt kijkt? Alleen bij een weg die naar beneden gaat en hij nog boven staat krijg je dan rare artefacts
        
        // Moves the vehicle to the designated position
        transform.position = vehicleCommandData.coordRear;
        transform.LookAt(vehicleCommandData.coordFront);
    }
}
