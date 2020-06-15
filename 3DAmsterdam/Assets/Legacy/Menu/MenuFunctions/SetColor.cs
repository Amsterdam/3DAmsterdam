using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetColor : MonoBehaviour
{
    private CUIColorPicker colorPicker;

    void Start()
    {
        colorPicker = GameObject.Find("CUIColorPicker").GetComponent<CUIColorPicker>();

        gameObject.GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    void ButtonClicked()
    {
        colorPicker.Color = gameObject.GetComponent<Image>().color;
    }
}
