using Netherlands3D.Events;
using Netherlands3D.Interface.Layers;
using Netherlands3D.JavascriptConnection;
using Netherlands3D.TileSystem;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class DataAccordionController : MonoBehaviour
{
    //private DefaultAccordionController _controller;

    [SerializeField]
    private bool generateChildrenWithLinkedObject = false;

    [SerializeField]
    private GameObject accordionChildPrefab;

    [SerializeField]
    private GameObject linkedObject;

    [SerializeField]
    private RootAccordionController rootAccordion;


    [Header("Transform necessities")]
    [SerializeField] private bool enableTransform = false;
    [SerializeField] private GameObject transformObject;
    [SerializeField] private UnityEvent<GameObject> openTransformOptions;


    public void SetFields(bool generateChildrenWithLinkedObject, GameObject accordionChildPrefab, GameObject linkedObject, bool enableTransform, RootAccordionController rootAccordion)
    {
        this.generateChildrenWithLinkedObject = generateChildrenWithLinkedObject;
        this.accordionChildPrefab = accordionChildPrefab;
        this.linkedObject = linkedObject;
        this.enableTransform = enableTransform;
        this.rootAccordion = rootAccordion; 

        Setup();
    }

    public void OnTransformButtonClick()
    {
        openTransformOptions.Invoke(linkedObject);
    }

    // Start is called before the first frame update
    void Awake()
    {
        Setup();
    }

    void Setup()
    {
        if (transformObject)  transformObject.SetActive(enableTransform);

        Transform childGroup = GetComponent<DefaultAccordionController2>()?.AccordionChildrenGroup.transform;

        //If yes: parent to generated accordions
        if (generateChildrenWithLinkedObject & linkedObject)
        {
            //For object that has materials
            if (linkedObject.GetComponent<BinaryMeshLayer>())
            {
                GenerateMaterialAccordions(childGroup);
            }
            //For object that has other subitsm
            else
            {
                //
            }
        }

        //To ensure the design is properly set
        rootAccordion.SetupAccordion();
    }

    public void SetUp(List<ColorLegendItem> legendItems)
    {

        Transform childGroup = GetComponent<DefaultAccordionController2>()?.AccordionChildrenGroup.transform;

        //Generate child accordions for the color accordion
        foreach (var item in legendItems)
        {
            var generatedAccordion = Instantiate(accordionChildPrefab, childGroup);
            var defaultController2 = generatedAccordion.GetComponent<DefaultAccordionController2>();
            if (defaultController2)
            {
                defaultController2.Title = item.Name;
                defaultController2.ToggleCheckmark(false);
            }


            var dataController = generatedAccordion.GetComponent<DataChildAccordionController>();
            dataController.SetLegendColour(item.Colour);
            dataController.ActivateColorButton(false);
        }

        //To ensure the design is properly set
        rootAccordion.SetupAccordion();

    }

    private void GenerateMaterialAccordions(Transform childGroup)
    {
        foreach (var material in linkedObject.GetComponent<BinaryMeshLayer>().DefaultMaterialList)
        {
            var generatedAccordion = Instantiate(accordionChildPrefab, childGroup);
            var defaultController2 = generatedAccordion.GetComponent<DefaultAccordionController2>();
            if (defaultController2)
            {
                //Removes uneedec chars
                var name = material.name.Contains('[') ? material.name.Substring(0, material.name.IndexOf('[')) : material.name;
                defaultController2.Title = name;
                defaultController2.ToggleCheckmark(false);
            }


            var dataController = generatedAccordion.GetComponent<DataChildAccordionController>();
            dataController.LegendColorMaterial = material;
        }
    }

    public void SetActiveLinkedObject(bool isOn)
    {
        linkedObject.SetActive(isOn);
    }
}
