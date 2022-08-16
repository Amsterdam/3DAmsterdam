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
            slider.navigation = new Navigation()
            {
                mode = Navigation.Mode.None
            };
        }

        public void HeightSliderChanged(float sliderValue)
        {
            ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.SetNormalizedCameraHeight(sliderValue);
        }

        void LateUpdate()
        {
            heightInNAP = Mathf.Round(ServiceLocator.GetService<CameraModeChanger>().ActiveCamera.transform.position.y - Config.activeConfiguration.zeroGroundLevelY);
            heightText.text = heightInNAP + textSuffix;

            slider.normalizedValue = ServiceLocator.GetService<CameraModeChanger>().CurrentCameraControls.GetNormalizedCameraHeight();
        }
    }
}