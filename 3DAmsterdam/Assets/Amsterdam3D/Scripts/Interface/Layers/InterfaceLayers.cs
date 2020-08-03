using BruTile.Wms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amsterdam3D.Interface
{
	public class InterfaceLayers : MonoBehaviour
	{
		[SerializeField]
		private CustomLayer customObjectLayerPrefab;

		[SerializeField]
		private RectTransform layersContainer;

		private Animator animator;
		private bool toggledVisible = false;

		[SerializeField]
		private LayerVisuals layerVisualsDialog;
		public LayerVisuals LayerVisuals { get => layerVisualsDialog; }

		void Awake()
		{
			animator = GetComponent<Animator>();
		}

		/// <summary>
		/// Toggle the layer panel
		/// </summary>
		/// <param name="visible"></param>
		public void ToggleVisibility(bool visible)
		{
			toggledVisible = visible;
			animator.SetBool("AnimateIn", toggledVisible);
		}

		/// <summary>
		/// Reset all the layers their colors to their starting values
		/// </summary>
		public void ResetAllLayerMaterialColors()
		{
			var interfaceLayers = FindObjectsOfType<InterfaceLayer>();
			foreach (InterfaceLayer layer in interfaceLayers)
			{
				layer.ResetAllColors();
			}
		}

		/// <summary>
		/// Generate a new interface layer, with a linked object
		/// </summary>
		/// <param name="linkedWorldObject">The GameObject that is linked to this interface layer</param>
		/// <param name="type">The layer/object type</param>

		public void AddNewCustomObjectLayer(GameObject linkedWorldObject, LayerType type)
		{
			CustomLayer newCustomlayer = Instantiate(customObjectLayerPrefab, layersContainer);
			newCustomlayer.Create(linkedWorldObject.name, linkedWorldObject, type, this);
			newCustomlayer.transform.SetSiblingIndex(0);
		}
	}
}
