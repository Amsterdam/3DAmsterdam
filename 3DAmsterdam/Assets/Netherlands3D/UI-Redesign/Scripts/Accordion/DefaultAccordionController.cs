using Netherlands3D.Events;
using Netherlands3D.Interface.Layers;
using Netherlands3D.JavascriptConnection;
using Netherlands3D.TileSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DefaultAccordionController : MonoBehaviour
{
    [Header("Logic")]
    [SerializeField]
    private bool isOpen = false;

    [SerializeField]
    private GameObject accordionChildrenGroup;
    public GameObject AccordionChildrenGroup { get => accordionChildrenGroup; set => accordionChildrenGroup = value; }
    private List<GameObject> allChildrenGroup;

    [Header("Customizations")]
    [SerializeField]
    private GameObject linkedObject;
    public GameObject LinkedObject { get => linkedObject; set => linkedObject = value; }
    [SerializeField]
    protected LayerType layerType = LayerType.STATIC;
    public LayerType LayerType { get => layerType; }


    [Header("Visual")]
    [SerializeField]
    private string title;
    public string Title { get => title; set => title = value; }
    [SerializeField]
    private bool toggleIsOn = true;

    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Sprite openedSprite;
    [SerializeField]
    private bool activateCheckMark = true;
    public bool ActivateCheckMark { get => activateCheckMark; set => activateCheckMark = value; }


    [Header("Link to components")]
    [SerializeField]
    private TMP_Text titleField;
    [SerializeField]
    private GameObject chevron;
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Toggle toggle;

    // Start is called before the first frame update
    void Awake()
    {
        //Get all the accordions under parent
        allChildrenGroup = new List<GameObject>();
        foreach (var component in GetComponentsInChildren<DefaultAccordionController>()) {
            allChildrenGroup.Add(component.AccordionChildrenGroup);
        }

        SetAccordionComponents();
    }


    private void OnValidate()
    {
        SetAccordionComponents();
    }


    private void SetAccordionComponents()
    {
        if (titleField) titleField.text = title;

        //Chevron is (not) shown depending if there are children
        if (chevron) { 
            chevron.SetActive(accordionChildrenGroup.GetComponentsInChildren<DefaultAccordionController>().Length > 0); 
        }

        if (toggle)
        {
            toggle.gameObject.SetActive(activateCheckMark);
            toggle.isOn = toggleIsOn;
        }



    }

    void Start()
    {
        //SetAccordionComponents();

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
        CascadeToggle(isOn);

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

        toggle.SetIsOnWithoutNotify(isOn);
    }

    private void CascadeToggle(bool isOn)
    {
        foreach (var component in GetComponentsInChildren<DefaultAccordionController>())
        {
            component.toggle.isOn = isOn;
        }
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
        if (isOpen)
        {
            AccordionChildrenGroup.SetActive(true);

        }
        else
        {
            foreach (var child in allChildrenGroup)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
