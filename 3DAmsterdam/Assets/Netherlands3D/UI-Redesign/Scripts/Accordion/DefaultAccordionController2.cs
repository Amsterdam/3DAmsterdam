using Netherlands3D.Events;
//using Netherlands3D.Interface.Layers;
using Netherlands3D.JavascriptConnection;
using Netherlands3D.TileSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

public class DefaultAccordionController2 : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private bool toggleEnabled = true;

    [SerializeField]
    private Toggle chevron;

    [SerializeField]
    private Transform accordionChildrenGroup;
    public Transform AccordionChildrenGroup => accordionChildrenGroup;
    private List<RectTransform> childrenPanels;

    [SerializeField] private TMP_Text titleField;

    [SerializeField] private bool excludeFromGroup = false;


    [Header("Visuals")]
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Sprite defaultBackgroundSprite;
    [SerializeField]
    private Sprite openedBackgroundSprite;
    [SerializeField]
    private HorizontalLayoutGroup startGroup;
    public void SetStartGroupLeftPadding(int amount) { startGroup.padding.left = amount; }

    [Header("Actions")]
    [SerializeField] private UnityEvent<bool> checkmarkAction;

    public bool ToggleEnabled
    {
        get { return toggleEnabled; }
        set { toggleEnabled = value; }
    }


    public bool IsOpen
    {
        get { return chevron ? chevron.isOn : false; }
        set { if (chevron) chevron.isOn = value; }
    }
    public bool ToggleIsOn
    {
        get { return toggle ? toggle.isOn : false; }
        set { if (toggle) toggle.isOn = value; }
    }
    public string Title
    {
        get { return titleField ? titleField.text : string.Empty; }
        set { if (titleField) titleField.text = value; }
    }

    public void SetDefaultBackgroundSprite(Sprite sprite) { defaultBackgroundSprite = sprite; }
    public void SetOpenedBackgroundSprite(Sprite sprite) { openedBackgroundSprite = sprite; }

    private void Awake()
    {
        SetAccordionComponents();
    }

    public void AddChildPanel(RectTransform childPanel, int index = -1)
    {
        childPanel.SetParent(accordionChildrenGroup);

        if (index >= 0)
            childPanel.SetSiblingIndex(index);

        SetAccordionComponents();
    }

    public void RemoveChildPanel(RectTransform childpanel)
    {
        childpanel.SetParent(null);
        SetAccordionComponents();
    }

    private void OnValidate()
    {
        SetAccordionComponents();
    }

    public void SetAccordionComponents()
    {
        //Chevron is (not) shown depending if there are children
        childrenPanels = accordionChildrenGroup.GetComponentsInChildren<RectTransform>().ToList();

        //Remove all panels that want to be excluded
        RectTransform panelToRemove = childrenPanels.Find(panel => panel.GetComponent<DefaultAccordionController2>()?.excludeFromGroup == true);
        if (panelToRemove != null)
        {
            childrenPanels.Remove(panelToRemove);
        }

        childrenPanels.RemoveAt(0); //remove accordionChildrenGroup itself
        if (chevron)
        {
            //chevron.transform.parent.gameObject.SetActive(childrenPanels.Count > 0);
            chevron.gameObject.SetActive(childrenPanels.Count > 0);
        }

        if (toggle)
        {
            toggle.gameObject.SetActive(ToggleEnabled);
            toggle.isOn = ToggleIsOn;
        }
    }

    private void Start()
    {
        SetAccordionComponents();

        //By default tabs are closed
        ToggleChildren(IsOpen);
    }

    private void ToggleChildren(bool isOpen)
    {
        accordionChildrenGroup.gameObject.SetActive(isOpen);
    }

    public void ToggleCheckmark(bool isOn)
    {
        toggle.SetIsOnWithoutNotify(isOn);
        CascadeToggle(isOn);

        checkmarkAction.Invoke(isOn);
    }

    private void CascadeToggle(bool isOn)
    {
        if (childrenPanels == null) return;

        foreach (var childPanel in childrenPanels)
        {
            var childAccordeon = childPanel.GetComponent<DefaultAccordionController2>();
            if (childAccordeon)
                childAccordeon.ToggleCheckmark(isOn);
        }
    }

    public void ChevronPressed(bool isOn)
    {
        //Logic
        IsOpen = isOn;
        ToggleChildren(IsOpen);

        //Visuals
        if (chevron) FlipChevron(IsOpen);

        if (backgroundImage) backgroundImage.sprite = IsOpen ? openedBackgroundSprite : defaultBackgroundSprite;
    }

    private void FlipChevron(bool upwards)
    {
        chevron.GetComponent<RectTransform>().localScale = new Vector3(1, upwards ? -1 : 1);
    }

    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage) backgroundImage.color = color;
    }
}
