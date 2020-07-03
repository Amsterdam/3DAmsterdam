using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class LayerVisuals : MonoBehaviour
	{
		[SerializeField]
		private MaterialSlot materialSlotPrefab;

		[SerializeField]
		private ColorPicker colorPicker;

		[SerializeField]
		private ToggleGroup materialSlotsGroup;
		public ToggleGroup MaterialSlotsGroup { get => materialSlotsGroup; }

		private InterfaceLayer targetLayer;
		private MaterialSlot targetMaterialSlot;

		[SerializeField]
		private Vector3 locationOffset;

		private void Start()
		{
			colorPicker.pickedNewColor += ChangeMaterialColor;
		}

		private void ChangeMaterialColor(Color pickedColor)
		{
			targetMaterialSlot?.ChangeColor(pickedColor);
		}

		public void OpenWithOptionsForLayer(InterfaceLayer interfaceLayer)
		{
			this.transform.position = interfaceLayer.transform.position + locationOffset;
			targetLayer = interfaceLayer;
			GenerateTargetLayerMaterialSlot();
			gameObject.SetActive(true);
		}

		public void Close()
		{
			gameObject.SetActive(false);
		}

		private void GenerateTargetLayerMaterialSlot()
		{
			ClearMaterialSlots();

			//Get all the unique materials we can find nested inside our linked GameObject.
			//Then instantiate a new MaterialSlot in our container
			var uniqueMaterials = new List<Material>();
			Renderer[] linkedObjectRenderers = targetLayer.LinkedObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in linkedObjectRenderers)
			{
				foreach (Material sharedMaterial in renderer.sharedMaterials)
				{
					if (!uniqueMaterials.Contains(sharedMaterial))
					{
						uniqueMaterials.Add(sharedMaterial);
						Instantiate(materialSlotPrefab, MaterialSlotsGroup.transform).Init(sharedMaterial, this);
					}
				}
			}
		}

		public void SelectedMaterialSlot()
		{
			//Get the selected MaterialSlot toggle from the ToggleGroup.
			//This allows us to optionally use this logic later for a multiselect of material slots
			targetMaterialSlot = MaterialSlotsGroup.ActiveToggles().FirstOrDefault().GetComponent<MaterialSlot>();
		}

		private void ClearMaterialSlots()
		{
			foreach (Transform materialSlot in MaterialSlotsGroup.transform)
			{
				Destroy(materialSlot.gameObject);
			}
		}
	}
}