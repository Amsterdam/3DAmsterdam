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

    public Camera cam;

    private void Update()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, 0, Screen.height / 2));

        Vector3 zoomPoint = cam.transform.position;

        maxZoomPosition = new Vector3(cam.transform.position.x, maxZoomOut, cam.transform.position.z);
        minZoomPosition = new Vector3(cam.transform.position.x, maxZoomIn, cam.transform.position.z);

        // raycast wordt afgevuurd vanaf het midden van het scherm totdat er iets wordt geraakt.
        if (Physics.Raycast(ray, out hit))
        {
            zoomPoint = hit.point;
        }

        // de afstand tussen de camera en het zoompunt wordt berekend.
        float zoomDistance = Vector3.Distance(zoomPoint, cam.transform.position);

        // de richting waar in gezoomd moet worden wordt berekend.
        Vector3 zoomDirection = Vector3.Normalize(zoomPoint - cam.transform.position);

        zoom = zoomDirection * zoomDistance * zoomSpeed;

        // er kan niet verder worden ingezoomd dan de maximale inzoomwaarde.
        if (cam.transform.position.y < maxZoomIn)
        {
            cam.transform.position = minZoomPosition;
        }

        // er kan niet verder wordt uitgezoomd dan de maximale uitzoomwaarde.
        if (cam.transform.position.y > maxZoomOut)
        {
            cam.transform.position = maxZoomPosition;
        }
    }

    public void ZoomIn()
    { 
        // er kan alleen gezoomd worden als het niet op de maximale waarde zit.
        if (Camera.main.transform.position.y != maxZoomIn) cam.transform.position += zoom;
    }

    public void ZoomOut()
    {
        // er kan alleen gezoomd worden als het niet op de maximale waarde zit.
        if (Camera.main.transform.position.y != maxZoomOut) cam.transform.position -= zoom;
    }
}
