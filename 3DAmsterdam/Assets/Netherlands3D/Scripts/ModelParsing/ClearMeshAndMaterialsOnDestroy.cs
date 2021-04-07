using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.ModelParsing
{
	public class ClearMeshAndMaterialsOnDestroy : MonoBehaviour
	{
		private void OnDestroy()
		{
			var renderer = GetComponent<MeshRenderer>();
			foreach (var material in renderer.sharedMaterials)
			{
				Destroy(material);
			}

			var meshFilter = GetComponent<MeshFilter>();
			Destroy(meshFilter.sharedMesh);
		}
	}
}