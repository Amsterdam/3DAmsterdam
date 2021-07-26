using Netherlands3D.Interface.Layers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateLayersOnEnable : MonoBehaviour
{
    [SerializeField]
    private InterfaceLayer[] activateLayers;
	private void OnEnable()
	{
		foreach (var layer in activateLayers) layer.Active = true;
	}
}
