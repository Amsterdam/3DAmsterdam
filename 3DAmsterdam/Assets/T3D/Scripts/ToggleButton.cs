using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleButton :MonoBehaviour
{
    public Sprite offSprite;
    public Sprite onSprite;

    private UnityEngine.UI.Toggle toggle;
    private Image image;

    void Start()
    {
        toggle = GetComponent<UnityEngine.UI.Toggle>();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        image = GetComponent<UnityEngine.UI.Image>();
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn) image.sprite = onSprite;
        else image.sprite = offSprite;
    }

}
