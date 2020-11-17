using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPoint : MonoBehaviour
{
    public Vector3 pointCoordinates;
    public void Initiate(double longitude, double latitude)
    {
        Vector3 tempCoordinates = ConvertCoordinates.CoordConvert.WGS84toUnity(longitude, latitude);
        tempCoordinates.y = 45f; // raycast naar de grond om te kijken of hij er is
        pointCoordinates = tempCoordinates;
        transform.position = pointCoordinates;
        //StartCoroutine(UpdateHeight()); // Mogenlijk vervangen door te raycasten vanaf de auto??? Bij zware performance issues doe t via de road points
    }

    public IEnumerator UpdateHeight()
    {
        yield return new WaitForSeconds(60f); // verander dit naar wanneer de tiles updaten
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        Vector3 temp = pointCoordinates;
        temp.y = 50f;
        if (Physics.Raycast(temp, -Vector3.up, out hit, Mathf.Infinity))
        {
            Debug.Log(hit.point);
            transform.position = hit.point;
            pointCoordinates = hit.point;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
