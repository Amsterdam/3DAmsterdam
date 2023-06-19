using Netherlands3D.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
public class SliderInput : MonoBehaviour
{

    private float amount;

    [SerializeField]
    private UnityEvent<float> amountChanged;

    [Header("Customisation")]
    [SerializeField]
    private string prefix = "m";

    [Header("Fields")]
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField] private TextMeshProUGUI sliderTextMin;
    [SerializeField] private TextMeshProUGUI sliderTextMax;
    [SerializeField] private TextMeshProUGUI prefixText;

    private void Start()
    {
        slider.gameObject.AddComponent<AnalyticsClickTrigger>();
        sliderTextMin.text = slider.minValue.ToString() + prefix;
        sliderTextMax.text = slider.maxValue.ToString()+ prefix;

        prefixText.text = prefix;
        amount = slider.value;
        inputField.text = amount.ToString();
    }

    public void SliderOnChange()
    {
        amount = slider.value;
        inputField.text = amount.ToString("0.00");

        amountChanged.Invoke(amount);
    }

    public void InputOnChange()
    {
        if (float.TryParse(inputField.text, out float value))
        {
            amount = value;

        } else
        {
            amount = (slider.minValue + slider.maxValue) / 2; //Takes the middle
        }

        slider.value = amount;
        amountChanged.Invoke(amount);
    }
}
