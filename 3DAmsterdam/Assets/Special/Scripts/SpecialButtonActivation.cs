using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialButtonActivation : MonoBehaviour
{
    [SerializeField]
    private Button backToDamButton;

    [SerializeField]
    private Button startStreetViewFireworksButton;

    [SerializeField]
    private GameObject fireworksOptions;

    [SerializeField]
    private Transform distanceReference;

    [SerializeField]
    private float triggerDistance = 300.0f;

    [SerializeField]
    private BillboardText billboardText;

    [SerializeField]
    private bool forceFirstPersonMode = false;


	void Update()
    {
        //Show streetview fireworks button if we are in god view and we are within the fireworks window 
        startStreetViewFireworksButton.gameObject.SetActive(billboardText.allowFireworks && CameraModeChanger.Instance.CameraMode != CameraMode.StreetView);

		//Activate button if distance is too far, and countdown is about to start, and we are not already showing the fireworks button
		backToDamButton.gameObject.SetActive(!startStreetViewFireworksButton.gameObject.activeSelf && billboardText.countDownAboutToStart && Vector3.Distance(CameraModeChanger.Instance.ActiveCamera.transform.position, distanceReference.transform.position) > triggerDistance);

        //Show fireworks options when they are allowed
        fireworksOptions.SetActive(billboardText.allowFireworks && CameraModeChanger.Instance.CameraMode == CameraMode.StreetView);
    }

    public void GoToTheDam()
    {
        CameraModeChanger.Instance.ActiveCamera.transform.position = billboardText.cameraStartpositie;
        CameraModeChanger.Instance.ActiveCamera.transform.rotation = billboardText.cameraStartRotatie;
    }

    public void StartFirstPersonFireWorks()
    {
        CameraModeChanger.Instance.FirstPersonMode(billboardText.cameraStartpositie, billboardText.cameraStartRotatie);
    }
}
