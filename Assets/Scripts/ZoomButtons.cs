using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomButtons : MonoBehaviour
{
    private float zoomSpeed = 0.5f;
    private const float maxZoomOut = 100f;
    private const float maxZoomIn = 1f;

    private Vector3 zoom;
    private Vector3 maxZoomPosition;
    private Vector3 minZoomPosition;

    private void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, 0, Screen.height / 2));

        Vector3 zoomPoint = Camera.main.transform.position;

        maxZoomPosition = new Vector3(Camera.main.transform.position.x, maxZoomOut, Camera.main.transform.position.z);
        minZoomPosition = new Vector3(Camera.main.transform.position.x, maxZoomIn, Camera.main.transform.position.z);

        // raycast wordt afgevuurd vanaf het midden van het scherm totdat er iets wordt geraakt.
        if (Physics.Raycast(ray, out hit))
        {
            zoomPoint = hit.point;
        }

        // de afstand tussen de camera en het zoompunt wordt berekend.
        float zoomDistance = Vector3.Distance(zoomPoint, Camera.main.transform.position);

        // de richting waar in gezoomd moet worden wordt berekend.
        Vector3 zoomDirection = Vector3.Normalize(zoomPoint - Camera.main.transform.position);

        zoom = zoomDirection * zoomDistance * zoomSpeed;

        // er kan niet verder worden ingezoomd dan de maximale inzoomwaarde.
        if (Camera.main.transform.position.y < maxZoomIn)
        {
            Camera.main.transform.position = minZoomPosition;
        }

        // er kan niet verder wordt uitgezoomd dan de maximale uitzoomwaarde.
        if (Camera.main.transform.position.y > maxZoomOut)
        {
            Camera.main.transform.position = maxZoomPosition;
        }
    }

    public void ZoomIn()
    { 
        // er kan alleen gezoomd worden als het niet op de maximale waarde zit.
        if (Camera.main.transform.position.y != maxZoomIn) Camera.main.transform.position += zoom;
    }

    public void ZoomOut()
    {
        // er kan alleen gezoomd worden als het niet op de maximale waarde zit.
        if (Camera.main.transform.position.y != maxZoomOut) Camera.main.transform.position -= zoom;
    }
}
