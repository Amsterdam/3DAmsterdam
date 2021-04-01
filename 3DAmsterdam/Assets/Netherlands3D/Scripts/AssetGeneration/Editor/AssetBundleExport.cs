using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Netherlands3D.AssetGeneration
{
	public class AssetBundleExport
	{
		public const string buildingsAssetBundleFolder = "/3DAmsterdam/BuildingTileAssets";	
		public const string treesAssetBundleFolder = "/3DAmsterdam/TreeTileAssets";	
		public const string terrainAssetBundleFolder = "/3DAmsterdam/TerrainTileAssets";

		public const string metaDataSubstring = "-data";

		/// <summary>
		/// Converts all the data files found in the TreeTiles folder to AssetBundles
		/// </summary>
		[MenuItem("3D Amsterdam/Tools/Exporteer buildings to AssetBundles")]
		public static void GenerateBuildingsAssetBundles()
		{
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath + buildingsAssetBundleFolder);
			var fileInfo = directory.GetFiles();

			List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();
			foreach (var file in fileInfo)
			{
				if (!file.Name.Contains(".meta"))
				{
					//Create asset bundle from mesh
					string[] assetName = new string[1];
					assetName[0] = "Assets/" + buildingsAssetBundleFolder + file.Name;

					if(file.Name.Contains(metaDataSubstring)){
						AssetBundleBuild buildMetaData = new AssetBundleBuild();
						buildMetaData.assetBundleName = file.Name.Replace(".asset", "");
						buildMetaData.assetNames = assetName;
						buildMap.Add(buildMetaData);
					}
					else{
						AssetBundleBuild buildMesh = new AssetBundleBuild();
						buildMesh.assetBundleName = file.Name.Replace(".asset", "");
						buildMesh.assetNames = assetName;
						buildMap.Add(buildMesh);
					}
				}
			}
			BuildPipeline.BuildAssetBundles("BuildingsAssetBundles", buildMap.ToArray(), BuildAssetBundleOptions.None, BuildTarget.WebGL);
			Debug.Log("Done exporting Tree tile AssetBundles");
		}

		/// <summary>
		/// Converts all the data files found in the TreeTiles folder to AssetBundles
		/// </summary>
		[MenuItem("3D Amsterdam/Tools/Exporteer trees to AssetBundles")]
		public static void GenerateTreesAssetBundles()
		{
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath + treesAssetBundleFolder);
			var fileInfo = directory.GetFiles();

			foreach (var file in fileInfo)
			{
				if (!file.Name.Contains(".meta"))
				{
					//Create asset bundle from mesh we just made
					AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
					string[] assetNames = new string[1];
					assetNames[0] = "Assets/" + treesAssetBundleFolder + file.Name;

					buildMap[0].assetBundleName = file.Name.Replace(".asset", "");
					buildMap[0].assetNames = assetNames;

					BuildPipeline.BuildAssetBundles("TreesAssetBundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.WebGL);
				}
			}

			Debug.Log("Done exporting Tree tile AssetBundles");
		}

		/// <summary>
		/// Converts all the data files found in the TreeTiles folder to AssetBundles
		/// </summary>
		[MenuItem("3D Amsterdam/Tools/Exporteer terrain to AssetBundles")]
		public static void GenerateTerrainAssetBundles()
		{
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/terrainMeshes/LOD0/");
			var fileInfo = directory.GetFiles();

			foreach (var file in fileInfo)
			{
				if (!file.Name.Contains(".meta"))
				{
					//Create asset bundle from mesh we just made
					AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
					string[] assetNames = new string[1];
					assetNames[0] = "Assets/terrainMeshes/LOD0/" + file.Name;

					buildMap[0].assetBundleName = file.Name.Replace(".mesh", "");
					buildMap[0].assetNames = assetNames;

					BuildPipeline.BuildAssetBundles("TerrainAssetBundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.WebGL);
				}
			}

			Debug.Log("Done exporting Tree tile AssetBundles");
		}
	}
}