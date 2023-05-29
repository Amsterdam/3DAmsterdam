using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;

public class DataChildAccordionController : MonoBehaviour
{
    private DefaultAccordionController _defaultController;

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

    void Awake()
    {
        _defaultController = GetComponent<DefaultAccordionController>();
        //SetAccordionComponents();
    }

    private void Start()
    {
        SetAccordionComponents();
    }

    private void SetAccordionComponents()
    {
        _defaultController.ActivateCheckMark = false;

        //TODO: Set color?
        if (legendColor && legendColorMaterial)
        {
            if (legendColorMaterial) legendColor.GetComponent<Image>().color = legendColorMaterial.GetColor("_BaseColor");
        }
    }

    public void OpenColorOptions()
    {
        //So that not all legend color are listening
        selectedColorEvent.RemoveAllListenersStarted();

        //Invokers
        openColorOptions.InvokeStarted(gameObject); //to open the color popup
        selectedMaterialEvent.InvokeStarted(legendColorMaterial); //

        //Listeners
        selectedColorEvent.AddListenerStarted(SetIconColor); //if color in color popup is changed, change the icon color
        selectedColorEvent.AddListenerStarted(SetMaterialColor); //if color in color popup is changed, change the  
    }

    private void SetIconColor(Color pickedColor)
    {
        legendColor.GetComponent<Image>().color = pickedColor;
    }

    private void SetMaterialColor(Color pickedColor)
    {
        legendColorMaterial.SetColor("_BaseColor", pickedColor);
    }
}
