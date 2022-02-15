using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EditorWebRootPath : MonoBehaviour
{
    [SerializeField]
    private string serverRootWebAddress = "https://acc.3d.amsterdam.nl";
#if UNITY_EDITOR
	void Awake()
	{
		TurnAllRelativeIntoFullPaths();
	}

	private void TurnAllRelativeIntoFullPaths()
	{
		Layer[] layers = GetComponentsInChildren<Layer>();
		foreach (var layer in layers)
		{
			foreach (var dataset in layer.Datasets)
			{
				if (dataset.path.StartsWith("/"))
				{
					//Turn relative paths into full path
					dataset.path = serverRootWebAddress + dataset.path;
				}
			}
		}
	}
#endif
}
