using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public class MaterialSlot : MonoBehaviour
	{
		private Material targetMaterial;
		private LayerVisuals layerVisuals;

		public void Select()
		{
			layerVisuals.SelectedMaterialSlot();
			Debug.Log("Selected material " + targetMaterial.name);
		}

		public void SetTargetMaterial(Material target, LayerVisuals targetLayerVisuals)
		{
			targetMaterial = target;
			layerVisuals = targetLayerVisuals;
			GetComponent<Toggle>().group = targetLayerVisuals.MaterialSlotsGroup;
		}
	}
}