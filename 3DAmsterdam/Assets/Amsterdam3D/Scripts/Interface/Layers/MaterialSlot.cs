using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class MaterialSlot : MonoBehaviour, IPointerClickHandler
	{
		private Material targetMaterial;
		private LayerVisuals layerVisuals;

		private bool selected = false;
		public bool Selected {
			get
			{
				return selected;
			}
			set
			{
				selected = value;
				selectedImage.enabled = selected;
			} 
		}

		[SerializeField]
		private Image selectedImage;
		[SerializeField]
		private Image colorImage;
		public Color GetColor => targetMaterial.GetColor("_BaseColor");

		private void Start()
		{
			Selected = selected; //start unselected
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Select();
		}

		private void Select()
		{
			var multiSelect = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			Selected = (multiSelect) ? !Selected : true;

			layerVisuals.SelectMaterialSlot(this, multiSelect);

			Debug.Log("Selected material " + targetMaterial.name);
		}

		public void Init(Material target, LayerVisuals targetLayerVisuals)
		{
			targetMaterial = target;
			var targetMaterialColor = GetColor;
			colorImage.color = new Color(targetMaterialColor.r, targetMaterialColor.g, targetMaterialColor.b,1.0f);

			layerVisuals = targetLayerVisuals;
		}

		public void ChangeColor(Color pickedColor)
		{
			colorImage.color = pickedColor;
			targetMaterial.SetColor("_BaseColor",pickedColor);
		}
	}
}