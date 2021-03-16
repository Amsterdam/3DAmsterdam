using Netherlands3D.CameraMotion;
using ConvertCoordinates;
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
    private string textSuffix = "m NAP";

    private float heightInNAP;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void HeightSliderChanged(float sliderValue){
        CameraModeChanger.Instance.CurrentCameraControls.SetNormalizedCameraHeight(sliderValue);
    }

    void LateUpdate()
    {
        heightInNAP = Mathf.Round(CameraModeChanger.Instance.ActiveCamera.transform.position.y + (float)CoordConvert.referenceRD.z);
        heightText.text = heightInNAP + textSuffix;

        slider.normalizedValue = CameraModeChanger.Instance.CurrentCameraControls.GetNormalizedCameraHeight();
    }
}
