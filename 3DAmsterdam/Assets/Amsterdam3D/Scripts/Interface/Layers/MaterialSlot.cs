using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class MaterialSlot : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField]
		private Material transparentMaterialSource;
		[SerializeField]
		private Material opaqueMaterialSource;

		private Material targetMaterial;
		private LayerVisuals layerVisuals;

		private bool selected = false;

		public float materialOpacity = 1.0f;

		public bool Selected
		{
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

		public void Init(Material target, LayerVisuals targetLayerVisuals)
		{
			targetMaterial = target;

			var targetMaterialColor = GetColor;
			
			colorImage.color = new Color(targetMaterialColor.r, targetMaterialColor.g, targetMaterialColor.b, 1.0f);
			materialOpacity = targetMaterialColor.a;

			layerVisuals = targetLayerVisuals;
		}

		private void Select()
		{
			var multiSelect = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			Selected = (multiSelect) ? !Selected : true;

			layerVisuals.SelectMaterialSlot(this, multiSelect);

			Debug.Log("Selected material " + targetMaterial.name);
		}

		public void ChangeColor(Color pickedColor)
		{
			colorImage.color = pickedColor;
			targetMaterial.SetColor("_BaseColor", new Color(pickedColor.r, pickedColor.g, pickedColor.b, materialOpacity));
		}

		public void ChangeOpacity(float opacity)
		{
			if(materialOpacity == opacity)
			{
				return;
			}
			else if (opacity < 1.0f)
			{
				targetMaterial.CopyPropertiesFromMaterial(transparentMaterialSource);
				var color = targetMaterial.GetColor("_BaseColor");
				color.a = materialOpacity;
				targetMaterial.SetFloat("_Surface", 1); //1 Alpha
				targetMaterial.SetColor("_BaseColor", color);
			}
			else {
				targetMaterial.CopyPropertiesFromMaterial(opaqueMaterialSource);
				targetMaterial.SetFloat("_Surface", 0); //0 Opaque
			}

			materialOpacity = opacity;
		}
	}
}