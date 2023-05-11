using Netherlands3D.Events;
using Netherlands3D.Interface.Layers;
using Netherlands3D.JavascriptConnection;
using Netherlands3D.TileSystem;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Netherlands3D.Interface;

public class AccordionController : MonoBehaviour
{
    [Header("Logic")]
    [SerializeField]
    private bool isOpen = false;

    [SerializeField]
    private GameObject accordionChildrenGroup;
    private List<GameObject> directChildren;
    private List<AccordionController> allChildren;

    [SerializeField]
    private bool generateChildrenWithLinkedObject = false;
    [SerializeField]
    private GameObject accordion3Prefab;

    [Header("Customizations")]
    [SerializeField]
    private GameObjectEvent openColorOptions;
    [SerializeField]
    private MaterialEvent selectedMaterialEvent;
    [SerializeField]
    private GameObject linkedObject;
    public GameObject LinkedObject { get => linkedObject; set => linkedObject = value; }
    [SerializeField]
    private Toggle toggleActiveLayer;
    [SerializeField]
    protected LayerType layerType = LayerType.STATIC;
    public LayerType LayerType { get => layerType; }


    [Header("Visual")]
    [SerializeField]
    private string title;
    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Sprite openedSprite;
    [SerializeField]
    private Material legendColorMaterial;
    public Material LegendColorMaterial { get => legendColorMaterial; set => legendColorMaterial = value; }
    [SerializeField]
    private bool activateCheckMark = true;


    [Header("Link to components")]
    [SerializeField]
    private TMP_Text titleField;
    [SerializeField]
    private GameObject legendColor;
    [SerializeField]
    private GameObject checkmark;
    [SerializeField]
    private GameObject radioButton;
    [SerializeField]
    private GameObject colorButton;
    [SerializeField]
    private GameObject chevron;
    [SerializeField]
    private Image backgroundImage;

    [Header("Listening events")]
    [SerializeField]
    private ColorEvent selectedColorEvent;

    // Start is called before the first frame update
    void Awake()
    {
        //Get the direct children accordion
        directChildren = new List<GameObject>();
        foreach (Transform transform in accordionChildrenGroup.transform)
        {
            directChildren.Add(transform.gameObject);
        }

        //Get all the accordions under parent
        allChildren = GetAllAccordionChildren(gameObject.transform);

        //If yes: parent to generated accordions
        if (generateChildrenWithLinkedObject & linkedObject)
        {
            foreach (var material in linkedObject.GetComponent<BinaryMeshLayer>().DefaultMaterialList)
            {
                var generatedAccordion = Instantiate(accordion3Prefab, accordionChildrenGroup.transform);

                var controller = generatedAccordion.GetComponent<AccordionController>();
                controller.linkedObject = linkedObject;
                controller.title = material.name;
                controller.legendColorMaterial = material;
                controller.activateCheckMark = false;

                //generatedAccordion.SetActive(true);
                directChildren.Add(generatedAccordion);
            }
        }

    }

    private void SetAccordionComponents()
    {
        if (titleField) titleField.text = title;

        //Chevron is (not) shown depending if there are children
        if (chevron) chevron.SetActive(directChildren.Count > 0);

        //Making sure the right data is there for the paintbrush
        if (colorButton) colorButton.SetActive(directChildren.Count == 0);

        //TODO: Set color?
        if (legendColor)
        {
            legendColor.SetActive(openColorOptions && legendColorMaterial);
            if (legendColorMaterial) legendColor.GetComponent<Image>().color = legendColorMaterial.GetColor("_BaseColor");
        }

        if (checkmark) checkmark.SetActive(activateCheckMark);
    }

    void Start()
    {
        SetAccordionComponents();

        //By default
        //Tabs are closed
        ToggleChildren(false);

    }

    //Functional/event logic
    /// <summary>
    /// Enable or Disable the linked GameObject
    /// </summary>
    /// <param name="isOn"></param>
    public void ToggleLinkedObject(bool isOn)
    {
        if (!linkedObject)
        {
            return;
        }
        if (layerType == LayerType.STATIC)
        {
            var staticLayer = LinkedObject.GetComponent<Layer>();
            if (staticLayer == null)
            {
                LinkedObject.SetActive(isOn);
            }
            else
            {
                //Static layer components better use their method to enable/disable, because maybe only the children should be disabled/reenabled
                staticLayer.isEnabled = isOn;
            }
        }
        else
        {
            LinkedObject.SetActive(isOn);
        }

        toggleActiveLayer.SetIsOnWithoutNotify(isOn);
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

    public void ChevronPressed()
    {
        //Logic
        isOpen = !isOpen;
        ToggleChildren(isOpen);

        //Visuals
        if (chevron) FlipChevron();
        backgroundImage.sprite = isOpen ? openedSprite : defaultSprite;
    }

    private void FlipChevron()
    {
        chevron.GetComponent<RectTransform>().localScale = new Vector3(1, isOpen ? -1 : 1);
    }

    private void ToggleChildren(bool isOpen)
    {
        //Close
        if (!isOpen) 
        {
            foreach (var child in allChildren)
            {
                child.gameObject.SetActive(false);
            }

        }
        //Open
        else
        {
            foreach (var child in directChildren)
            {
                child.SetActive(true);
            }
        }
    }

    private List<AccordionController> GetAllAccordionChildren(Transform parent)
    {
        List<AccordionController> accordions = new List<AccordionController>();
        foreach (Transform child in parent)
        {
            AccordionController component;
            if ((component = child.GetComponent<AccordionController>()) != null)
            {
                accordions.Add(component);
            }

            GetAllAccordionChildren(child).ForEach(x => accordions.Add(x));
        }

        return accordions;

    }
}
