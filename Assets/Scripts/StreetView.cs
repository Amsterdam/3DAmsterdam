using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StreetView : MonoBehaviour
{
    private bool moveToStreet = false;
    private bool canClick = false;
    private bool instanCam = false;

    private Vector3 endPos;
    private Vector3 mousePos;

    public GameObject FPSCam;
    public Camera cam;

    private float stopHeight = 1f;
    private float lerpSpeed = 3f;

    private void Start()
    {
    }

    private void LateUpdate()
    {
        // als de linkermuis knop ergens op het scherm wordt ingedrukt vlieg je daarheen
        if (Input.GetMouseButtonDown(0) && canClick)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // raycast wordt afgevuurd naar de positie van de muis. als er iets wordt gedecteerd wordt dat opgeslagen in een variabel.
            if (Physics.Raycast(ray, out hit))
            {
                mousePos = hit.point;
            }

            // de stoppositie is waar geklikt is met een bepaalde afstand van de grond (y-axis)
            endPos = new Vector3(mousePos.x, stopHeight, mousePos.z);

            canClick = false;
            moveToStreet = true;
        }

        if (moveToStreet)
        {
            // camera beweegt langzaam naar aangewezen plaats
            cam.transform.position = Vector3.Lerp(cam.transform.position, endPos, lerpSpeed * Time.deltaTime);
        }

        // camera stopt met bewegen als die is aangekomen op locatie
        if ((cam.transform.position.y <= (stopHeight + 0.01f)) && instanCam)
        {
            moveToStreet = false;
            instanCam = false;

            cam.enabled = false;
            Destroy(cam.GetComponent<AudioListener>());

            // nieuwe FPS camera wordt op juiste positie geplaatst 
            FPSCam.transform.position = endPos;
        }
    }

    // de knop wordt ingedrukt dus er kan nu een plek geselecteerd worden om heen te vliegen
    public void StreetCam()
    {
        canClick = true;
        instanCam = true;
    }
}
