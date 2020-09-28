using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraHeightSlider : MonoBehaviour
{
    private CameraManager cameraManager;
    private Slider slider;
    [SerializeField]
    private Text heightText;

    [SerializeField]
    private string textSuffix = "m";

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Start()
    {
        cameraManager = CameraManager.instance;
    }

    public void HeightSliderChanged(float sliderValue){
       // cameraControls.SetNormalizedCameraHeight(sliderValue);
    }

    void LateUpdate()
    {
        heightText.text = Mathf.Round(cameraManager.currentCameraControlsComponent.GetCameraHeight()) + textSuffix;
        slider.normalizedValue = cameraManager.currentCameraControlsComponent.GetNormalizedCameraHeight();
    }
}
