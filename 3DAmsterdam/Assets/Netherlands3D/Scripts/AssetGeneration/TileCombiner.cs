﻿using Netherlands3D.LayerSystem;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Netherlands3D.AssetGeneration
{
	public class TileCombiner
	{
		public static string unityMeshAssetFolder = "Assets/GeneratedTileAssets/";
		[SerializeField] 
		Material DefaultMaterial;
		/// <summary>
		/// Combine all the children of this tile into a single mesh
		/// </summary>
		/// <param name="sourceGameobject">Source gameobject with childrens that need to be combined into one tile mesh</param>
		/// <param name="worldPosition">Original position to move the tile to for previewing it</param>
		public static void CombineSource(GameObject sourceGameobject, Vector3 worldPosition, bool renderInViewport, Material defaultMaterial,bool writeAsAssetFile)
		{
			CreateAssetFolder();

			MeshFilter[] meshFilters = sourceGameobject.GetComponentsInChildren<MeshFilter>();
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];

			//Construct the seperate metadata containing the seperation of the buildings
			ObjectMappingClass buildingMetaData = ScriptableObject.CreateInstance<ObjectMappingClass>();
			buildingMetaData.ids = new List<string>();
			foreach (var meshFilter in meshFilters)
			{
				buildingMetaData.ids.Add(meshFilter.gameObject.name);
			}
			var textureSize = ObjectIDMapping.GetTextureSize(buildingMetaData.ids.Count);
			List<Vector2> allObjectUVs = new List<Vector2>();
			List<int> allVectorMapIndices = new List<int>();
			buildingMetaData.uvs = allObjectUVs.ToArray();

			//Generate the combined tile mesh
			sourceGameobject.transform.position = Vector3.zero;

			string assetFileName = unityMeshAssetFolder + sourceGameobject.name + ".asset";
			string assetMetaDataFileName = unityMeshAssetFolder + sourceGameobject.name + "-data.asset";

			var totalVertexCount = 0;
			for (int i = 0; i < combine.Length; i++)
			{
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
				Mesh buildingMesh = meshFilters[i].sharedMesh;
				totalVertexCount += buildingMesh.vertexCount;
				//Create UVS
				var buildingUV = ObjectIDMapping.GetUV(i, textureSize);
				for (int v = 0; v < buildingMesh.vertexCount; v++)
				{
					//UV count should match vert count
					allObjectUVs.Add(buildingUV);
					//Create vector map reference for vert
					allVectorMapIndices.Add(i);
				}

				combine[i].mesh = buildingMesh;
				meshFilters[i].gameObject.SetActive(false);
			}
			//Now add all the combined uvs to our metadata
			buildingMetaData.uvs = allObjectUVs.ToArray();
			buildingMetaData.vectorMap = allVectorMapIndices;

			Mesh newCombinedMesh = new Mesh();
			if (totalVertexCount > Mathf.Pow(2, 16))
				newCombinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			if (meshFilters.Length > 0)
			{
				newCombinedMesh.name = sourceGameobject.name;
				newCombinedMesh.CombineMeshes(combine, true);
				newCombinedMesh.RecalculateNormals();
				newCombinedMesh.Optimize();

				//And clean up memory
				for (int i = 0; i < combine.Length; i++)
				{
					MonoBehaviour.DestroyImmediate(meshFilters[i].sharedMesh, true);
					MonoBehaviour.Destroy(meshFilters[i].gameObject);
				}
			}
			if (renderInViewport)
			{
				sourceGameobject.AddComponent<MeshFilter>().sharedMesh = newCombinedMesh;
				sourceGameobject.AddComponent<MeshRenderer>().material = defaultMaterial;
				sourceGameobject.transform.position = worldPosition;
			}
			else
			{
				MonoBehaviour.Destroy(sourceGameobject);
			}

#if UNITY_EDITOR
			if (writeAsAssetFile)
			{
				AssetDatabase.CreateAsset(newCombinedMesh, assetFileName);
				AssetDatabase.CreateAsset(buildingMetaData, assetMetaDataFileName);
				AssetDatabase.SaveAssets();
			}
#endif
		}

		private static void CreateAssetFolder()
		{
			Directory.CreateDirectory($"{Application.dataPath}/../{unityMeshAssetFolder}");
		}
	}
}