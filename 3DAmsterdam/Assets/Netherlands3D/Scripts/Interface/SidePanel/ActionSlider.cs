using Netherlands3D.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Netherlands3D.Interface.SidePanel
{
    public class ActionSlider : MonoBehaviour
    {
        private Action<float> changeAction;

        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI sliderTextMin;
        [SerializeField] private TextMeshProUGUI sliderTextMax;
        [SerializeField] private TextMeshProUGUI valueText;

        private bool fadingValueText = false;
        [SerializeField] private CanvasGroup valueTextGroup;
        private void Start()
        {
            slider.gameObject.AddComponent<AnalyticsClickTrigger>();
        }

        public void SliderChange(float value)
        {
            if (Selector.doingMultiselect)
            {
                value = Mathf.Round(value);
                slider.SetValueWithoutNotify(value);
            }

            if (changeAction != null) changeAction.Invoke(value);
            valueText.text = value.ToString("F2");
            valueTextGroup.alpha = 1.0f;

            if (!fadingValueText)
            {
                StartCoroutine(FadeValueText());
            }
        }

        private IEnumerator FadeValueText()
        {
            fadingValueText = true;
            while(valueTextGroup.alpha > 0.0f)
            {
                valueTextGroup.alpha -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            fadingValueText = false;
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