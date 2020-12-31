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
    private Button replayCountdownButton;

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

    private SunVisuals fireWorksLightOverride;

    [SerializeField]
    private float lightFlickerSpeed = 0.5f;

	private void Start()
	{   
        //Find the sun directional light
        fireWorksLightOverride = FindObjectOfType<SunVisuals>();

        StartCoroutine(LightFlicker());
    }

	void Update()
    {
        //Allow flicker of light colors
        fireWorksLightOverride.customColor = billboardText.allowFireworksFlicker;

        //Show streetview fireworks button if we are in god view and we are within the fireworks window 
        startStreetViewFireworksButton.gameObject.SetActive(billboardText.allowFireworks && CameraModeChanger.Instance.CameraMode != CameraMode.StreetView);

		//Activate button if distance is too far, and countdown is about to start, and we are not already showing the fireworks button
		backToDamButton.gameObject.SetActive(!startStreetViewFireworksButton.gameObject.activeSelf && billboardText.countDownAboutToStart && Vector3.Distance(CameraModeChanger.Instance.ActiveCamera.transform.position, distanceReference.transform.position) > triggerDistance);

        //Show fireworks options when they are allowed
        fireworksOptions.SetActive(PointerLock.GetMode() == PointerLock.Mode.FIRST_PERSON && billboardText.allowFireworks);

        replayCountdownButton.gameObject.SetActive(billboardText.allowReplay && !startStreetViewFireworksButton.gameObject.activeSelf && CameraModeChanger.Instance.CameraMode != CameraMode.StreetView);
    }
    IEnumerator LightFlicker(){
        while (true) {
            if (billboardText.allowFireworksFlicker) {
                var startColor = fireWorksLightOverride.SunDirectionalLight.color;
                var newColor = new Color(Random.Range(0.2f, 1.0f), Random.Range(0.2f, 1.0f), Random.Range(0.2f, 1.0f));

                float elapsedTime = 0.0f;
                float totalTime = Random.Range(0.2f, 1.0f);
                while (elapsedTime < totalTime)
                {
                    elapsedTime += Time.deltaTime;
                    fireWorksLightOverride.SunDirectionalLight.color = Color.Lerp(startColor, newColor, (elapsedTime / totalTime));
                    yield return null;
                }

                yield return null;
            }
            yield return null;
        }
        yield return null;
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

    public void ReplayNewyearMessage()
    {
        billboardText.GoToCountdown();
        StartFirstPersonFireWorks();
    }
}
