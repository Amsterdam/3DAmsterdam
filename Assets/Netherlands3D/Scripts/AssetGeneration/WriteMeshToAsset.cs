#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WriteMeshToAsset : MonoBehaviour
{
	private string customMeshFolder = "Assets/3DAmsterdam/Models/DetailedBuildings/";
	
	void Start()
	{
		MeshToAsset(this.GetComponent<MeshFilter>().mesh);
	}

	private void MeshToAsset(Mesh targetMesh)
	{
		
		string assetFileName = customMeshFolder + this.name + ".asset";
		AssetDatabase.CreateAsset(targetMesh, assetFileName);
		AssetDatabase.SaveAssets();
		
	}
}
#endif