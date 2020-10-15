using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraHeightSlider : MonoBehaviour
{
    private Slider slider;
    [SerializeField]
    private Text heightText;

    [SerializeField]
    private string textSuffix = "m";

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void HeightSliderChanged(float sliderValue){
        CameraModeChanger.Instance.CurrentCameraControls.SetNormalizedCameraHeight(sliderValue);
    }

    void LateUpdate()
    {
        heightText.text = Mathf.Round(CameraModeChanger.Instance.CurrentCameraControls.GetCameraHeight()) + textSuffix;
        slider.normalizedValue = CameraModeChanger.Instance.CurrentCameraControls.GetNormalizedCameraHeight();
    }
}
