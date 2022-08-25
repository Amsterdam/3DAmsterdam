using System;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.SidePanel
{
    public class ActionSlider : MonoBehaviour
    {
        private Action<float> changeAction;

        [SerializeField]
        private Slider slider;

        [SerializeField]
        private Text sliderTextMin;

        [SerializeField]
        private Text sliderTextMax;

        public void SliderChange(float value)
        {
            if (changeAction != null) changeAction.Invoke(value);
        }

        public void SetAction(string minText, string maxText, float minValue, float maxValue, float defaultValue, Action<float> action, bool wholeNumberSteps = false, string description = "")
        {
            gameObject.name = description;

            sliderTextMin.text = minText;
            sliderTextMax.text = maxText;

            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.wholeNumbers = wholeNumberSteps;

            slider.value = defaultValue;

            changeAction = action;
        }
    }
}