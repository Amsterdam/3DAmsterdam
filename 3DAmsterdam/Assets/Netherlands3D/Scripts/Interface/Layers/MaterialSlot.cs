using Netherlands3D.JavascriptConnection;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
namespace Netherlands3D.Interface.Layers
{
	public class MaterialSlot : ChangePointerStyleHandler, IPointerClickHandler
	{
		[SerializeField]
		private Material transparentMaterialSource;
		[SerializeField]
		private Material opaqueMaterialSource;
		[SerializeField]
		private TextMeshProUGUI materialTitle;

		private Color resetMaterialColor;

		private Material targetMaterial;
		private LayerVisuals layerVisuals;

		private bool selected = false;

		public float materialOpacity = 1.0f;

		private bool swapLowOpacityMaterialProperties = false;

		private const string EXPLANATION_TEXT = "\nShift+Klik: Multi-select";

		private const bool disableShadowsOnLoweredOpacity = false;

		public bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				selected = value;
				selectedImage.enabled = selected;
			}
		}

		[SerializeField]
		private Image selectedImage;
		[SerializeField]
		private Image colorImage;
		public Color GetMaterialColor => targetMaterial.GetColor("_BaseColor");

		private void Start()
		{
			Selected = selected; //start unselected
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Select();
		}

		/// <summary>
		/// Sets the material target and reference the target LayerVisuals where this slot is in.
		/// </summary>
		/// <param name="target">The Material this slot targets</param>
		/// <param name="targetLayerVisuals">The target LayerVisuals where this slot is in</param>
		public void Init(Material target, Color resetColor, LayerVisuals targetLayerVisuals, Material transparentMaterialSourceOverride = null, Material opaqueMaterialSourceOverride = null, bool swapMaterialSources = false)
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
			colorImage.color = new Color(materialColor.r, materialColor.g, materialColor.b, 1.0f);
			materialOpacity = materialColor.a;

			resetMaterialColor = resetColor;

			layerVisuals = targetLayerVisuals;
		}

		/// <summary>
		/// User (multi)selection of the material slot
		/// </summary>
		private void Select()
		{
			var multiSelect = Selector.doingMultiselect;
			Selected = (multiSelect) ? !Selected : true;

			layerVisuals.SelectMaterialSlot(this, multiSelect);

			Debug.Log("Selected material " + targetMaterial.name);
		}

		/// <summary>
		/// Reset the material color back to what it was at initialisation.
		/// </summary>
		public void ResetColor()
		{
			materialOpacity = 1.0f;
			ChangeColor(resetMaterialColor);
		}

		/// <summary>
		/// Changes the color of the Material that is linked to this slot
		/// </summary>
		/// <param name="pickedColor">The new color for the linked Material</param>
		public void ChangeColor(Color pickedColor)
		{
			colorImage.color = pickedColor;
			if(targetMaterial)
				targetMaterial.SetColor("_BaseColor", new Color(pickedColor.r, pickedColor.g, pickedColor.b, materialOpacity));

			if(layerVisuals && layerVisuals.targetInterfaceLayer.usingRuntimeInstancedMaterials)
				CopyPropertiesToAllChildMaterials();
		}

		/// <summary>
		/// Copies the target material of this material slot to all child materials found within the layer linked object
		/// </summary>
		private void CopyPropertiesToAllChildMaterials()
		{
			MeshRenderer[] childRenderers = layerVisuals.targetInterfaceLayer.LinkedObject.GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer meshRenderer in childRenderers)
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
			if(materialOpacity == opacity)
			{
				return;
			}
			else{
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
			targetMaterial.SetColor("_BaseColor", colorImage.color);
		}

		private void SwapShaderToTransparent()
		{
			if (swapLowOpacityMaterialProperties)
			{
				targetMaterial.CopyPropertiesFromMaterial(transparentMaterialSource);
				targetMaterial.SetFloat("_Surface", 1); //1 Alpha
			}
			var color = colorImage.color;
			color.a = materialOpacity;
			targetMaterial.SetColor("_BaseColor", color);
		}
	}
}