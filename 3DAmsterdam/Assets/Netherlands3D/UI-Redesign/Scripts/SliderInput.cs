using Netherlands3D.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SliderInput : MonoBehaviour
{

    private float amount = 0;
    [SerializeField]
    private string prefix = "m";
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField] private TextMeshProUGUI sliderTextMin;
    [SerializeField] private TextMeshProUGUI sliderTextMax;

    private void Start()
    {
        slider.gameObject.AddComponent<AnalyticsClickTrigger>();
        sliderTextMin.text = slider.minValue.ToString() + prefix;
        sliderTextMax.text = slider.maxValue.ToString()+ prefix;
    }

    public void SliderOnChange()
    {
        amount = slider.value;
        inputField.text = amount.ToString();
    }

    public void InputOnChange()
    {
        amount = float.Parse(inputField.text);
        slider.value = amount;
    }
}
