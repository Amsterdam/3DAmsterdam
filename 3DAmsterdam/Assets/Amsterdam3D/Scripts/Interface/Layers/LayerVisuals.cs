using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;
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
		private HexColorField hexColorField;

		[SerializeField]
		private ToggleGroup materialSlotsGroup;
		public ToggleGroup MaterialSlotsGroup { get => materialSlotsGroup; }

		private InterfaceLayer targetLayer;
		private MaterialSlot targetMaterialSlot;

		[SerializeField]
		private Vector2 locationOffset;

		private void Start()
		{
			colorPicker.selectedNewColor += ChangeMaterialColor;
			hexColorField.selectedNewColor += ChangeMaterialColor;
		}

		private void ChangeMaterialColor(Color pickedColor, ColorSelector selector)
		{
			if (!targetMaterialSlot) return;

			targetMaterialSlot.ChangeColor(pickedColor);
			if (targetMaterialSlot.transform.GetSiblingIndex() == 0)
			{
				targetLayer.UpdateLayerPrimaryColor();
			}

			if(selector == colorPicker)
			{
				hexColorField.ChangeColorInput(pickedColor);
			}
			else if(selector == hexColorField)
			{
				colorPicker.ChangeColorInput(pickedColor);
			}
		}

		public void OpenWithOptionsForLayer(InterfaceLayer interfaceLayer)
		{
			this.GetComponent<RectTransform>().anchoredPosition = interfaceLayer.GetComponent<RectTransform>().anchoredPosition + locationOffset;
			colorPicker.CalculateHitArea();
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
				MaterialSlot newMaterialSlot = Instantiate(materialSlotPrefab, MaterialSlotsGroup.transform);
				newMaterialSlot.Init(uniqueMaterial, this);

				if (!targetMaterialSlot) SelectMaterialSlot(newMaterialSlot);
			}
		}

		public void SelectMaterialSlot(MaterialSlot materialSlot)
		{
			targetMaterialSlot = materialSlot;
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