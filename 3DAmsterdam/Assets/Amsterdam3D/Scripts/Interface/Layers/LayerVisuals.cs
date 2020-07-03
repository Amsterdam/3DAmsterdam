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
			if (!targetMaterialSlot) return;

			targetMaterialSlot.ChangeColor(pickedColor);
			if (targetMaterialSlot.transform.GetSiblingIndex() == 0)
			{
				targetLayer.UpdateLayerPrimaryColor();
			}
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

			foreach (Material uniqueMaterial in targetLayer.UniqueLinkedObjectMaterials)
			{
				Instantiate(materialSlotPrefab, MaterialSlotsGroup.transform).Init(uniqueMaterial, this);
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