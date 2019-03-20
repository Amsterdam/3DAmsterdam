using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class ModeManager : MonoBehaviour
{
    public GameObject zoomIn, zoomOut;
    public GameObject streetViewButton, godModeButton;

    public GameObject FPSCam;
    public Camera cam;

    [HideInInspector]
    public int mode = 1; // FPS of god modus
    private int FPSMode = 1; // switchen in FPSmodus van menu kunnen selecteren of camera gebruiken

    void Update()
    {
        // als 'god modus' is geselecteerd dan wordt de FPS camera uitgezet
        if (cam.enabled)
        {
            mode = 1;

            FPSCam.SetActive(false);
            zoomIn.SetActive(true);
            zoomOut.SetActive(true);
            streetViewButton.SetActive(true);
            godModeButton.SetActive(false);
        } // als 'FPS modus' is geselecteerd wordt de FPS camera aangezet
        else
        {
            mode = -1;

            FPSCam.SetActive(true);
            zoomIn.SetActive(false);
            zoomOut.SetActive(false);
            streetViewButton.SetActive(false);
            godModeButton.SetActive(true);


            // het switchen van FPSmodus
            if (Input.GetKeyDown(KeyCode.F)) FPSMode *= -1;

            // camera kan gebruikt worden
            if (FPSMode == 1)
            {
                FPSCam.GetComponent<FirstPersonController>().enabled = true;
            } // menus kunnen geselecteerd worden
            else
            {
                FPSCam.GetComponent<FirstPersonController>().enabled = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
