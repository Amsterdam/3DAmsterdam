using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public enum LayerType
	{
		BASICSHAPE,
		OBJMODEL,
		STATIC,
		DYNAMIC_MATERIALS
	}
	
	public class InterfaceLayer : MonoBehaviour
	{
		[SerializeField]
		protected LayerType layerType = LayerType.STATIC;
		public LayerType LayerType { get => layerType; }

		private bool active = true;
		public bool Active { get => active; set => active = value; }

		[SerializeField]
		protected GameObject linkedObject;

		[SerializeField]
		private List<Material> uniqueLinkedObjectMaterials;
		public List<Material> UniqueLinkedObjectMaterials { get => uniqueLinkedObjectMaterials; set => uniqueLinkedObjectMaterials = value; }
		
		[SerializeField]
		protected InterfaceLayers parentInterfaceLayers;

		[SerializeField]
		private Image visualOptionsButton;
		private void Start()
		{
			//If we set a linkedObject manualy, get the color.
			if (linkedObject){
				UpdateLayerPrimaryColor();
			}
		}

		/// <summary>
		/// Create the connection between this 2D layer, and a 3D GameObject.
		/// Materials will be fetched according to the layer type.
		/// </summary>
		/// <param name="newLinkedObject">The GameObject to be linked</param>
		public void LinkObject(GameObject newLinkedObject){
			linkedObject = newLinkedObject;

			switch(layerType)
			{
				case LayerType.BASICSHAPE:
					//Create a clone so we can change this specific material
					uniqueLinkedObjectMaterials.Add(linkedObject.GetComponent<MeshRenderer>().material);
					break;
				case LayerType.OBJMODEL:
					//Get all the nested materials in this OBJ
					GetUniqueNestedMaterials();
					break;
			}

			UpdateLayerPrimaryColor();
		}

		/// <summary>
		/// Sets the layer primary color that will appear on the layer visuals button.
		/// The first material color in the linked GameObject will be used.
		/// </summary>
		public void UpdateLayerPrimaryColor()
		{
			if (uniqueLinkedObjectMaterials.Count > 0)
				visualOptionsButton.color = uniqueLinkedObjectMaterials[0].GetColor("_BaseColor");
		}

		/// <summary>
		/// Fetch all the nested Materials found in renderers within the linked GameObject.
		/// </summary>
		public void GetUniqueNestedMaterials(){
			uniqueLinkedObjectMaterials = new List<Material>();

			Renderer[] linkedObjectRenderers = linkedObject.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer renderer in linkedObjectRenderers)
			{
				foreach (Material sharedMaterial in renderer.sharedMaterials)
				{
					if (!uniqueLinkedObjectMaterials.Contains(sharedMaterial))
					{
						uniqueLinkedObjectMaterials.Add(sharedMaterial);
					}
				}
			}
		}

		/// <summary>
		/// Enable or Disable the linked GameObject
		/// </summary>
		/// <param name="isOn"></param>
		public void ToggleLinkedObject(bool isOn)
		{
			active = isOn;
			linkedObject.SetActive(active);
		}

		/// <summary>
		/// Opens the layer visuals panel ( with color picker a.o. ) targeting this layer.
		/// </summary>
		public void OpenLayerVisualOptions(){
			Debug.Log("Open layer visual buttons", this);
			parentInterfaceLayers.LayerVisuals.OpenWithOptionsForLayer(this);
		}
	}
}