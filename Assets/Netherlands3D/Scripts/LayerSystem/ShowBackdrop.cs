using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.LayerSystem
{
	public class ShowBackdrop : MonoBehaviour
	{
		[SerializeField]
		private GameObject backdropObject;

		private Layer layerSystemLayer;

		private void Awake()
		{
			layerSystemLayer = GetComponent<Layer>();
			layerSystemLayer.onLayerEnabled.AddListener(EnableBackdrop);
			layerSystemLayer.onLayerDisabled.AddListener(DisableBackdropIfNotInUse);
		}

		private void EnableBackdrop()
		{
			backdropObject.SetActive(true);
		}
		private void DisableBackdropIfNotInUse()
		{
			var layers = FindObjectsOfType<Layer>();
			foreach(var layer in layers)
			{
				//Do not disable if there is anther active layer that uses the same backdrop
				if(layer != layerSystemLayer && layer.isEnabled && layer.GetComponent<ShowBackdrop>()?.backdropObject == backdropObject)
				{
					return;
				}
			}
			backdropObject.SetActive(false);
		}
	}
}