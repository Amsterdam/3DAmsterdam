using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodView : MonoBehaviour
{
    private bool moveToAir = false;

    private Vector3 endPos;

    public GameObject FPSCam, godCam;
    public Camera cam;

    private float stopHeight = 20f;
    private float lerpSpeed = 2f;

    private void Update()
    {
        endPos = new Vector3(FPSCam.transform.position.x, stopHeight, FPSCam.transform.position.z);

        if (moveToAir)
        {
            // camera beweegt langzaam naar aangewezen plaats
            cam.transform.position = Vector3.Lerp(cam.transform.position, endPos, lerpSpeed * Time.deltaTime);
        }

        // camera stopt met bewegen als die is aangekomen op locatie
        if ((cam.transform.position.y >= (stopHeight - 0.01f)))
        {
            moveToAir = false;
        }
    }

    public void GodCam()
    {
        moveToAir = true;

        cam.transform.position = FPSCam.transform.position;

        FPSCam.SetActive(false);
        cam.enabled = true;

        godCam.AddComponent<AudioListener>();
    }
}
