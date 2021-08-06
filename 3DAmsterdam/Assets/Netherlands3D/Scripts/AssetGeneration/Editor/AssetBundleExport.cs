/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
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