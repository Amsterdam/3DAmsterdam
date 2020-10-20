using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportTreeTiles
{
	[MenuItem("Tools/Genereer Bomen naar AssetBundles")]
	// Start is called before the first frame update
	public static void GenerateAssetBundles()
	{
		DirectoryInfo directory = new DirectoryInfo(Application.dataPath + "/TreeTiles");
		var fileInfo = directory.GetFiles();
		foreach (var file in fileInfo)
		{
			if (!file.Name.Contains(".meta"))
			{
				Debug.Log("Exporting tile: " + file.Name);
				//Create asset bundle from mesh we just made
				AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
				string[] assetNames = new string[1];
				assetNames[0] = "Assets/TreeTiles/" + file.Name;

				buildMap[0].assetBundleName = file.Name.Replace(".asset", "");
				buildMap[0].assetNames = assetNames;

				BuildPipeline.BuildAssetBundles("Assets/TreeTilesAssetBundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.WebGL);
			}
		}
	}
}
