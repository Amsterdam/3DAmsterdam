using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SetObjectToColor : MonoBehaviour
{
    private CUIColorPicker colorPicker;

    void Start()
    {
        colorPicker = GameObject.Find("CUIColorPicker").GetComponent<CUIColorPicker>();

        gameObject.GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    void ButtonClicked()
    {
        colorPicker.selectedObject = this.gameObject.name;
    }
}
