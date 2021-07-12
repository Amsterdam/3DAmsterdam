using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Netherlands3D.AssetGeneration
{
	public class AssetBundleExport
	{
		public const string assetsFolder = "/GeneratedTileAssets/";	//Relative to project Assets folder
		public const string assetBundlesFolder = "/TileAssetBundles/";	//Relative to project root folder
		public const string metaDataSubstring = "-data";

		/// <summary>
		/// Converts all the data files found in the GeneratedTileAssets folder to AssetBundles
		/// </summary>
		[MenuItem("Netherlands 3D/Tools/Export .asset files to AssetBundles")]
		public static void GenerateAssetBundles()
		{
			DirectoryInfo directory = new DirectoryInfo(Application.dataPath + assetsFolder);
			var fileInfo = directory.GetFiles();

			Directory.CreateDirectory(Application.dataPath + "/.." + assetBundlesFolder);

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
			BuildPipeline.BuildAssetBundles("TileAssetBundles", buildMap.ToArray(), BuildAssetBundleOptions.None, BuildTarget.WebGL);
			Debug.Log("Done exporting tile assets to AssetBundles");
		}
	}
}