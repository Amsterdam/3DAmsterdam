using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public enum CustomLayerType
	{
		BASICSHAPE,
		OBJMODEL,
		STATIC
	}
	
	public class InterfaceLayer : MonoBehaviour
	{
		[SerializeField]
		protected CustomLayerType layerType = CustomLayerType.STATIC;

		[SerializeField]
		protected GameObject linkedObject;

		[SerializeField]
		private List<Material> uniqueLinkedObjectMaterials;
		public List<Material> UniqueLinkedObjectMaterials { get => uniqueLinkedObjectMaterials; set => uniqueLinkedObjectMaterials = value; }

		[SerializeField]
		protected InterfaceLayers parentInterfaceLayers;

		[SerializeField]
		private bool combinedMaterialSlots = false;

		[SerializeField]
		private Image visualOptionsButton;

		private void Start()
		{
			if (linkedObject){
				UpdateLayerPrimaryColor();
			}
		}

		public void LinkObject(GameObject newLinkedObject){
			linkedObject = newLinkedObject;
			GetUniqueNestedMaterials();
			UpdateLayerPrimaryColor();
		}

		public void UpdateLayerPrimaryColor()
		{
			if (uniqueLinkedObjectMaterials.Count > 0)
				visualOptionsButton.color = uniqueLinkedObjectMaterials[0].color;
		}

		private void GetUniqueNestedMaterials(){
			print("Get Layer materials");
			uniqueLinkedObjectMaterials = new List<Material>();
			Renderer[] linkedObjectRenderers = linkedObject.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer renderer in linkedObjectRenderers)
			{
				print("Renderer");
				foreach (Material sharedMaterial in renderer.sharedMaterials)
				{
					if (!uniqueLinkedObjectMaterials.Contains(sharedMaterial))
					{
						Debug.Log("material: ",sharedMaterial);
						uniqueLinkedObjectMaterials.Add(sharedMaterial);
					}
				}
			}
		}

		public void ToggleLinkedObject(bool isOn)
		{
			linkedObject.SetActive(isOn);
		}

		public void OpenLayerVisualOptions(){
			Debug.Log("Open layer visual buttons", this);
			parentInterfaceLayers.LayerVisuals.OpenWithOptionsForLayer(this);
		}
	}
}