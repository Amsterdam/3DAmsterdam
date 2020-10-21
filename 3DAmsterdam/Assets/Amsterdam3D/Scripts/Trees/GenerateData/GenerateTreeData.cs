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

	[SerializeField]
	private List<Tree> trees;

	private List<string> treeLines;

	[SerializeField]
	private Material previewMaterial;
	[SerializeField]
	private Material treesMaterial;

	private double tileSize = 1000.0;

	public void Start()
	{
		trees = new List<Tree>();
		treeLines = new List<string>();

		ParseTreeData();
	}

	private void ParseTreeData()
	{
		foreach (var csvData in bomenCsvDataFiles)
		{
			string[] lines = csvData.text.Split('\n');
			for (int i = 1; i < lines.Length; i++)
			{
				if (lines[i].Contains(";"))
				{
					treeLines.Add(lines[i]);
				}
			}
		}
		Debug.Log("Tree dataset contains " + treeLines.Count);

		StartCoroutine(ParseTreeLines());
	}

	IEnumerator ParseTreeLines()
	{
		var lineNr = 0;
		Debug.Log("Started parsing tree dataset..");
		yield return new WaitForEndOfFrame();

		while (lineNr < treeLines.Count)
		{
			ParseTree(treeLines[lineNr]);
			lineNr++;
			if (lineNr % 10000 == 0)
			{
				Debug.Log("Parsing tree line nr: " + lineNr);
				yield return new WaitForEndOfFrame();
			}
		}
		Debug.Log("Done parsing tree lines. Start filling the tiles with trees..");
		TraverseTileFiles();

		yield return null;
	}

	/// <summary>
	/// Parse a tree from a string line to a new List item containing the following ; seperated fields:
	/// OBJECTNUMMER;Soortnaam_NL;Boomnummer;Soortnaam_WTS;Boomtype;Boomhoogte;Plantjaar;Eigenaar;Beheerder;Categorie;SOORT_KORT;SDVIEW;RADIUS;WKT_LNG_LAT;WKT_LAT_LNG;LNG;LAT;
	/// </summary>
	/// <param name="line">Text line matching the same fields as the header</param>
	private void ParseTree(string line)
	{
		string[] cell = line.Split(';');

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

		//Extra generated tree data
		newTree.RD = CoordConvert.WGS84toRD(newTree.LNG, newTree.LAT);
		newTree.position = CoordConvert.WGS84toUnity(newTree.LNG, newTree.LAT);
		newTree.averageTreeHeight = EstimateTreeHeight(newTree.Boomhoogte);
		newTree.prefab = FindClosestPrefabTypeByName(newTree.Soortnaam_NL);

		trees.Add(newTree);
	}

	/// <summary>
	/// Find a prefab in our list of tree prefabs that has a substring matching a part of our prefab name.
	/// Make sure prefab names are unique to get unique results.
	/// </summary>
	/// <param name="treeTypeDescription">The string containing the tree type word</param>
	/// <returns>The prefab with a matching substring</returns>
	private GameObject FindClosestPrefabTypeByName(string treeTypeDescription)
	{
		string[] treeNameParts = treeTypeDescription.Replace("\"","").Split(' ');
		string treeTypeName = treeNameParts[0].ToLower();

		foreach (var namePart in treeNameParts)
		{
			foreach (GameObject tree in treeTypes.items)
			{
				if (tree.name.ToLower().Contains(treeTypeName))
				{
					return tree;
				}
			}
		}
		return treeTypes.items[3];
	}

	/// <summary>
	/// Estimate the tree height according to the height description.
	/// We try to parse every number found, and use the average.
	/// </summary>
	/// <param name="description">For example: "6 to 8 m"</param>
	/// <returns></returns>
	private float EstimateTreeHeight(string description)
	{
		float treeHeight = 10.0f;

		string[] numbers = description.Split(' ');
		int numbersFoundInString = 0;
		float averageHeight = 0;
		foreach(string nr in numbers)
		{
			float parsedNumber = 10;

			if (float.TryParse(nr, out parsedNumber))
			{
				numbersFoundInString++;
				averageHeight += parsedNumber;
			}
		}
		if(numbersFoundInString > 0){
			treeHeight = averageHeight / numbersFoundInString;
		}

		return treeHeight;
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
				//Debug.Log("Placing tile at RD coordinates:" + coordinates[0] + "," + coordinates[1]);
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
				newTile.transform.position = CoordConvert.RDtoUnity(tileRDCoordinatesBottomLeft);

				GameObject treeRoot = new GameObject();
				treeRoot.name = file.Name.Replace("terrain","trees");
				treeRoot.transform.position = newTile.transform.position;

				StartCoroutine(SpawnTreesInTile(treeRoot, tileRDCoordinatesBottomLeft));
			}
		}
	}

	private IEnumerator SpawnTreesInTile(GameObject treeTile, Vector3RD tileCoordinates)
	{
		//TODO: Add all trees within this time (1x1km)
		yield return new WaitForEndOfFrame(); //make sure collider is there

		int treeChecked = 0;

		while(treeChecked < trees.Count-1){
			Tree tree = trees[treeChecked];

			//Debug.Log("Checking if tree with coordinates " + tree.RD.x + ", " + tree.RD.y + " is within tile coordinates " + tileCoordinates.x + " " + tileCoordinates.y);
			if (tree.RD.x > tileCoordinates.x && tree.RD.y > tileCoordinates.y && tree.RD.x < tileCoordinates.x + tileSize && tree.RD.y < tileCoordinates.y + tileSize)
			{
				GameObject newTreeInstance = Instantiate(tree.prefab, treeTile.transform);
				newTreeInstance.transform.localScale = Vector3.one * 0.1f * tree.averageTreeHeight;

				float raycastHitY = Constants.ZERO_GROUND_LEVEL_Y;
				if (Physics.Raycast(tree.position + Vector3.up*1000.0f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
				{
					raycastHitY = hit.point.y;
				}
				newTreeInstance.transform.position = new Vector3(tree.position.x, raycastHitY, tree.position.z);
				//Debug.Log("Tree placed with coordinates " + tree.RD.x + ", " + tree.RD.y + " in tile coordinates " + tileCoordinates.x + " " + tileCoordinates.y);
			}
			treeChecked++;
		}

		Vector3 worldPosition = treeTile.transform.position;
		
		//Calculate offset
		Vector3RD convertedOffset = CoordConvert.UnitytoRD(Vector3.zero);
		convertedOffset.x -= 500;
		convertedOffset.y -= 500;
		convertedOffset.z -= 43;
		treeTile.transform.position = CoordConvert.RDtoUnity(convertedOffset);

		//Combine child meshes in a new single mesh
		MeshFilter[] meshFilters = treeTile.GetComponentsInChildren<MeshFilter>();
		string assetName = "Assets/TreeTiles/" + treeTile.name + ".asset";
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		for (int i = 0; i < combine.Length; i++)
		{
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			combine[i].mesh = meshFilters[i].sharedMesh;
			meshFilters[i].gameObject.SetActive(false);
		}
		Mesh newCombinedMesh = new Mesh();
		if (meshFilters.Length > 0)
		{
			newCombinedMesh.name = treeTile.name;
			newCombinedMesh.CombineMeshes(combine,true);
		}

		treeTile.AddComponent<MeshFilter>().sharedMesh = newCombinedMesh;
		treeTile.AddComponent<MeshRenderer>().material = treesMaterial;
		#if UNITY_EDITOR
		AssetDatabase.CreateAsset(newCombinedMesh, assetName);
		AssetDatabase.SaveAssets();
		#endif

		treeTile.transform.position = worldPosition;

		yield return null;
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
		public float averageTreeHeight;

		public GameObject prefab;
	}
}
