using Netherlands3D.Cameras;
using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface
{
    public class CameraHeightSlider : MonoBehaviour
    {
        private Slider slider;
        [SerializeField]
        private TextMeshProUGUI heightText;

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

		private void Start()
		{
            CameraModeChanger.Instance.OnGodViewModeEvent += EnableObject;
            CameraModeChanger.Instance.OnFirstPersonModeEvent += DisableObject;

            CameraModeChanger.Instance.OnOrtographicModeEvent += DisableObject;
            CameraModeChanger.Instance.OnPerspectiveModeEvent += EnableObject;
        }

        private void EnableObject()
        {
            gameObject.SetActive(true);
		}
        private void DisableObject()
        {
            gameObject.SetActive(false);
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