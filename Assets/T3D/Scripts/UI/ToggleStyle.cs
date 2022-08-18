using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ToggleStyle : MonoBehaviour
{
    public static ColorBlock SelectedColors = new ColorBlock
    {
        normalColor = Color.black,
        highlightedColor = Color.black,
        pressedColor = new Color(55f / 255f, 55f / 255f, 55f / 255f),
        selectedColor = Color.black,
        disabledColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 0.5f),
        colorMultiplier = 1f,
        fadeDuration = 0.1f,
    };

    public static ColorBlock UnselectedColors = new ColorBlock
    {
        normalColor = Color.white,
        highlightedColor = Color.white,
        pressedColor = new Color(200f / 255f, 200f / 255f, 200f / 255f),
        selectedColor = Color.white,
        disabledColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 0.5f),
        colorMultiplier = 1f,
        fadeDuration = 0.1f,
    };

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        ChangeToggleStyle(toggle.isOn);
    }

    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(ChangeToggleStyle);
    }

    private void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(ChangeToggleStyle);
    }

    private void ChangeToggleStyle(bool active)
    {
        if (active)
        {
            toggle.colors = SelectedColors;
        }
        else
        {
            toggle.colors = UnselectedColors;
        }
    }
}
