using Netherlands3D.Cameras;
using ConvertCoordinates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
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

        public void HeightSliderChanged(float sliderValue)
        {
            CameraModeChanger.Instance.CurrentCameraControls.SetNormalizedCameraHeight(sliderValue);
        }

        void LateUpdate()
        {
            heightInNAP = Mathf.Round(CameraModeChanger.Instance.ActiveCamera.transform.position.y - Config.activeConfiguration.zeroGroundLevelY);
            heightText.text = heightInNAP + textSuffix;

            slider.normalizedValue = CameraModeChanger.Instance.CurrentCameraControls.GetNormalizedCameraHeight();
        }
    }
}