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
    private List<AccordionController> directChildren;
    private List<AccordionController> allChildren;


    [Header("Customizations")]
    [SerializeField]
    private GameObjectEvent openColorOptions;
    [SerializeField]
    private GameObject linkedObject;
    public GameObject LinkedObject { get => linkedObject; set => linkedObject = value; }
    [SerializeField]
    private Toggle toggleActiveLayer;



    [SerializeField]
    protected LayerType layerType = LayerType.STATIC;
    public LayerType LayerType { get => layerType; }

    //Color
    [SerializeField]
    private List<Material> uniqueLinkedObjectMaterials;
    public List<Material> UniqueLinkedObjectMaterials { get => uniqueLinkedObjectMaterials; set => uniqueLinkedObjectMaterials = value; }

    public Material opaqueShaderSourceOverride;
    public Material transparentShaderSourceOverride;
    public List<Color> ResetColorValues { get => resetColorValues; set => resetColorValues = value; }
    private List<Color> resetColorValues;
    [Tooltip("Override shaders instead of copying material source properties")]
    public bool swapTransparentMaterialSources = false;
    [Tooltip("Enable if materials are created on the fly within the layer of this linked object")]
    public bool usingRuntimeInstancedMaterials = false;






    [Header("Visuals")]
    [SerializeField]
    private TMP_Text titleField;
    [SerializeField]
    private string title;


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
    private Sprite defaultSprite;
    [SerializeField]
    private Sprite openedSprite;
    [SerializeField]
    private Image image;

    // Start is called before the first frame update
    void Awake()
    {
        //Get the children accordion
        directChildren = new List<AccordionController>();
        foreach (Transform transform in accordionChildrenGroup.transform)
        {
            AccordionController component;
            if ((component = transform.GetComponent<AccordionController>()) != null)
            {
                directChildren.Add(component);
            }
        }

        allChildren = GetAllAccordionChildren(gameObject.transform);

        //Chevron is (not) shown depending if there are children
        if (chevron) chevron.SetActive(directChildren.Count > 0);

        //Making sure the right data is there for the paintbrush
        if (colorButton) colorButton.SetActive(openColorOptions && linkedObject && directChildren.Count == 0);
        if (legendColor)
        {
            legendColor.SetActive(openColorOptions && linkedObject); //TODO: not just this,also if just color withotu edit
            //TODO: color should be set
        }

        //TODO: change
        //if (checkmark) checkmark.SetActive(directChildren.Count > 0);

        /*
        //If we set a linkedObject manualy, get the color.
        if (LinkedObject)
        {
            var binaryMeshLayer = LinkedObject.GetComponent<BinaryMeshLayer>();
            if (binaryMeshLayer && UniqueLinkedObjectMaterials)
                UniqueLinkedObjectMaterials = binaryMeshLayer.DefaultMaterialList[0];

            UpdateLayerPrimaryColor();
            //GetResetColorValues();
        }
        */
    }

        void Start()
    {
        if (titleField) titleField.text = title;
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
        openColorOptions.InvokeStarted(gameObject);
    }

    public void UpdateLayerPrimaryColor()
    {
        if (uniqueLinkedObjectMaterials.Count > 0)
        {
            var primaryColor = uniqueLinkedObjectMaterials[0].GetColor("_BaseColor");
            primaryColor.a = 1.0f;
            legendColor.GetComponent<Renderer>().material.color = primaryColor;
        }
    }
    //Visual logic

    public void ChevronPressed()
    {

        //Logic
        isOpen = !isOpen;
        ToggleChildren(isOpen);

        //Visuals
        if (chevron) FlipChevron();
        //Background imagge
        image.sprite = isOpen ? openedSprite : defaultSprite;

        Debug.Log($"Chevron pressed: {isOpen}");
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
                Debug.Log($"Close 1: {child}");

            }
            Debug.Log($"Close 2:");

        }
        //Open
        else
        {
            foreach (var child in directChildren)
            {
                child.gameObject.SetActive(true);
                Debug.Log($"Open 1: {child}");

            }

            Debug.Log($"Open 2:");

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
    /*

    //Interface layer



    //Color
    [SerializeField]
    private Material transparentMaterialSource;
    [SerializeField]
    private Material opaqueMaterialSource;
    [SerializeField]
    private TextMeshProUGUI materialTitle;

    private Color resetMaterialColor;

    private Material targetMaterial;
    private LayerVisualsv2 layerVisuals;

    private bool selected = false;

    public float materialOpacity = 1.0f;

    private bool swapLowOpacityMaterialProperties = false;
    public Color GetMaterialColor => targetMaterial.GetColor("_BaseColor");



    /// <summary>
    /// Changes the color of the Material that is linked to this slot
    /// </summary>
    /// <param name="pickedColor">The new color for the linked Material</param>
    public void ChangeColor(Color pickedColor)
    {
        legendColor.GetComponent<Renderer>().material.color = pickedColor;

        if (targetMaterial)
            targetMaterial.SetColor("_BaseColor", new Color(pickedColor.r, pickedColor.g, pickedColor.b, materialOpacity));

        if (layerVisuals && layerVisuals.targetInterfaceLayer.usingRuntimeInstancedMaterials)
            CopyPropertiesToAllChildMaterials();
    }

    /// <summary>
    /// Sets the material target and reference the target LayerVisuals where this slot is in.
    /// </summary>
    /// <param name="target">The Material this slot targets</param>
    /// <param name="targetLayerVisuals">The target LayerVisuals where this slot is in</param>
    public void Init(Material target, Color resetColor, LayerVisualsv2 targetLayerVisuals, Material transparentMaterialSourceOverride = null, Material opaqueMaterialSourceOverride = null, bool swapMaterialSources = false)
    {
        targetMaterial = target;
        swapLowOpacityMaterialProperties = swapMaterialSources;

        //Optional non standard shader type overrides ( for layers with custom shaders )
        if (swapMaterialSources)
        {
            swapLowOpacityMaterialProperties = true;
            transparentMaterialSource = transparentMaterialSourceOverride;
            opaqueMaterialSource = opaqueMaterialSourceOverride;
        }
        //Set tooltip text. Users do not need to know if a material is an instance.
        var materialName = targetMaterial.name.Replace(" (Instance)", "");

        //Filter out our externail textures tag
        if (materialName.Contains("[texture="))
            materialName = materialName.Split('[')[0].Trim();

        GetComponent<TooltipTrigger>().TooltipText = materialName;
        materialTitle.text = materialName;

        var materialColor = GetMaterialColor;
        legendColor.GetComponent<Renderer>().material.color = new Color(materialColor.r, materialColor.g, materialColor.b, 1.0f);
        materialOpacity = materialColor.a;

        resetMaterialColor = resetColor;

        layerVisuals = targetLayerVisuals;
    }

    /// <summary>
    /// Copies the target material of this material slot to all child materials found within the layer linked object
    /// </summary>
    private void CopyPropertiesToAllChildMaterials()
    {
        MeshRenderer[] childRenderers = layerVisuals.targetInterfaceLayer.LinkedObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in childRenderers)
        {
            if (meshRenderer.sharedMaterial != targetMaterial)
            {
                var optionalColorIDMap = meshRenderer.sharedMaterial.GetTexture("_HighlightMap");
                meshRenderer.sharedMaterial.shader = targetMaterial.shader;
                meshRenderer.sharedMaterial.CopyPropertiesFromMaterial(targetMaterial);
                if (optionalColorIDMap)
                    meshRenderer.sharedMaterial.SetTexture("_HighlightMap", optionalColorIDMap);
            }
        }
    }

    /// <summary>
    /// Changes the opacity of the material, and always swap the shader type to the faster Opaque surface when opacity is 1.
    /// </summary>
    /// <param name="opacity">Opacity value from 0.0 to 1.0</param>
    public void ChangeOpacity(float opacity)
    {
        if (materialOpacity == opacity)
        {
            return;
        }
        else
        {
            materialOpacity = opacity;
            SwitchShaderAccordingToOpacity();
        }

        //We may not have to do this if we can force the sunlight shadow to ignore cutout.
        targetMaterial.SetShaderPassEnabled("ShadowCaster", (opacity == 1.0f));

        if (layerVisuals.targetInterfaceLayer.usingRuntimeInstancedMaterials)
            CopyPropertiesToAllChildMaterials();
    }

    private void SwitchShaderAccordingToOpacity()
    {
        if (materialOpacity < 1.0f)
        {
            SwapShaderToTransparent();
        }
        else
        {
            SwapShaderToOpaque();
        }
    }

    private void SwapShaderToOpaque()
    {
        if (swapLowOpacityMaterialProperties)
        {
            targetMaterial.CopyPropertiesFromMaterial(opaqueMaterialSource);
            targetMaterial.SetFloat("_Surface", 0); //0 Opaque
        }
        targetMaterial.SetColor("_BaseColor", legendColor.GetComponent<Renderer>().material.color);
    }

    private void SwapShaderToTransparent()
    {
        if (swapLowOpacityMaterialProperties)
        {
            targetMaterial.CopyPropertiesFromMaterial(transparentMaterialSource);
            targetMaterial.SetFloat("_Surface", 1); //1 Alpha
        }
        var color = legendColor.GetComponent<Renderer>().material.color;
        color.a = materialOpacity;
        targetMaterial.SetColor("_BaseColor", color);
    }

    */



}
