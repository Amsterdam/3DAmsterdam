﻿using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Netherlands3D.AssetGeneration
{
	public class ExportTreeTiles
	{
		/// <summary>
		/// Converts all the data files found in the TreeTiles folder to AssetBundles
		/// </summary>
		[MenuItem("Tools/Exporteer Bomen naar AssetBundles")]
		public static void GenerateAssetBundles()
		{
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/TreeTileAssets");
			var fileInfo = directory.GetFiles();

			foreach (var file in fileInfo)
			{
				if (!file.Name.Contains(".meta") && !File.ReadAllText(file.FullName).Contains("vertexCount: 0"))
				{
					//Create asset bundle from mesh we just made
					AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
					string[] assetNames = new string[1];
					assetNames[0] = "Assets/TreeTileAssets/" + file.Name;

					buildMap[0].assetBundleName = file.Name.Replace(".asset", "");
					buildMap[0].assetNames = assetNames;

					BuildPipeline.BuildAssetBundles("TreeTileAssetBundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.WebGL);
				}
			}

			Debug.Log("Done exporting Tree tile AssetBundles");
		}

		/// <summary>
		/// Converts all the data files found in the TreeTiles folder to AssetBundles
		/// </summary>
		[MenuItem("Tools/Exporteer terein naar AssetBundles")]
		public static void GenerateTerrainAssetBundles()
		{
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/terrainMeshes/LOD0/");
			var fileInfo = directory.GetFiles();

			foreach (var file in fileInfo)
			{
				if (!file.Name.Contains(".meta") && !File.ReadAllText(file.FullName).Contains("vertexCount: 0"))
				{
					//Create asset bundle from mesh we just made
					AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
					string[] assetNames = new string[1];
					assetNames[0] = "Assets/terrainMeshes/LOD0/" + file.Name;

					buildMap[0].assetBundleName = file.Name.Replace(".mesh", "");
					buildMap[0].assetNames = assetNames;

					BuildPipeline.BuildAssetBundles("Terrain", buildMap, BuildAssetBundleOptions.None, BuildTarget.WebGL);
				}
			}

			Debug.Log("Done exporting Tree tile AssetBundles");
		}
	}
}