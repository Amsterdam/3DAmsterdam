using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraHeightSlider : MonoBehaviour
{
    private CameraControls cameraControls;
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
        cameraControls = FindObjectOfType<CameraControls>();
    }

    public void HeightSliderChanged(float sliderValue){
        cameraControls.SetNormalizedCameraHeight(sliderValue);
    }

    void LateUpdate()
    {
        heightText.text = Mathf.Round(cameraControls.GetCameraHeight()) + textSuffix;
        slider.normalizedValue = cameraControls.GetNormalizedCameraHeight();
    }
}
