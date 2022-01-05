using Netherlands3D.JavascriptConnection;
using Netherlands3D.TileSystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Layers
{
	public enum LayerType
	{
		BASICSHAPE,
		OBJMODEL,
		STATIC,
		ANNOTATION,
		CAMERA
	}

	public class InterfaceLayer : ChangePointerStyleHandler
	{
		[SerializeField]
		private Text layerNameText;
		public string GetName => layerNameText.text;

		[SerializeField]
		protected LayerType layerType = LayerType.STATIC;
		public LayerType LayerType { get => layerType; }

		[SerializeField]
		private GameObject linkedObject;
		public GameObject LinkedObject { get => linkedObject; set => linkedObject = value; }

		[SerializeField]
		private bool hasOptions = true;

		[SerializeField]
		private GameObject customOptions;

		public Material opaqueShaderSourceOverride;
		public Material transparentShaderSourceOverride;

		[SerializeField]
		private List<Material> uniqueLinkedObjectMaterials;
		public List<Material> UniqueLinkedObjectMaterials { get => uniqueLinkedObjectMaterials; set => uniqueLinkedObjectMaterials = value; }
		public List<Color> ResetColorValues { get => resetColorValues; set => resetColorValues = value; }

		private List<Color> resetColorValues;

		private bool active = true;
		public bool Active
		{
			get
			{
				return active;
			}
			set
			{
				active = value;
				toggleActiveLayer.isOn = active;
			}
		}

		[SerializeField]
		private Image expandIcon;
		private bool expanded = false;
		[SerializeField]
		private bool autoCloseNeighbourLayers = true;

		[Tooltip("Enable if materials are created on the fly within the layer of this linked object")]
		public bool usingRuntimeInstancedMaterials = false;

		[SerializeField]
		private Toggle toggleActiveLayer;

		[HideInInspector]
		public InterfaceLayers parentInterfaceLayers;

		[SerializeField]
		private Image layerLabelColor;

		[Tooltip("Override shaders instead of copying material source properties")]
		public bool swapTransparentMaterialSources = false;

		private int maxNameLength = 24;
		private void Awake()
		{
			//If we set a linkedObject manualy, get the color.
			if (LinkedObject)
			{
				var binaryMeshLayer = LinkedObject.GetComponent<BinaryMeshLayer>();
				if (binaryMeshLayer && UniqueLinkedObjectMaterials.Count == 0) 
					UniqueLinkedObjectMaterials = binaryMeshLayer.DefaultMaterialList;

				UpdateLayerPrimaryColor();
				GetResetColorValues();
			}
		}

		private void Start()
		{
			parentInterfaceLayers = FindObjectOfType<InterfaceLayers>();
		}

		public void EnableOptions(bool enabled)
		{
			layerLabelColor.gameObject.SetActive(enabled);
		}

		/// <summary>
		/// Grab all the starting colors for this layer, so we can always reset it back during runtime
		/// </summary>
		private void GetResetColorValues()
		{
			//Store all the colors for the materials so we can reset to it later
			resetColorValues = new List<Color>();
			foreach (Material material in uniqueLinkedObjectMaterials)
				resetColorValues.Add(material.GetColor("_BaseColor"));
		}

		/// <summary>
		/// Reset all the linked materials their color back to their starting values
		/// </summary>
		public void ResetAllColors()
		{
			for (int i = 0; i < uniqueLinkedObjectMaterials.Count; i++)
			{
				uniqueLinkedObjectMaterials[i].SetColor("_BaseColor", resetColorValues[i]);
			}
			UpdateLayerPrimaryColor();
		}

		/// <summary>
		/// Create the connection between this 2D layer, and a 3D GameObject.
		/// Materials will be fetched according to the layer type.
		/// </summary>
		/// <param name="newLinkedObject">The GameObject to be linked</param>
		public void LinkObject(GameObject newLinkedObject)
		{
			LinkedObject = newLinkedObject;

			//Add script that keeps a two way connection to this layer
			AddTwoWayConnectionToLinkedObject();

			//linkedObject.Add
			switch (layerType)
			{
				case LayerType.BASICSHAPE:
					//Target the main material of a basic shape
					uniqueLinkedObjectMaterials.Add(LinkedObject.GetComponent<MeshRenderer>().material);
					break;
				case LayerType.OBJMODEL:
					//Get all the nested materials in this OBJ
					GetUniqueNestedMaterials();
					break;
			}

			UpdateLayerPrimaryColor();
			GetResetColorValues();
		}

		private void AddTwoWayConnectionToLinkedObject()
		{
			LinkedObject.AddComponent<InterfaceLayerLinkedObject>().InterfaceLayer = this;
		}

		/// <summary>
		/// Sets the layer primary color that will appear on the layer visuals button.
		/// The first material color in the linked GameObject will be used.
		/// </summary>
		public void UpdateLayerPrimaryColor()
		{
			if (uniqueLinkedObjectMaterials.Count > 0)
			{
				var primaryColor = uniqueLinkedObjectMaterials[0].GetColor("_BaseColor");
				primaryColor.a = 1.0f;
				layerLabelColor.color = primaryColor;
			}
		}

		/// <summary>
		/// Fetch all the nested Materials found in renderers within the linked GameObject.
		/// </summary>
		public void GetUniqueNestedMaterials()
		{
			uniqueLinkedObjectMaterials = new List<Material>();

			Renderer[] linkedObjectRenderers = LinkedObject.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer renderer in linkedObjectRenderers)
			{
				foreach (Material sharedMaterial in renderer.sharedMaterials)
				{
					if (!uniqueLinkedObjectMaterials.Contains(sharedMaterial) && !sharedMaterial.name.Contains("Outline"))
					{
						uniqueLinkedObjectMaterials.Add(sharedMaterial);
					}
				}
			}
		}

		public Material GetMaterialFromSlot(int slotId)
		{
			return uniqueLinkedObjectMaterials[slotId];
		}

		/// <summary>
		/// Enable or Disable the linked GameObject
		/// </summary>
		/// <param name="isOn"></param>
		public void ToggleLinkedObject(bool isOn)
		{
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

		/// <summary>
		/// Opens if closed, closes if opened.
		/// </summary>
		public void ToggleLayerOpened()
		{
			expanded = !expanded;

			if(hasOptions)
				ExpandLayerOptions(expanded);

			if(expanded)
			{
				//In case of own models,make sure to always grab the latest materials
				if (layerType == LayerType.OBJMODEL)
				{
					GetUniqueNestedMaterials();
					GetResetColorValues();
					UpdateLayerPrimaryColor();
				}

				//If we do not use any custom options for this layer, use the default layer visuals panel
				if(hasOptions && !customOptions)
					parentInterfaceLayers.LayerVisuals.OpenWithOptionsForLayer(this);
			}
			else{
				parentInterfaceLayers.LayerVisuals.Close();
			}
		}

		/// <summary>
		/// Should these layer options be expanded
		/// </summary>
		/// <param name="expandLayer">Expanded or closed</param>
		public void ExpandLayerOptions(bool expandLayer = true)
		{
			expanded = expandLayer;

			if (customOptions)
			{
				customOptions.SetActive(expandLayer);
				customOptions.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
			}

			if (expandIcon)
				expandIcon.rectTransform.eulerAngles = new Vector3(0, 0, (expanded) ? -90 : 0); //Rotate chevron icon

			if (expanded && autoCloseNeighbourLayers)
			{
				var neighbourLayers = this.transform.parent.GetComponentsInChildren<InterfaceLayer>();
				foreach(var layer in neighbourLayers)
				{
					if (layer != this)
						layer.ExpandLayerOptions(false);
				}
			}
		}

        public void RenameLayer(string newName){
            name = newName; //use our object name to store our full name

            if (newName.Length > maxNameLength)
                newName = newName.Substring(0, maxNameLength - 3) + "...";

            layerNameText.text = newName;
        }
	}
}