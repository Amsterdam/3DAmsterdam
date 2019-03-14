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
    public float lerpSpeed;

    private void Start()
    {
    }

    private void LateUpdate()
    {
        Debug.Log(endPos);

        if (Input.GetMouseButtonDown(0) && canClick)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // raycast wordt afgevuurd naar de positie van de muis. als er iets wordt gedecteerd wordt dat opgeslagen in een variabel.
            if (Physics.Raycast(ray, out hit))
            {
                mousePos = hit.point;
            }

            endPos = new Vector3(mousePos.x, stopHeight, mousePos.z);

            canClick = false;
            moveToStreet = true;
        }

        if (moveToStreet)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, endPos, lerpSpeed * Time.deltaTime);
        }

        if ((cam.transform.position.y <= (stopHeight + 0.01f)) && instanCam)
        {
            moveToStreet = false;
            instanCam = false;

            cam.enabled = false;
            Destroy(cam.GetComponent<AudioListener>());

            FPSCam.transform.position = endPos;
        }
    }

    public void StreetCam()
    {
        canClick = true;
        instanCam = true;
    }
}
