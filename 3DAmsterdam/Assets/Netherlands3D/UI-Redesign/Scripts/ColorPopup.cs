using Netherlands3D.Events;
using Netherlands3D.Interface.Coloring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Layers
{
	public class ColorPopup : MonoBehaviour
	{
		[Header("Listening to")]
		[SerializeField]
		private GameObjectEvent openWithTargetLayer;
		[SerializeField]
		private MaterialEvent selectedMaterialEvent;

		[Header("Invoking")]
		[SerializeField]
		private ColorEvent sendColorPicker;


		[SerializeField]
		private ColorPicker colorPicker;

		[SerializeField]
		private HexColorField hexColorField;

		[SerializeField]
		private Slider opacitySlider;

		private void Awake()
		{
			colorPicker.selectedNewColor += ChangeMaterialColor;
			hexColorField.selectedNewColor += ChangeMaterialColor;

			selectedMaterialEvent.AddListenerStarted(OpenMaterialOptions);
		}

		private void OpenMaterialOptions(Material selectedMaterial)
		{
			colorPicker.ChangeColorInput(selectedMaterial.GetColor("_BaseColor"));

		}
		private void ChangeMaterialColor(Color pickedColor, ColorSelector selector)
		{
			sendColorPicker.InvokeStarted(pickedColor);

			//colorPicker.ChangeColorInput(pickedColor);
			//hexColorField.ChangeColorInput(pickedColor);
		}

	}
}