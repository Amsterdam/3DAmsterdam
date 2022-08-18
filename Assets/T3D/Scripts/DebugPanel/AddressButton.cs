using System.Collections;
using System.Collections.Generic;
using ConvertCoordinates;
using UnityEngine;
using UnityEngine.UI;

public class AddressButton : MonoBehaviour
{
    private Button button;
    private Image buttonImage;
    private Text buttonText;

    private Color defaultColor;
    private Color defaultTextColor;

    [SerializeField]
    private Color selectedColor;
    [SerializeField]
    private Color selectedTextColor;

    [SerializeField]
    private string bagId;
    [SerializeField]
    private Vector3 positionRD; //as default Vector3 to allow serialization

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonText = button.GetComponentInChildren<Text>();

        defaultColor = buttonImage.color;
        defaultTextColor = buttonText.color;
    }

    private void Start()
    {
        if (bagId == DebugSettings.BagId)
        {
            ButtonClickAction();
        }
    }

    private void OnEnable()
    {
        button.onClick.AddListener(ButtonClickAction);
    }

    public void ResetButtoncolors()
    {
        buttonImage.color = defaultColor;
        buttonText.color = defaultTextColor;
    }

    public void ButtonClickAction()
    {
        DebugSettings.BagId = bagId;
        DebugSettings.PositionRD = new Vector3RD(positionRD.x, positionRD.y, positionRD.z);
        SetButtonColors();
    }

    private void SetButtonColors()
    {
        var allButtons = transform.parent.GetComponentsInChildren<AddressButton>();
        foreach (var btn in allButtons)
        {
            btn.ResetButtoncolors();
        }

        buttonImage.color = selectedColor;
        buttonText.color = selectedTextColor;
    }
}
