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

		/// <summary>
		/// Change the color, and update all the color selectors to match the same color.
		/// </summary>
		/// <param name="pickedColor">The color to change to</param>
		/// <param name="selector">The selector we used to change the color</param>
		private void ChangeMaterialColor(Color pickedColor, ColorSelector selector)
		{
			if (!targetMaterialSlot) return;

			targetMaterialSlot.ChangeColor(pickedColor);
			if (targetMaterialSlot.transform.GetSiblingIndex() == 0)
			{
				targetLayer.UpdateLayerPrimaryColor();
			}

			//Match all selector colors
			if (selector == colorPicker)
			{
				hexColorField.ChangeColorInput(pickedColor);
			}
			else if (selector == hexColorField)
			{
				colorPicker.ChangeColorInput(pickedColor);
			}
		}

		/// <summary>
		/// Open this layer visuals panel with the options for this target interface layer.
		/// </summary>
		/// <param name="interfaceLayer">Target interface layer</param>
		public void OpenWithOptionsForLayer(InterfaceLayer interfaceLayer)
		{
			this.GetComponent<RectTransform>().anchoredPosition = interfaceLayer.GetComponent<RectTransform>().anchoredPosition + locationOffset;
			
			targetLayer = interfaceLayer;
			GenerateMaterialSlots();
			gameObject.SetActive(true);
		}

		/// <summary>
		/// Closes the dialog GameObject
		/// </summary>
		public void Close()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Generate all the material slots for this layer.
		/// </summary>
		private void GenerateMaterialSlots()
		{
			ClearMaterialSlots();
			
			foreach (Material uniqueMaterial in targetLayer.UniqueLinkedObjectMaterials)
			{
				MaterialSlot newMaterialSlot = Instantiate(materialSlotPrefab, MaterialSlotsGroup.transform);
				newMaterialSlot.Init(uniqueMaterial, this);

				if (!targetMaterialSlot) SelectMaterialSlot(newMaterialSlot);
			}
		}

		/// <summary>
		/// Clear all the current material slots.
		/// </summary>
		private void ClearMaterialSlots()
		{
			foreach (Transform materialSlot in MaterialSlotsGroup.transform)
			{
				Destroy(materialSlot.gameObject);
			}
		}

		/// <summary>
		/// Selection of a specific material slot.
		/// </summary>
		/// <param name="materialSlot">Selected material slot</param>
		public void SelectMaterialSlot(MaterialSlot materialSlot)
		{
			targetMaterialSlot = materialSlot;
			colorPicker.ChangeColorInput(targetMaterialSlot.GetColor);
			hexColorField.ChangeColorInput(targetMaterialSlot.GetColor);
		}
	}
}