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

    Button btn;
    bool isOn;

    void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);

        //toggle = GetComponent<Toggle>();
        //toggle.onValueChanged.AddListener(OnToggleValueChanged);
        image = GetComponent<Image>();
    }

    void OnClick()
    {
        isOn = !isOn;
        image.sprite = isOn ? onSprite : offSprite;
    }

    private void OnToggleValueChanged(bool isOn)
    {
        isOn = !isOn;

        if (isOn) image.sprite = onSprite;
        else image.sprite = offSprite;
    }

}
