#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Netherlands3D.Core;
using System;
using Netherlands3D;

namespace Netherlands3D.AssetGeneration
{
	/// <summary>
	/// Asset generation utility class
	/// Methods to load a view or correct asset files
	/// </summary>
	public class TileAssetTester : MonoBehaviour
	{
		private class Tree
		{
			public string OBJECTNUMMER;
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

		public GameObject TreeObject;

		public string CsvFile;

		public Material BuildingMaterial;
		public Material TreeMaterial;
		public Material TerrainMaterial;

		GameObject _yaml;
		GameObject _fs;
		GameObject _terrain;
		GameObject _trees;
		GameObject _buildings;

		public List<Material> _terrainMaterials;

		Vector3 offset = Vector3.zero;

		private List<Tree> trees;

		string assetCreationRelativePath = "Assets/GeneratedTileAssets";


		void Start()
		{
			if (Config.activeConfiguration == null)
			{
				throw new Exception("No ApplicationConfuguration is set in the scene");
			}

			trees = new List<Tree>();

			_yaml = new GameObject("yaml");
			_yaml.transform.parent = transform;

			_fs = new GameObject("fs");
			_fs.transform.parent = transform;

			_terrain = new GameObject("terrain");
			_terrain.transform.parent = transform;

			_trees = new GameObject("trees");
			_trees.transform.parent = transform;

			_buildings = new GameObject("buildings");
			_buildings.transform.parent = transform;


			//StartCoroutine(GetTilesUnityFS(@"F:\Files\Assetbundles\terrain.lod1", true, _terrain.transform, TerrainMaterial, _terrainMaterials.ToArray()));

			//StartCoroutine(GetTilesUnityFSAndCorrectHeightSpikes(@"F:\Files\Assetbundles\terrain.lod1", _terrain.transform, TerrainMaterial, _terrainMaterials.ToArray()));
			StartCoroutine(GetTilesYAML(assetCreationRelativePath, _terrainMaterials.ToArray(), "*.mesh", _terrain.transform));

			//GetTilesUnityFS(@"F:\Files\Assetbundles\trees", false, _fs.transform, TreeMaterial, null);

			//GetTilesYAML("TreeTileAssets", TreeMaterial);

			//List<Material> mats = new List<Material>();
			//mats.Add(BuildingMaterial);

			// StartCoroutine(GetTilesYAML("Buildings", mats.ToArray(), "*.mesh", _buildings.transform));

			//StartCoroutine( GetTilesYAML(@"terrainMeshes\LOD0", _terrainMaterials.ToArray(), "*.mesh", _terrain.transform));

			//ReadTreesFromCsv();

			//DrawTrees();


		}


		IEnumerator GetTilesUnityFSAndCorrectHeightSpikes(string path, Transform parent, Material material, Material[] materials)
		{
			Directory.CreateDirectory(assetCreationRelativePath);

			int count = 0;
			var files = Directory.GetFiles(path).Where(o => !o.Contains(".manifest")).ToArray();

			int heightMax = 15; //vertices above this value are considered to be spikes, vlaue in meters
			int heightMin = -15; //vertices above this value are considered to be spikes, vlaue in meters            
			int lookaroundWidth = 1; //area in meters to look around for other vertices to find common height

			foreach (var file in files)
			{
				var finfo = new FileInfo(file);

				if (File.Exists(Path.Combine(assetCreationRelativePath, finfo.Name)))
				{
					continue;
				}

				//if (!file.Contains("140000-457000")) continue;               

				if (!file.Contains('-')) continue;

				yield return null;
				Debug.Log($"going to process: {finfo.Name}");

				var rd = file.GetRDCoordinate();
				var tilepos = CoordConvert.RDtoUnity(rd);

				var assetbundle = AssetBundle.LoadFromFile(file);

				var mesh = assetbundle.LoadAllAssets<Mesh>().First();

				if (mesh.vertices.Length == 0) continue;

				var verts = mesh.vertices;

				var correctverts = verts.Where(o => o.y < heightMax && o.y > heightMin);
				var correctvertsAvgHeight = correctverts.Average(o => o.y);

				bool hasspike = false;
				for (int i = 0; i < verts.Length; i++)
				{
					if (verts[i].y > heightMax || verts[i].y < heightMin)
					{
						hasspike = true;
						verts[i].y = correctvertsAvgHeight; //for now just use the average height

						//experimental code, needs further testing
						//var x = verts[i].x;
						//var z = verts[i].z;
						//var vertsaround = correctverts.Where(o => o.x < x + lookaroundWidth
						//								&& o.x > x - lookaroundWidth
						//								&& o.z < z + lookaroundWidth
						//								&& o.z > z - lookaroundWidth);
						//if (vertsaround.Any())
						//{
						//	var avgh = vertsaround.Max(o => o.y);
						//	verts[i].y = avgh;
						//}
						//else
						//{
						//	verts[i].y = correctvertsAvgHeight;
						//}
					}
				}

				if (hasspike) mesh.vertices = verts;

				if (!hasspike)
				{
					yield return null;
					Debug.Log($"no spike in: {finfo.Name}");
					continue;
				}

				GameObject gam = new GameObject();
				gam.name = finfo.Name;
				gam.transform.parent = parent;
				gam.transform.position = tilepos + offset;
				gam.AddComponent<MeshFilter>().sharedMesh = mesh;

				gam.AddComponent<MeshRenderer>().material = material;

				if (materials != null)
				{
					var ren = gam.GetComponent<MeshRenderer>();
					ren.materials = materials;
				}

				yield return null;

				var newmesh = Instantiate(mesh);
				newmesh.name = finfo.Name;

				AssetBundle.UnloadAllAssetBundles(true);
				AssetDatabase.CreateAsset(newmesh, Path.Combine(assetCreationRelativePath, finfo.Name + ".mesh"));

				count++;
				Debug.Log($"processed {count}");
				//if (count > 1) break;
			}

			Debug.Log("All tiles processed");

		}

		IEnumerator GetTilesUnityFS(string path, bool addCollider, Transform parent, Material material, Material[] materials)
		{
			var files = Directory.GetFiles(path).Where(o => !o.Contains(".manifest")).ToArray();

			foreach (var file in files)
			{
				var finfo = new FileInfo(file);

				if (!file.Contains('-')) continue;

				yield return null;
				Debug.Log($"going to process: {finfo.Name}");

				var rd = file.GetRDCoordinate();
				var tilepos = CoordConvert.RDtoUnity(rd);

				var assetbundle = AssetBundle.LoadFromFile(file);

				var mesh = assetbundle.LoadAllAssets<Mesh>().First();

				if (mesh.vertices.Length == 0) continue;

				var verts = mesh.vertices;

				GameObject gam = new GameObject();
				gam.name = finfo.Name;
				gam.transform.parent = parent;
				gam.transform.position = tilepos + offset;
				gam.AddComponent<MeshFilter>().sharedMesh = mesh;

				if (addCollider) gam.AddComponent<MeshCollider>().sharedMesh = mesh;
				gam.AddComponent<MeshRenderer>().material = material;

				if (materials != null)
				{
					var ren = gam.GetComponent<MeshRenderer>();
					ren.materials = materials;
				}

				yield return null;

				var newmesh = Instantiate(mesh);
				newmesh.name = finfo.Name;

				AssetBundle.UnloadAllAssetBundles(true);
				AssetDatabase.CreateAsset(newmesh, Path.Combine(assetCreationRelativePath, finfo.Name));

				Debug.Log($"loaded {finfo.Name}");
			}

			Debug.Log("All tiles processed");

		}

		IEnumerator GetTilesYAML(string dirname, Material[] materials, string searchfilter, Transform parent)
		{
			var files = Directory.GetFiles(dirname, searchfilter);

			foreach (var file in files)
			{
				var finfo = new FileInfo(file);

				var rd = file.GetRDCoordinate();
				var tilepos = CoordConvert.RDtoUnity(rd);

				//var mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{dirname}/{finfo.Name}");
				var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(file);

				if (mesh == null || mesh.vertices.Length == 0) continue;

				GameObject gam = new GameObject();
				gam.name = finfo.Name;
				gam.transform.parent = parent;
				gam.transform.position = tilepos + offset;

				var filter = gam.AddComponent<MeshFilter>();
				filter.sharedMesh = mesh;

				var ren = gam.AddComponent<MeshRenderer>();
				//ren.material = material;
				ren.materials = materials;
				yield return null;
				Debug.Log($"loaded: {finfo.Name}");
			}
			yield return null;

		}



		void ReadTreesFromCsv()
		{
			Debug.Log("ReadTreesFromCsv");

			var lines = File.ReadAllLines(CsvFile);

			foreach (var line in lines.Skip(1))
			{
				try
				{
					var columns = line.Split(';');
					var tree = new Tree();

					tree.OBJECTNUMMER = columns[0];
					tree.Soortnaam_NL = columns[1];
					tree.Boomhoogte = columns[2];
					tree.Plantjaar = int.Parse(columns[3]);
					tree.RD = new Vector3RD(Convert.ToDouble(columns[5]), Convert.ToDouble(columns[6]), 0);

					//var longlat = ConvertToLatLong(tree.RD.x, tree.RD.y);

					tree.position = CoordConvert.RDtoUnity(tree.RD);
					//tree.position = CoordConvert.WGS84toUnity(longlat.longitude, longlat.latitude);


					//tree.averageTreeHeight = EstimateTreeHeight(tree.Boomhoogte);
					//tree.prefab = FindClosestPrefabTypeByName(tree.Soortnaam_NL);
					//tree.prefab = TestCube;

					//ID
					//BOOMSOOR02
					//BOOMHOOGTE
					//BOUWJAAR
					//EIGENAAR_O
					//X_COORDINA
					//Y_COORDINA
					//LEEFTIJD
					//BOOMHOOG01

					trees.Add(tree);
				}
				catch
				{
				}
			}

			//398 soorten bomen
			var soorten = trees.GroupBy(o => o.Soortnaam_NL).ToArray();
			var hoogtrd = trees.GroupBy(o => o.Boomhoogte).ToArray();
			var oldestTree = trees.Min(o => o.Plantjaar);

			Debug.Log($"Aantal bomen:{trees.Count} soorten:{soorten.Length} Oudste boom:{oldestTree}");

			var minx = trees.Min(o => o.RD.x);
			var miny = trees.Min(o => o.RD.y);
			var maxx = trees.Max(o => o.RD.x);
			var maxy = trees.Max(o => o.RD.y);

			var avgHoogteMin = trees.Min(o => o.averageTreeHeight);
			var avgHoogteMax = trees.Max(o => o.averageTreeHeight);

			Debug.Log($"minx:{minx} maxx:{maxx} miny:{miny} maxy:{maxy}");

			//minx:126805.07 maxx:141827.31 miny:448979.02 maxy:461149.85

		}

		void DrawTrees()
		{
			Debug.Log("DrawTrees");

			foreach (var tree in trees)
			{
				tree.position += new Vector3(-500, 0, -500);

				//float raycastHitY = treeTile.transform.position.y;
				if (Physics.Raycast(tree.position + (Vector3.up * 1000.0f), Vector3.down, out RaycastHit hit))
				{
					var boom = Instantiate(TreeObject);
					boom.transform.position = hit.point;
					boom.transform.parent = _trees.transform;
					//Debug.Log($"raycastHitY:{hit.point.y}");
				}
				else
				{
					//throw new Exception("no raycasthit");
					Debug.Log("no raycasthit");
				}

			}
		}

	}

}
#endif