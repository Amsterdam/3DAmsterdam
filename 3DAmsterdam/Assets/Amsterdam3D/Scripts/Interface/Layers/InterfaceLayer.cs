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
		protected GameObject linkedObject;

		private LayerVisuals layerVisuals;

		private void Start()
		{
			
		}

		public void ToggleLinkedObject(bool isOn)
		{
			linkedObject.SetActive(isOn);
		}

		public void 
	}
}