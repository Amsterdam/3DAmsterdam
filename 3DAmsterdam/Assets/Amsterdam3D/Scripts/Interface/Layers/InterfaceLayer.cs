using UnityEngine;
using UnityEngine.UI;

namespace Amsterdam3D.Interface
{
	public enum CustomLayerType
	{
		BASICSHAPE,
		OBJMODEL,
		STATIC
	}
	
	public class InterfaceLayer : MonoBehaviour
	{
		[SerializeField]
		protected CustomLayerType layerType = CustomLayerType.STATIC;

		[SerializeField]
		private GameObject linkedObject;
		public GameObject LinkedObject { get => linkedObject; set => linkedObject = value; }

		[SerializeField]
		protected InterfaceLayers parentInterfaceLayers;

		public void ToggleLinkedObject(bool isOn)
		{
			LinkedObject.SetActive(isOn);
		}

		public void OpenLayerVisualOptions(){
			Debug.Log("Open layer visual buttons", this);
			parentInterfaceLayers.LayerVisuals.OpenWithOptionsForLayer(this);
		}
	}
}