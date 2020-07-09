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
		public Color GetColor => targetMaterial.GetColor("_BaseColor");

		public void Select()
		{
			layerVisuals.SelectMaterialSlot(this);
			Debug.Log("Selected material " + targetMaterial.name);
		}

		public void Init(Material target, LayerVisuals targetLayerVisuals)
		{
			targetMaterial = target;
			var targetMaterialColor = GetColor;
			colorImage.color = new Color(targetMaterialColor.r, targetMaterialColor.g, targetMaterialColor.b,1.0f);

			layerVisuals = targetLayerVisuals;
			GetComponent<Toggle>().group = targetLayerVisuals.MaterialSlotsGroup;
		}

		public void ChangeColor(Color pickedColor)
		{
			colorImage.color = pickedColor;
			targetMaterial.SetColor("_BaseColor",pickedColor);
		}
	}
}