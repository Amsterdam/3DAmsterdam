using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSphere : MonoBehaviour
{
    public void OnMouseOver()
    {
        Debug.Log("OEIOEIOEI");

        if (Input.GetMouseButtonDown(0))
        {
            GameObject sphere = GameObject.Find("Visualisatiemenu");
            sphere.GetComponent<PanoramaAPI>().SpawnPanorama();
        }
    }
}
