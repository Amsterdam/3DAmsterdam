using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSelection : MonoBehaviour
{
	[SerializeField]
	private Material gridMaterial;

	[SerializeField]
	private float gridSize = 100.0f; //Meter

	[SerializeField]
	private float gridPlaneSize = 10000.0f;

    void Awake()
    {
		this.transform.position = new Vector3(0, Constants.ZERO_GROUND_LEVEL_Y, 0);
		SetGridSize();
    }

	private void SetGridSize()
	{
		gridMaterial.SetTextureScale("_MainTex", Vector2.one * (gridPlaneSize / gridSize));
	}

	 
	public void OnValidate()
	{
		SetGridSize();
	}

}
