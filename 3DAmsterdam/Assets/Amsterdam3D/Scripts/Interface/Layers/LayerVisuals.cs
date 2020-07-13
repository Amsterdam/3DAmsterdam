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
		private RectTransform materialSlotsContainer;

		private InterfaceLayer targetLayer;
		private List<MaterialSlot> selectedMaterialSlots;

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
			if (selectedMaterialSlots.Count < 1) return;

			foreach(MaterialSlot materialSlot in selectedMaterialSlots)
			{
				materialSlot.ChangeColor(pickedColor);
				if (materialSlot.transform.GetSiblingIndex() == 0)
				{
					targetLayer.UpdateLayerPrimaryColor();
				}
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
		/// Select the first one if none is selected yet.
		/// </summary>
		private void GenerateMaterialSlots()
		{
			ClearMaterialSlots();

			foreach (Material uniqueMaterial in targetLayer.UniqueLinkedObjectMaterials)
			{
				MaterialSlot newMaterialSlot = Instantiate(materialSlotPrefab, materialSlotsContainer);
				newMaterialSlot.Init(uniqueMaterial, this);

				if (selectedMaterialSlots.Count < 1) SelectMaterialSlot(newMaterialSlot);
			}
		}

		/// <summary>
		/// Clear all the current material slots.
		/// </summary>
		private void ClearMaterialSlots()
		{
			ClearMaterialSlotsSelection();

			foreach (Transform materialSlot in materialSlotsContainer)
			{
				Destroy(materialSlot.gameObject);
			}
		}

		/// <summary>
		/// Clears our multiselection list or create it if it doesnt exist yet
		/// </summary>
		private void ClearMaterialSlotsSelection()
		{
			if (selectedMaterialSlots == null)
			{
				selectedMaterialSlots = new List<MaterialSlot>();
			}
			else
			{
				selectedMaterialSlots.Clear();
			}
		}

		/// <summary>
		/// Selection of a specific material slot. Only clear materialslots list if we dont do multiselect
		/// </summary>
		/// <param name="selectedMaterialSlot">Selected material slot</param>
		public void SelectMaterialSlot(MaterialSlot selectedMaterialSlot, bool multiSelect = false)
		{
			if (!multiSelect)
			{
				selectedMaterialSlots.Clear();
				//If we are not multiselecting, make sure we only select this one
				var materialSlots = materialSlotsContainer.GetComponentsInChildren<MaterialSlot>();
				foreach (MaterialSlot slot in materialSlots)
					slot.Selected = (slot == selectedMaterialSlot) ? true : false;
			}
			selectedMaterialSlots.Add(selectedMaterialSlot);
			
			if (!multiSelect)
			{
				colorPicker.ChangeColorInput(selectedMaterialSlot.GetColor);
				hexColorField.ChangeColorInput(selectedMaterialSlot.GetColor);
			}
		}
	}
}