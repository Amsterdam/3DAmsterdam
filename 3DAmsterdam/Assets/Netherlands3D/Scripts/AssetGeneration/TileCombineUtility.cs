using Netherlands3D.Core;
using Netherlands3D.TileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Netherlands3D.AssetGeneration
{
	public class TileCombineUtility
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
#if UNITY_EDITOR
			CreateAssetFolder();

			MeshFilter[] meshFilters = sourceGameobject.GetComponentsInChildren<MeshFilter>();
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];

			//Construct the seperate metadata containing the seperation of the buildings
			ObjectMappingClass subObjectsMetaData = ScriptableObject.CreateInstance<ObjectMappingClass>();
			subObjectsMetaData.ids = new List<string>();
			foreach (var meshFilter in meshFilters)
			{
				subObjectsMetaData.ids.Add(meshFilter.gameObject.name);
			}
			var textureSize = ObjectIDMapping.GetTextureSize(subObjectsMetaData.ids.Count);
			List<Vector2> allObjectUVs = new List<Vector2>();
			List<int> allVectorMapIndices = new List<int>();
			subObjectsMetaData.uvs = allObjectUVs.ToArray();

			//Generate the combined tile mesh
			sourceGameobject.transform.position = Vector3.zero;

			string assetFileName = unityMeshAssetFolder + sourceGameobject.name + ".asset";
			string assetMetaDataFileName = unityMeshAssetFolder + sourceGameobject.name + "-data.asset";

			var totalVertexCount = 0;
			for (int i = 0; i < combine.Length; i++)
			{
				combine[i].transform = sourceGameobject.transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
				Mesh buildingMesh = meshFilters[i].sharedMesh;

				if (buildingMesh == null) continue;

				totalVertexCount += buildingMesh.vertexCount;
				//Create UVS
				var objectIdMappingUV = ObjectIDMapping.GetUV(i, textureSize);
				for (int v = 0; v < buildingMesh.vertexCount; v++)
				{
					//UV count should match vert count
					allObjectUVs.Add(objectIdMappingUV);
					//Create vector map reference for vert
					allVectorMapIndices.Add(i);
				}

				combine[i].mesh = buildingMesh;
				meshFilters[i].gameObject.SetActive(false);
			}
			//Now add all the combined uvs to our metadata
			subObjectsMetaData.uvs = allObjectUVs.ToArray();
			subObjectsMetaData.vectorMap = allVectorMapIndices;

			Mesh newCombinedMesh = new Mesh();
			if (totalVertexCount > Mathf.Pow(2, 16))
				newCombinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			if (meshFilters.Length > 0)
			{
				newCombinedMesh.name = sourceGameobject.name;
				newCombinedMesh.CombineMeshes(combine, true, true, false);
				newCombinedMesh.RecalculateNormals();

				//And clean up memory
				for (int i = 0; i < combine.Length; i++)
				{
					if (!AssetDatabase.Contains(meshFilters[i].sharedMesh))
					{
						//Destroy child mesh ( if it is not an asset from our database )
						MonoBehaviour.DestroyImmediate(meshFilters[i].sharedMesh, true);
					}
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

			if (writeAsAssetFile)
			{
				AssetDatabase.CreateAsset(newCombinedMesh, assetFileName);
				AssetDatabase.CreateAsset(subObjectsMetaData, assetMetaDataFileName);
				AssetDatabase.SaveAssets();
			}
#endif
		}
		public static void CreateAssetFolder()
		{
			Directory.CreateDirectory($"{Application.dataPath}/../{unityMeshAssetFolder}");
		}

		public static bool IsAnyVertexWithinConfigBounds(Vector3[] allVertices)
		{
			return IsAnyVertexWithinBounds(allVertices,Config.activeConfiguration.BottomLeftRD, Config.activeConfiguration.TopRightRD);
		}

		public static bool IsAnyVertexWithinBounds(Vector3[] allVertices, Vector2RD minBoundingBox, Vector2RD maxBoundingBox)
		{
			var unityBottomLeft = CoordConvert.RDtoUnity(minBoundingBox);
			var unityTopRight = CoordConvert.RDtoUnity(maxBoundingBox);

			return allVertices.Any(
				vert => (
					vert.x >= unityBottomLeft.x &&
					vert.x < unityTopRight.x && 
					vert.z >= unityBottomLeft.z &&
					vert.z < unityTopRight.z
				)
			);
		}
	}
}