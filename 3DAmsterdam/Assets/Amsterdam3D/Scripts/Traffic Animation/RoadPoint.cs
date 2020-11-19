using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPoint : MonoBehaviour
{
    public Vector3 pointCoordinates;
    public void Initiate(double longitude, double latitude)
    {
        Vector3 tempCoordinates = ConvertCoordinates.CoordConvert.WGS84toUnity(longitude, latitude);
        tempCoordinates.y = 45f; // random height to make sure its somewhat close to the map.
        pointCoordinates = tempCoordinates;
        transform.position = pointCoordinates;
    }
    
    /// <summary>
    /// Draws this roads Gizmo's
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
