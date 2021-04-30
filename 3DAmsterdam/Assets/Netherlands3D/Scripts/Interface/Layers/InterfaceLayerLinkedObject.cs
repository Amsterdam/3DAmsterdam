using Netherlands3D.Interface.Layers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceLayerLinkedObject : MonoBehaviour
{
	public InterfaceLayer InterfaceLayer;

	private void OnEnable()
	{
		if(InterfaceLayer)
			InterfaceLayer.ToggleLinkedObject(true);
	}

	private void OnDisable()
	{
		if (InterfaceLayer)
			InterfaceLayer.ToggleLinkedObject(false);
	}
}
