using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class ModeManager : MonoBehaviour
{
    public GameObject zoomIn, zoomOut;

    public GameObject FPSCam;
    public Camera cam;

    private int FPSMode = 1;

    void Update()
    {
        if (cam.enabled)
        {
            FPSCam.SetActive(false);
            zoomIn.SetActive(true);
            zoomOut.SetActive(true);
        }
        else
        {
            FPSCam.SetActive(true);
            zoomIn.SetActive(false);
            zoomOut.SetActive(false);

            if (Input.GetKeyDown(KeyCode.F)) FPSMode *= -1;

            if (FPSMode == 1)
            {
                FPSCam.GetComponent<FirstPersonController>().enabled = true;
            }
            else
            {
                FPSCam.GetComponent<FirstPersonController>().enabled = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
