using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
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

        public void SetAction(string minText, string maxText, float minValue, float maxValue, float defaultValue, Action<float> action)
        {
            sliderTextMin.text = minText;
            sliderTextMax.text = maxText;

            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;

            changeAction = action;
        }
    }
}