using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Netherlands3D.Core;
using System.Linq;

namespace Netherlands3D.AssetGeneration
{
	public class GenerateTreeTiles : MonoBehaviour
	{
		[Serializable]
		private class Tree
		{
			public string id;
			public string typeName;
			public string treeHeight;
			public int plantedYear;
			public string radius;
			public double lng;
			public double lat;

			public Vector3RD RD;
			public Vector3 position;
			public float averageTreeRootOriginHeight;

			public GameObject prefab;
		}

		[Serializable]
		private class CsvFieldNameMapping
		{
			public string id = "OBJECTNUMMER";
			[HideInInspector]
			public int id_Index = -1;

			public string typeName = "Soortnaam_NL";
			[HideInInspector]
			public int typeName_Index = -1;

			public string treeHeight = "Boomhoogte";
			[HideInInspector]
			public int treeHeight_Index = -1;

			public string plantedYear = "Plantjaar";
			[HideInInspector]
			public int plantedYear_Index = -1;

			public string radius = "RADIUS";
			[HideInInspector]
			public int radius_Index = -1;

			public string lng = "LNG";
			[HideInInspector]
			public int lng_Index = -1;

			public string lat = "LAT";
			[HideInInspector]
			public int lat_Index = -1;
		}

		[SerializeField]
		private CsvFieldNameMapping fieldNameMapping;

		private const float raycastYRandomOffsetRange = 0.08f;
		[SerializeField]
		private GameObjectsGroup treeTypes;

		[SerializeField]
		private TextAsset[] bomenCsvDataFiles;

		private List<Tree> trees;

		private List<string> treeLines;

		[SerializeField]
		private Material previewMaterial;
		[SerializeField]
		private Material treesMaterial;

		private double tileSize = 1000.0;
		public string sourceGroundTilesFolder = "C:/Projects/GemeenteAmsterdam/1x1kmGroundTiles";

		private string[] treeNameParts;
		private string treeTypeName = "";

		[SerializeField]
		private List<string> noPrefabFoundNames;

		private Vector3RD tileOffset;
		private Vector3 unityTileOffset; 

		public void Start()
		{
			//Calculate offset. ( Our viewer expects tiles with the origin in the center )
			tileOffset = new Vector3RD()
            {
                x = Config.activeConfiguration.RelativeCenterRD.x,
                y = Config.activeConfiguration.RelativeCenterRD.y,
                z = 0
            };
			tileOffset.x -= 500;
            tileOffset.y -= 500;
			unityTileOffset = CoordConvert.RDtoUnity(tileOffset);
			trees = new List<Tree>();
			treeLines = new List<string>();
			noPrefabFoundNames = new List<string>();
			ParseTreeData();
		}

		/// <summary>
		/// Put all the csv lines from csv files in one big array, before parsing them all.
		/// </summary>
		private void ParseTreeData()
		{
			foreach (var csvData in bomenCsvDataFiles)
			{
				string[] lines = csvData.text.Split('\n');

				ReadHeader(lines[0]);

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

		/// <summary>
		/// Determine at what location the fieldnames are
		/// </summary>
		/// <param name="header">The top line of the csv containing the field names</param>
		private void ReadHeader(string header)
		{
			string[] headerFields = header.Split(';');
			for (int i = 0; i < headerFields.Length; i++)
			{
				var fieldName = headerFields[i];
				if(fieldName == fieldNameMapping.id){
					fieldNameMapping.id_Index = i;
				}
				else if (fieldName == fieldNameMapping.lat)
				{
					fieldNameMapping.lat_Index = i;
				}
				else if (fieldName == fieldNameMapping.lng)
				{
					fieldNameMapping.lng_Index = i;
				}
				else if (fieldName == fieldNameMapping.plantedYear)
				{
					fieldNameMapping.plantedYear_Index = i;
				}
				else if (fieldName == fieldNameMapping.radius)
				{
					fieldNameMapping.radius_Index = i;
				}
				else if (fieldName == fieldNameMapping.treeHeight)
				{
					fieldNameMapping.treeHeight_Index = i;
				}
				else if (fieldName == fieldNameMapping.typeName)
				{
					fieldNameMapping.typeName_Index = i;
				}
			}
		}

		/// <summary>
		/// Parse all the CSV lines found within the .csv files and fill our list containing all the trees
		/// </summary>
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
					Debug.Log("Parsing tree line nr: " + lineNr + "/" + treeLines.Count);
					yield return new WaitForEndOfFrame();
				}
			}

			Debug.Log("No prefabs were found for the following tree names: ");
			string listOfNamesNotFound = string.Join("\n", noPrefabFoundNames);
			var treesNotFoundList = Application.dataPath + "/TreesNotFound.csv";
			File.WriteAllText(treesNotFoundList, listOfNamesNotFound);
			Debug.Log($"A list of trees that were not found is saved in {treesNotFoundList}");

			Debug.Log("Done parsing tree lines. Start filling the tiles with trees..");

			StartCoroutine(TraverseTileFiles());

			yield return null;
		}

		/// <summary>
		/// Parse a tree from a string line to a new List item. Make sure to check your csv file header field names, and the ; as seperator character.
		/// </summary>
		/// <param name="line">Text line matching the same fields as the header</param>
		private void ParseTree(string line)
		{
			string[] cell = line.Split(';');

			Tree newTree = new Tree()
			{
				id = cell[fieldNameMapping.id_Index],
				typeName = cell[fieldNameMapping.typeName_Index],
				treeHeight = cell[fieldNameMapping.treeHeight_Index],	
				radius = cell[fieldNameMapping.radius_Index],
				lng = double.Parse(cell[fieldNameMapping.lng_Index], System.Globalization.CultureInfo.InvariantCulture),
				lat = double.Parse(cell[fieldNameMapping.lat_Index], System.Globalization.CultureInfo.InvariantCulture)
			};

			//Extra generated tree data
			newTree.RD = CoordConvert.WGS84toRD(newTree.lng, newTree.lat);
			newTree.position = CoordConvert.WGS84toUnity(newTree.lng, newTree.lat);
			newTree.averageTreeRootOriginHeight = EstimateTreeHeight(newTree.treeHeight);
			newTree.prefab = FindClosestPrefabTypeByName(newTree.typeName);

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
			treeNameParts = treeTypeDescription.Replace("\"", "").Split(' ');

			foreach (var namePart in treeNameParts)
			{
				treeTypeName = namePart.ToLower();
				foreach (GameObject tree in treeTypes.items)
				{
					if (tree.name.ToLower().Contains(treeTypeName))
					{
						return tree;
					}
				}
			}
			noPrefabFoundNames.Add(treeTypeDescription);
			return treeTypes.items[3]; //Just use an average tree prefab as default
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
			foreach (string nr in numbers)
			{
				float parsedNumber = 10;

				if (float.TryParse(nr, out parsedNumber))
				{
					numbersFoundInString++;
					averageHeight += parsedNumber;
				}
			}
			if (numbersFoundInString > 0)
			{
				treeHeight = averageHeight / numbersFoundInString;
			}

			return treeHeight;
		}

		/// <summary>
		/// Load all the large ground tiles from AssetBundles, spawn it in our world, and start filling it with the trees that match the tile
		/// its RD coordinate rectangle. The tiles are named after the RD coordinates in origin at the bottomleft of the tile.
		/// </summary>
		private IEnumerator TraverseTileFiles()
		{
			var info = new DirectoryInfo(sourceGroundTilesFolder);
			var fileInfo = info.GetFiles();

			//Initialize preview progress map
			var xTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.x - (float)Config.activeConfiguration.BottomLeftRD.x) / (float)tileSize);
			var yTiles = Mathf.RoundToInt(((float)Config.activeConfiguration.TopRightRD.y - (float)Config.activeConfiguration.BottomLeftRD.y) / (float)tileSize);
			yield return ProgressPreviewMap.Instance.Initialize(xTiles, yTiles);

			var currentFile = 0;
			while(currentFile < fileInfo.Length)
			{
				FileInfo file = fileInfo[currentFile];
				if (!file.Name.Contains(".manifest") && file.Name.Contains("_"))
				{
					Debug.Log("Filling tile " + currentFile + "/" + fileInfo.Length);
					yield return new WaitForEndOfFrame();
					string filename = file.Name;
					filename = filename.Replace("terrain_", "");
					string[] coordinates = filename.Split('-');
					
					Vector3RD tileRDCoordinatesBottomLeft = new Vector3RD(double.Parse(coordinates[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(coordinates[1], System.Globalization.CultureInfo.InvariantCulture), 0);
					Vector3RD tileCenter = new Vector3RD(tileRDCoordinatesBottomLeft.x+500, tileRDCoordinatesBottomLeft.y+500, tileRDCoordinatesBottomLeft.z);
					AssetBundle assetBundleTerrainTile = AssetBundle.LoadFromFile(file.FullName);
					Mesh[] meshesInAssetbundle = new Mesh[0];
					try
					{
						meshesInAssetbundle = assetBundleTerrainTile.LoadAllAssets<Mesh>();
					}
					catch (Exception)
					{
						Debug.Log("Could not find a mesh in this assetbundle.");
						assetBundleTerrainTile.Unload(true);
					}

					//Spawn a new gameobject in our scene for the tile with a meshcollider
					GameObject terrainTile = new GameObject();
					terrainTile.name = file.Name;
					var mesh = meshesInAssetbundle[0];
					var terrainTileMeshFilter = terrainTile.AddComponent<MeshFilter>();
					terrainTileMeshFilter.sharedMesh = mesh;

					//Collision meshes can only be made on meshes with more then 3 distict vertices
					if (mesh.vertices.Distinct().Count() >= 3)
					{
						terrainTile.AddComponent<MeshCollider>().sharedMesh = mesh;
					}

					MeshRenderer tileRenderer = terrainTile.AddComponent<MeshRenderer>();
					Material[] materials = new Material[mesh.subMeshCount];
					for (int i = 0; i < mesh.subMeshCount; i++)
					{
						materials[i] = previewMaterial;
					}
					tileRenderer.materials = materials;
					terrainTile.transform.position = CoordConvert.RDtoUnity(tileCenter);

					//Spawn a new container for our trees that lines up with the tile
					GameObject treeRoot = new GameObject();
					treeRoot.name = file.Name.Replace("terrain", "trees");
					treeRoot.transform.position = terrainTile.transform.position;

					yield return new WaitForEndOfFrame(); //Make sure collider is processed

					SpawnTreesInTile(treeRoot, tileRDCoordinatesBottomLeft);

					//Clean up our terrain tile to clear memory/physX objects
					assetBundleTerrainTile.Unload(true);
					Destroy(mesh);
					Destroy(terrainTile);

				}
				currentFile++;
			}
		}

		/// <summary>
		/// Spawn all the trees located within the RD coordinate bounds of the 1x1km tile.
		/// </summary>
		/// <param name="treeTile">The target tree tile container that lines up with the terrain tile</param>
		/// <param name="tileCoordinates">RD Coordinates of the tile</param>
		/// <returns></returns>
		private void SpawnTreesInTile(GameObject treeTile, Vector3RD tileCoordinates)
		{
			//TODO: Add all trees within this tile (1x1km)
			int treeChecked = trees.Count -1;
			int treesAddedToTile = 0;
			while (treeChecked >= 0)
			{
				Tree tree = trees[treeChecked];
				if (tree.RD.x > tileCoordinates.x && tree.RD.y > tileCoordinates.y && tree.RD.x < tileCoordinates.x + tileSize && tree.RD.y < tileCoordinates.y + tileSize)
				{
					SpawnTreeOnGround(treeTile, tree);
					treesAddedToTile++;
					trees.RemoveAt(treeChecked);
				}
				treeChecked--;
			}

			//Update our preview image
			var xPreviewCoordinate = (tileCoordinates.x - Config.activeConfiguration.BottomLeftRD.x) / tileSize;
			var yPreviewCoordinate = (tileCoordinates.y - Config.activeConfiguration.BottomLeftRD.y) / tileSize;
			ProgressPreviewMap.Instance.ColorTile((int)xPreviewCoordinate, (int)yPreviewCoordinate, (treesAddedToTile>0) ? TilePreviewState.DONE : TilePreviewState.EMPTY);

			//Define a preview position to preview the tree tile in our scene
			Vector3 previewPosition = treeTile.transform.position;
			treeTile.transform.position = Vector3.zero;

			CreateTreeTile(treeTile, previewPosition);
		}

		/// <summary>
		/// Spawn a new tree object matching the tree data properties.
		/// </summary>
		/// <param name="treeTile">The root parent for the new tree</param>
		/// <param name="tree">The tree data object containing our tree properties</param>
		private void SpawnTreeOnGround(GameObject treeTile, Tree tree)
		{
			GameObject newTreeInstance = Instantiate(tree.prefab, treeTile.transform);

			//Apply properties/variations based on tree data
			newTreeInstance.name = tree.id;
			newTreeInstance.transform.localScale = Vector3.one * 0.1f * tree.averageTreeRootOriginHeight;
			newTreeInstance.transform.Rotate(0, UnityEngine.Random.value * 360.0f, 0);

			float raycastHitY = 0;
			if (Physics.Raycast(tree.position + Vector3.up * 1000.0f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
			{
				raycastHitY = hit.point.y;
			}

			//Add a little random variation in our hitpoint, to avoid z-fighting on the same trees that are intersecting with eachother
			raycastHitY -= UnityEngine.Random.value * raycastYRandomOffsetRange;
			
			newTreeInstance.transform.position = new Vector3(tree.position.x, raycastHitY, tree.position.z);
		}

		/// <summary>
		/// Get all the child meshes of the tile, and merge them into one big tile mesh.
		/// </summary>
		/// <param name="treeTile">The root parent containing all our spawned trees</param>
		/// <param name="worldPosition">The position to move the tile to when it is done (for previewing purposes)</param>
		private void CreateTreeTile(GameObject treeTile, Vector3 worldPosition)
		{
			TileCombineUtility.CreateAssetFolder();
			string assetName = TileCombineUtility.unityMeshAssetFolder + treeTile.name + ".asset";

			MeshFilter[] meshFilters = treeTile.GetComponentsInChildren<MeshFilter>();
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];

			var totalVertexCount = 0;
			for (int i = 0; i < combine.Length; i++)
			{
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

				Mesh treeMesh = meshFilters[i].mesh;						
				if (treeMesh.vertexCount > 0)
				{
					totalVertexCount += treeMesh.vertexCount;
					AddIDToMeshUV(treeMesh, int.Parse(meshFilters[i].name));
				}
				combine[i].mesh = treeMesh;
				meshFilters[i].gameObject.SetActive(false);
			}

			Mesh newCombinedMesh = new Mesh();
			if (totalVertexCount > 65536) //In case we go over the 16bit ( 2^16 ) index count, increase the indexformat.
				newCombinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

			if (meshFilters.Length > 0)
			{
				newCombinedMesh.name = treeTile.name;
				newCombinedMesh.CombineMeshes(combine, true);
			}

			treeTile.AddComponent<MeshFilter>().sharedMesh = newCombinedMesh;
			treeTile.AddComponent<MeshRenderer>().material = treesMaterial;
#if UNITY_EDITOR
			AssetDatabase.CreateAsset(newCombinedMesh, assetName);
			AssetDatabase.SaveAssets();
#endif

			treeTile.transform.position = worldPosition;
		}

		/// <summary>
		/// Adds a specific number to a mesh UV slot for all the verts
		/// </summary>
		/// <param name="treeMesh">The mesh to assign the ID to</param>
		/// <param name="objectNumber">The number to inject into the UV slot</param>
		private void AddIDToMeshUV(Mesh treeMesh, float objectNumber)
		{
			treeMesh.uv3 = new Vector2[treeMesh.vertexCount];
			Vector2 uvIds = new Vector2() { x = objectNumber, y = 0 };
			for (int j = 0; j < treeMesh.uv3.Length; j++)
			{
				treeMesh.uv3[j] = uvIds;
			}
		}
	}
}