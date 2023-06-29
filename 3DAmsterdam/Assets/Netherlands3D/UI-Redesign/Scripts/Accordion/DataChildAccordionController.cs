using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;

public class DataChildAccordionController : MonoBehaviour
{
    [Header("Invoked events")]
    [SerializeField]
    private GameObjectEvent openColorOptions;
    [SerializeField]
    private MaterialEvent selectedMaterialEvent;

    [Header("Listening events")]
    [SerializeField]
    private ColorEvent selectedColorEvent;


    [Header("Link to components")]
    [SerializeField]
    private GameObject legendColor;
    [SerializeField]
    private GameObject colorButton;

    [SerializeField]
    private Material legendColorMaterial;
    public Material LegendColorMaterial { get => legendColorMaterial; set => legendColorMaterial = value; }

    private void Start()
    {
        SetAccordionComponents();
    }

    private void SetAccordionComponents()
    {
        if (GetComponent<DefaultAccordionController2>()) GetComponent<DefaultAccordionController2>().ToggleCheckmark(false);

        //TODO: Set color?
        if (legendColor && legendColorMaterial)
        {
            SetLegendColour(legendColorMaterial.GetColor("_BaseColor"));
        }
    }

    public void ActivateColorButton(bool showColorButton = true)
    {
        colorButton.SetActive(showColorButton);
    }

    public void SetLegendColour(Color colour)
    {
        if (legendColor) legendColor.GetComponent<Image>().color = colour;
    }

    public void OpenColorOptions()
    {
        //So that not all legend color are listening
        selectedColorEvent.RemoveAllListenersStarted();

        //Invokers
        openColorOptions.InvokeStarted(gameObject); //to open the color popup
        selectedMaterialEvent.InvokeStarted(legendColorMaterial); //to set the color

        //Listeners
        selectedColorEvent.AddListenerStarted(SetLegendColour); //if color in color popup is changed, change the icon color
        selectedColorEvent.AddListenerStarted(SetMaterialColor); //if color in color popup is changed, change the  
    }

    private void SetMaterialColor(Color pickedColor)
    {
        legendColorMaterial.SetColor("_BaseColor", pickedColor);
    }
}
