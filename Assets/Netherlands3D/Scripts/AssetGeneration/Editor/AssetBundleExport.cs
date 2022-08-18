using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Netherlands3D.AssetGeneration
{
	public class AssetBundleExport
	{
		public const string assetsFolder = "/GeneratedTileAssets/";	
		public const string metaDataSubstring = "-data";

		/// <summary>
		/// Converts all the data files found in the GeneratedTileAssets folder to AssetBundles
		/// </summary>
		[MenuItem("Netherlands 3D/Tools/Export .asset files to AssetBundles")]
		public static void GenerateBuildingsAssetBundles()
		{
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath + assetsFolder);
			var fileInfo = directory.GetFiles();

			List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();
			foreach (var file in fileInfo)
			{
				if (!file.Name.Contains(".meta"))
				{
					//Create asset bundle from mesh
					string[] assetName = new string[1];
					assetName[0] = "Assets/" + assetsFolder + file.Name;

					AssetBundleBuild buildMesh = new AssetBundleBuild();
					buildMesh.assetBundleName = file.Name.Replace(".asset", "");
					buildMesh.assetBundleName = buildMesh.assetBundleName.Replace(".mesh", "");
					buildMesh.assetNames = assetName;
					buildMap.Add(buildMesh);
				}
			}
			BuildPipeline.BuildAssetBundles("BuildingsAssetBundles", buildMap.ToArray(), BuildAssetBundleOptions.None, BuildTarget.WebGL);
			Debug.Log("Done exporting tile assets to AssetBundles");
		}
	}
}