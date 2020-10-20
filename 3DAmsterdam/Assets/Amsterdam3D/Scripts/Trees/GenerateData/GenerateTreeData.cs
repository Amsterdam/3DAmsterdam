using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using ConvertCoordinates;

public class GenerateTreeData : MonoBehaviour
{
	[SerializeField]
	private GameObjectsGroup treeTypes;

	[SerializeField]
	private TextAsset[] bomenCsvDataFiles;

	private List<Tree> trees;

	[SerializeField]
	private Material previewMaterial;
	private double tileSize = 1000.0;

	public void Start()
	{
		trees = new List<Tree>();
		ParseTreeData();
		TraverseTileFiles();
	}

	private void ParseTreeData()
	{
		foreach (var csvData in bomenCsvDataFiles)
		{
			string[] lines = csvData.text.Split('\n');
			Debug.Log("Parsing " + lines.Length + " trees from " + csvData.name);
			for (int i = 1; i < lines.Length; i++)
			{
				if(lines[i].Contains(";"))
					ParseTree(lines[i]);
			}
		}

		Debug.Log(trees.Count + " trees in database");
	}

	private void ParseTree(string line)
	{
		string[] cell = line.Split(';');

		//A comma seperated file has the following values:
		//OBJECTNUMMER;Soortnaam_NL;Boomnummer;Soortnaam_WTS;Boomtype;Boomhoogte;Plantjaar;
		//Eigenaar;Beheerder;Categorie;SOORT_KORT;SDVIEW;RADIUS;WKT_LNG_LAT;WKT_LAT_LNG;LNG;LAT;
		Tree newTree = new Tree()
		{
			OBJECTNUMMER = int.Parse(cell[0]),
			Soortnaam_NL = cell[1],
			Boomnummer = cell[2],
			Soortnaam_WTS = cell[3],
			Boomtype = cell[4],
			Boomhoogte = cell[5],
			Plantjaar = int.Parse(cell[6]),
			Eigenaar = cell[7],
			Beheerder = cell[8],
			Categorie = cell[9],
			SOORT_KORT = cell[10],
			SDVIEW = cell[11],
			RADIUS = cell[12],
			WKT_LNG_LAT = cell[13],
			WKT_LAT_LNG = cell[14],
			LNG = double.Parse(cell[15]),
			LAT = double.Parse(cell[16])
		};

		newTree.RD = CoordConvert.WGS84toRD(newTree.LNG, newTree.LAT);
		newTree.position = CoordConvert.WGS84toUnity(newTree.LNG, newTree.LAT);

		trees.Add(newTree);
	}

	private void TraverseTileFiles()
	{
		var info = new DirectoryInfo("C:/Users/Sam/Desktop/1x1kmTiles");
		var fileInfo = info.GetFiles();
		foreach (var file in fileInfo)
		{
			Debug.Log(file.Name);
			if (!file.Name.Contains(".manifest") && file.Name.Contains("_"))
			{
				string[] coordinates = file.Name.Split('_');
				Debug.Log("Placing tile at RD coordinates:" + coordinates[0] + "," + coordinates[1]);

				//Tile name coordinates are bottom left, but origin is at center of tile, so we add 500m
				Vector3RD tileRDCoordinatesBottomLeft = new Vector3RD(double.Parse(coordinates[0]), double.Parse(coordinates[1]), 0);
				Vector3RD tileRDCoordinatesCenter = new Vector3RD(tileRDCoordinatesBottomLeft.x + tileSize/2.0, tileRDCoordinatesBottomLeft.y + tileSize/2.0, 0);

				var assetBundleTile = AssetBundle.LoadFromFile(file.FullName);				
				Mesh[] meshesInAssetbundle = new Mesh[0];
				try
				{
					meshesInAssetbundle = assetBundleTile.LoadAllAssets<Mesh>();
				}
				catch (Exception)
				{
					Debug.Log("Couldnt parse asset");
					assetBundleTile.Unload(true);
				}


				GameObject newTile = new GameObject();
				newTile.name = file.Name;
				newTile.AddComponent<MeshFilter>().sharedMesh = meshesInAssetbundle[0];
				newTile.AddComponent<MeshCollider>().sharedMesh = meshesInAssetbundle[0];
				newTile.AddComponent<MeshRenderer>().material = previewMaterial;
				newTile.transform.position = CoordConvert.RDtoUnity(tileRDCoordinatesCenter);
	
				StartCoroutine(SpawnTreesInTile(newTile, tileRDCoordinatesBottomLeft));
			}
		}
	}

	private IEnumerator SpawnTreesInTile(GameObject parentTile, Vector3RD tileCoordinates)
	{
		//TODO: Add all trees within this time (1x1km)
		int treeChecked = 0;
		while(treeChecked < trees.Count-1){
			Tree tree = trees[treeChecked];

			//Debug.Log("Checking if tree with coordinates " + tree.RD.x + ", " + tree.RD.y + " is within tile coordinates " + tileCoordinates.x + " " + tileCoordinates.y);

			if (tree.RD.x > tileCoordinates.x && tree.RD.y > tileCoordinates.y && tree.RD.x < tileCoordinates.x + tileSize && tree.RD.y < tileCoordinates.y + tileSize)
			{
				GameObject newTreeInstance = Instantiate(treeTypes.items[0], parentTile.transform);
				float raycastHitY = Constants.ZERO_GROUND_LEVEL_Y;
				if (Physics.Raycast(tree.position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
				{
					raycastHitY = hit.point.y;
				}
				newTreeInstance.transform.position = new Vector3(tree.position.x, raycastHitY, tree.position.z);

				Debug.Log("Tree placed with coordinates " + tree.RD.x + ", " + tree.RD.y + " in tile coordinates " + tileCoordinates.x + " " + tileCoordinates.y);

			}
			treeChecked++;
			yield return null;
		}
	}

	[Serializable]
	private class Tree
	{
		public int OBJECTNUMMER;
		public string Soortnaam_NL; 
		public string Boomnummer; 
		public string Soortnaam_WTS; 
		public string Boomtype; 
		public string Boomhoogte; 
		public int Plantjaar; 
		public string Eigenaar; 
		public string Beheerder; 
		public string Categorie; 
		public string SOORT_KORT; 
		public string SDVIEW; 
		public string RADIUS; 
		public string WKT_LNG_LAT; 
		public string WKT_LAT_LNG; 
		public double LNG; 
		public double LAT;

		public Vector3RD RD;
		public Vector3 position;

		public GameObject prefab;
	}
}
