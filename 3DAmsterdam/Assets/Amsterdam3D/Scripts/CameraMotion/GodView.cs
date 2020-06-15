using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodView : MonoBehaviour
{
    private bool moveToAir = false;

    private Vector3 endPos;

    public GameObject FPSCam, godCam;
    public Camera cam;
    public CameraMovement camcontroller;
    public GameObject streetcamButton;
    public GameObject GodviewButton;

    private float stopHeight = 150f;
    private float lerpSpeed = 2f;

    private void Update()
    {
        

        if (moveToAir)
        {
           
            // camera beweegt langzaam naar aangewezen plaats
            cam.transform.position = Vector3.Lerp(cam.transform.position, endPos, lerpSpeed * Time.deltaTime);
        

        // camera stopt met bewegen als die is aangekomen op locatie
        if ((cam.transform.position.y >= (stopHeight - 0.01f)))
        {
            moveToAir = false;
            
            
        }
        }
    }

    public void GodCam()
    {
        streetcamButton.SetActive(true);
        GodviewButton.SetActive(false);
        moveToAir = true;
        endPos = new Vector3(cam.transform.position.x, stopHeight, cam.transform.position.z);
        camcontroller.IsFPSmode = false;
        //cam.transform.position = FPSCam.transform.position;

        //FPSCam.SetActive(false);
        //cam.enabled = true;

        //godCam.AddComponent<AudioListener>();
    }
}
