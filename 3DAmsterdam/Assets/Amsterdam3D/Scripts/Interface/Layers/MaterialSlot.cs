using System;
using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class MaterialSlot : MonoBehaviour
	{
		private Material targetMaterial;
		private LayerVisuals layerVisuals;

		[SerializeField]
		private Image colorImage;
		public Color GetColor => targetMaterial.color;

		public void Select()
		{
			layerVisuals.SelectMaterialSlot(this);
			Debug.Log("Selected material " + targetMaterial.name);
		}

		public void Init(Material target, LayerVisuals targetLayerVisuals)
		{
			targetMaterial = target;
			colorImage.color = new Color(targetMaterial.color.r, targetMaterial.color.g, targetMaterial.color.b,1.0f);

			layerVisuals = targetLayerVisuals;
			GetComponent<Toggle>().group = targetLayerVisuals.MaterialSlotsGroup;
		}

		public void ChangeColor(Color pickedColor)
		{
			colorImage.color = pickedColor;
			targetMaterial.color = pickedColor;
		}
	}
}